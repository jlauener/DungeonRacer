using MonoGame.Extended.Tiled;
using System;
using MonoPunk;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	enum DungeonTileType
	{
		Ground,
		Wall,
		Roof
	}

	enum TriggerType
	{
		None,
		Spike,
		Lava
	}

	enum ObjectType
	{
		Link,
		Info
	}

	class ObjectData
	{
		public static ObjectData CreateLink(Rectangle bounds, DungeonData target)
		{
			return new ObjectData(bounds, ObjectType.Link, target, null);
		}

		public static ObjectData CreateInfo(Rectangle bounds, string text)
		{
			return new ObjectData(bounds, ObjectType.Info, null, text);
		}

		public Rectangle Bounds { get; }
		public ObjectType Type { get; }
		public DungeonData Target { get; }
		public string Text { get; }
		public EntityData Loot { get; }

		private ObjectData(Rectangle bounds, ObjectType type, DungeonData target, string text)
		{
			Bounds = bounds;
			Type = type;
			Target = target;
			Text = text;
		}

		public override string ToString()
		{
			return "[ObjectData Bounds=" + Bounds + " Type=" + Type + " Target=" + Target +" Text='" + Text + "']";
		}
	}

	class DungeonTile
	{
		public int X { get; }
		public int Y { get; }
		public float ScreenX { get { return X * Global.TileSize; } }
		public float ScreenY { get { return Y * Global.TileSize; } }

		public int Id { get; }
		public Dictionary<string, string> Properties { get; }

		public int DisplayTid { get; set; } = -1;
		public DungeonTileType Type { get; set; }
		public TileSolidType SolidType { get; set; } = TileSolidType.None;
		public PixelMask PixelMask { get; set; }
		public string Trigger { get; set; }
		public string Anim { get; set; }

		public DungeonTile(int x, int y, int id, Dictionary<string, string> properties)
		{
			X = x;
			Y = y;
			Id = id;
			Properties = properties;
		}

		public override string ToString()
		{
			return "[DungeonTile X=" + X + " Y=" + Y + "]";
		}
	}

	class DungeonData
	{
		public string Name { get; }
		public int Width { get; }
		public int Height { get; }
		public int WidthTiles { get; }
		public int HeightTiles { get; }
		public Tileset Tileset { get; private set; }

		public DungeonTile PlayerStartTile { get; private set; }
		public Direction PlayerStartDirection { get; private set; }

		private readonly TiledMap map;
		private readonly DungeonTile[,] tiles;
		private readonly List<EntityArguments> entities = new List<EntityArguments>();
		private readonly List<ObjectData> objects = new List<ObjectData>();
		private readonly List<EntityData> groupRewards = new List<EntityData>();

		private DungeonData(TiledMap map, string name)
		{
			this.map = map;
			Name = name;
			WidthTiles = map.Width;
			HeightTiles = map.Height;
			Width = WidthTiles / Global.RoomWidth;
			Height = HeightTiles / Global.RoomHeight;
			Tileset = new Tileset("gfx/game/" + map.Properties.GetString("tileset"), Global.TileSize, Global.TileSize);

			tiles = new DungeonTile[WidthTiles, HeightTiles];

			var layer = map.GetLayer<TiledMapTileLayer>("main");

			for (var ix = 0; ix < WidthTiles; ix++)
			{
				for (var iy = 0; iy < HeightTiles; iy++)
				{
					var tid = -1;
					if (layer.TryGetTile(ix, iy, out TiledMapTile? tiledTile))
					{
						tid = tiledTile.Value.GlobalIdentifier;
					}

					var properties = map.GetTilePropertiesAt(layer, ix, iy);
					var tile = new DungeonTile(ix, iy, tid, properties);
					if (properties != null)
					{
						InitTile(tile, properties);
					}
					tiles[ix, iy] = tile;
				}
			}

			if (PlayerStartTile == null)
			{
				throw new Exception("Dungeon " + this + " has no starting player tile.");
			}

			Log.Debug("Created " + this + " of size " + Width + "x" + Height + " with player start tile " + PlayerStartTile + ".");
		}

		private void InitTile(DungeonTile tile, TiledMapProperties properties)
		{
			if (Enum.TryParse(properties.GetString("solidType", "None"), out TileSolidType solidType))
			{
				tile.SolidType = solidType;
			}
			else
			{
				Log.Error("Unknown tile solid type '" + properties.GetString("solidType") + "' at " + tile);
			}

			if (properties.ContainsKey("tile"))
			{
				if (Enum.TryParse(properties.GetString("tile"), out DungeonTileType type))
				{
					tile.Type = type;
				}
				else
				{
					Log.Error("Unknown tile type '" + properties.GetString("type") + "' at " + tile);
				}
			}

			tile.Trigger = properties.GetString("trigger");

			tile.Anim = properties.GetString("anim");

			if (properties.ContainsKey("displayTid"))
			{
				tile.DisplayTid = properties.GetInt("displayTid");
			}

			if (properties.ContainsKey("entity"))
			{
				InitEntity(tile, properties);
			}
		}

		private void InitEntity(DungeonTile tile, TiledMapProperties properties)
		{
			if (!properties.ContainsKey("entity"))
			{
				Log.Error("Invalid entity at " + tile + ": 'entity' not found.");
				return;
			}

			if (!Enum.TryParse(properties.GetString("direction", "Down"), out Direction direction))
			{
				Log.Error("Unknown entity direction '" + properties.GetString("direction") + "' at " + tile);
			}

			var entityName = properties.GetString("entity");
			if (entityName == "player")
			{
				PlayerStartTile = tile;
				PlayerStartDirection = direction;
			}
			else
			{
				var entity = EntityData.Get(entityName);
				if (entity == null)
				{
					Log.Error("Unknown entity '" + entityName + "' at " + tile);
				}
				else
				{
					entities.Add(new EntityArguments(entity, tile.X, tile.Y, direction));
				}
			}
		}

		private void InitObjects()
		{
			Log.Debug("Creating objects for " + this + ".");

			var layer = map.GetLayer<TiledMapObjectLayer>("object");
			if (layer == null)
			{
				Log.Warn(this + " has no 'object' layer.");
				return;
			}

			foreach(var obj in layer.Objects)
			{
				var bounds = new Rectangle((int)obj.Position.X, (int)obj.Position.Y, (int)obj.Size.Width, (int)obj.Size.Height);

				if (obj.Properties.GetString("type") == "Group")
				{
					InitGroup(obj, bounds);
				}
				else
				{
					if (!Enum.TryParse(obj.Properties.GetString("type"), out ObjectType type))
					{
						Log.Error("Unknown object type '" + obj.Properties.GetString("type") + "' at " + bounds + ".");
						continue;
					}

					switch (type)
					{
						case ObjectType.Link:
							{
								var target = Get(obj.Properties.GetString("target"));
								if (target == null)
								{
									Log.Error("Object at " + bounds + " has an invalid target '" + obj.Properties.GetString("target") + "'.");
									continue;
								}
								objects.Add(ObjectData.CreateLink(bounds, target));
								break;
							}
						case ObjectType.Info:
							{
								var text = obj.Properties.GetString("text");
								if (text == null)
								{
									Log.Error("Object at " + bounds + " has an no text.");
									continue;
								}
								objects.Add(ObjectData.CreateInfo(bounds, text));
								break;
							}
					}

					Log.Debug("Added " + objects[objects.Count - 1] + ".");
				}
			}
		}

		private void InitGroup(TiledMapObject obj, Rectangle bounds)
		{
			var loot = EntityData.Get(obj.Properties.GetString("loot"));
			if (loot == null)
			{
				Log.Error("Group at " + bounds + " has an invalid loot '" + obj.Properties.GetString("loot") + "'.");
				return;
			}

			var id = groupRewards.Count;
			groupRewards.Add(loot);

			IterateEntities((entity) =>
			{
				if(entity.Data.Groupable && Mathf.IntersectPoint(bounds, entity.TileX * Global.TileSize, entity.TileY * Global.TileSize))
				{
					entity.GroupId = id;
					Log.Debug("Assigned " + entity + " to group " + id + ".");
				}
			});
		}

		public DungeonTile GetTileAt(int x, int y)
		{
			if (x < 0 || x >= WidthTiles || y < 0 || y >= HeightTiles) return null;
			return tiles[x, y];
		}

		public void IterateTiles(Action<DungeonTile> action)
		{
			for (var ix = 0; ix < WidthTiles; ix++)
			{
				for (var iy = 0; iy < HeightTiles; iy++)
				{
					action(tiles[ix, iy]);
				}
			}
		}

		public void IterateEntities(Action<EntityArguments> action)
		{
			foreach (var entity in entities)
			{
				action(entity);
			}
		}

		public void IterateObjects(Action<ObjectData> action)
		{
			foreach(var obj in objects)
			{
				action(obj);
			}
		}

		public EntityData GetGroupReward(int id)
		{
			if (id < 0) return null;

			return groupRewards[id];
		}

		public override string ToString()
		{
			return "[DungeonData Name='" + Name + "' Width=" + WidthTiles + " Height=" + HeightTiles + "]";
		}

		private static readonly Dictionary<string, DungeonData> dungeons = new Dictionary<string, DungeonData>();

		public static void Init()
		{
			Log.Debug("Loading maps...");
			var maps = Asset.LoadAll<TiledMap>("map");
			foreach (var map in maps.Values)
			{
				if (map.Properties.GetString("type") == "dungeon")
				{
					var name = Path.GetFileName(map.Name);
					if (dungeons.ContainsKey(name))
					{
						throw new Exception("Dungeon '" + name + "' already exists.");
					}
					dungeons[name] = new DungeonData(map, name);
				}
			}

			Log.Debug("Initializing objects...");
			foreach(var room in dungeons.Values)
			{
				room.InitObjects();
			}
		}

		public static DungeonData Get(string name)
		{
			if (!dungeons.TryGetValue(name, out DungeonData dungeon))
			{
				throw new Exception("Dungeon '" + name + "' not found.");
			}
			return dungeons[name];
		}
	}
}
