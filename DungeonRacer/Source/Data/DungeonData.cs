using MonoGame.Extended.Tiled;
using System;
using MonoPunk;
using System.Collections.Generic;
using System.IO;

namespace DungeonRacer
{
	//enum DungeonTileLayer
	//{
	//	Ground,
	//	Back,
	//	Front
	//}

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

	class DungeonTile
	{
		public int X { get; }
		public int Y { get; }
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

		//public RoomData StartingRoom { get; private set; }
		public DungeonTile PlayerStartTile { get; private set; }
		public Direction PlayerStartDirection { get; private set; }

		private readonly DungeonTile[,] tiles;
		private readonly List<EntityArguments> entities = new List<EntityArguments>();
		//private readonly RoomData[,] rooms;

		private DungeonData(TiledMap map, string name)
		{
			Name = name;
			WidthTiles = map.Width;
			HeightTiles = map.Height;
			Width = WidthTiles / Global.RoomWidth;
			Height = HeightTiles / Global.RoomHeight;
			Tileset = new Tileset("gfx/game/" + map.Properties.GetString("tileset"), Global.TileSize, Global.TileSize);

			//rooms = new RoomData[Width, Height];
			//for (var ix = 0; ix < Width; ix++)
			//{
			//	for(var iy = 0; iy < Height; iy++)
			//	{
			//		rooms[ix,iy] = new RoomData(ix, iy);
			//	}
			//}

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

			//var roomX = tile.X / Global.RoomWidth;
			//var roomY = tile.Y / Global.RoomHeight;
			//var room = GetRoomAt(roomX, roomY);

			var entityName = properties.GetString("entity");
			if (entityName == "player")
			{
				//StartingRoom = room;
				PlayerStartTile = tile;
				PlayerStartDirection = direction;
				//room.Type = RoomType.Start;
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
					//room.AddEntity(entity, tile.X, tile.Y, direction);
					entities.Add(new EntityArguments(entity, tile.X, tile.Y, direction));
				}
			}
		}

		public DungeonTile GetTileAt(int x, int y)
		{
			if (x < 0 || x >= WidthTiles || y < 0 || y >= HeightTiles) return null;
			return tiles[x, y];
		}

		//public RoomData GetRoomAt(int x, int y)
		//{
		//	if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
		//	return rooms[x, y];
		//}

		//public void IterateRooms(Action<RoomData> action)
		//{
		//	for (var ix = 0; ix < Width; ix++)
		//	{
		//		for (var iy = 0; iy < Height; iy++)
		//		{
		//			action(rooms[ix, iy]);
		//		}
		//	}
		//}

		public void IterateEntities(Action<EntityArguments> action)
		{
			foreach (var entity in entities)
			{
				action(entity);
			}
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

		public override string ToString()
		{
			return "[DungeonData Name='" + Name + "' Width=" + WidthTiles + " Height=" + HeightTiles + "]";
		}

		private static readonly Dictionary<string, DungeonData> dungeons = new Dictionary<string, DungeonData>();

		public static void Init()
		{
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
