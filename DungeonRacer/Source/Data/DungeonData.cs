using MonoGame.Extended.Tiled;
using System;
using MonoPunk;
using System.Collections.Generic;

namespace DungeonRacer
{
	enum DungeonTileLayer
	{
		Ground,
		Back,
		Front
	}

	class DungeonTile
	{
		public int X { get; }
		public int Y { get; }
		public Dictionary<string, string> Properties { get; }

		public int Id { get; set; } = -1;
		public int DisplayTid { get; set; } = -1;
		public Direction Direction { get; set; }
		public TileSolidType SolidType { get; set; } = TileSolidType.None;
		public DungeonTileLayer Layer { get; set; } = DungeonTileLayer.Back;
		public AnimatorData Anim { get; set; }
		public EntityData Entity { get; set; }

		public DungeonTile(int x, int y, Dictionary<string, string> properties)
		{
			X = x;
			Y = y;
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
		public Tileset Tileset { get; private set; }

		public DungeonTile PlayerStartTile { get; private set; }

		public int CoinCount { get; private set; }
		public float TimeGold { get; private set; }
		public float TimeSilver { get; private set; }
		public float TimeBronze { get; private set; }

		private readonly DungeonTile[,] tiles;

		private DungeonData(TiledMap map)
		{
			Name = map.Properties.GetString("name");
			Width = map.Width;
			Height = map.Height;
			Tileset = new Tileset("gfx/game/" + map.Properties.GetString("tileset"), Global.TileSize, Global.TileSize);

			TimeGold = map.Properties.GetFloat("timeGold");
			TimeSilver = map.Properties.GetFloat("timeSilver");
			TimeBronze = map.Properties.GetFloat("timeBronze");

			tiles = new DungeonTile[Width, Height];

			var tilesLayer = map.GetLayer<TiledMapTileLayer>("tiles");
			var entitiesLayer = map.GetLayer<TiledMapTileLayer>("entities");

			for (var ix = 0; ix < map.Width; ix++)
			{
				for (var iy = 0; iy < map.Height; iy++)
				{
					var tileProperties = map.GetTilePropertiesAt(tilesLayer, ix, iy);

					var tile = new DungeonTile(ix, iy, tileProperties);
					tiles[ix, iy] = tile;

					var tiledTile = map.GetTileAt(tilesLayer, ix, iy);
					if (tiledTile != null)
					{
						tile.Id = tiledTile.Id;
						tile.DisplayTid = tiledTile.Id;
					}

					if (tileProperties != null)
					{
						InitTile(tile, tileProperties);
					}

					var entityProperties = map.GetTilePropertiesAt(entitiesLayer, ix, iy);
					if (entityProperties != null)
					{
						InitEntity(tile, entityProperties);
					}
				}
			}

			if (PlayerStartTile == null)
			{
				throw new Exception("Dungeon " + this + " has no starting player tile.");
			}

			Log.Debug("Created " + this + " with player start tile " + PlayerStartTile + ".");
		}

		private void InitTile(DungeonTile tile, TiledMapProperties properties)
		{
			if (properties.ContainsKey("solidType"))
			{
				if (Enum.TryParse(properties.GetString("solidType"), out TileSolidType solidType))
				{
					tile.SolidType = solidType;
				}
				else
				{
					Log.Error("Unknown tile solid type '" + properties.GetString("solidType") + "' at " + tile);
				}
			}

			if (properties.ContainsKey("layer"))
			{
				if (Enum.TryParse(properties.GetString("layer"), out DungeonTileLayer layer))
				{
					tile.Layer = layer;
				}
				else
				{
					Log.Error("Unknown tile layer '" + properties.GetString("layer") + "' at " + tile);
				}
			}

			if (properties.GetBool("animated"))
			{
				var frameCount = properties.GetInt("animFrameCount");
				var interval = properties.GetFloat("animInterval");
				var frames = new int[frameCount];
				for (var i = 0; i < frameCount; i++) frames[i] = tile.DisplayTid + i;
				// TODO don't create a new AnimatorData each time
				tile.Anim = new AnimatorData(Tileset);
				tile.Anim.Add("idle", AnimatorMode.Loop, interval, frames);
			}

			if (properties.ContainsKey("displayTid"))
			{
				tile.DisplayTid = properties.GetInt("displayTid");
			}
		}

		private void InitEntity(DungeonTile tile, TiledMapProperties properties)
		{
			if (!properties.ContainsKey("entity"))
			{
				Log.Error("Invalid entity at " + tile + ": 'entity' not found.");
				return;
			}

			if (Enum.TryParse(properties.GetString("direction", "Down"), out Direction direction))
			{
				tile.Direction = direction;
			}
			else
			{
				Log.Error("Unknown tile direction '" + properties.GetString("direction") + "' at " + tile);
			}

			var entityName = properties.GetString("entity");
			if (entityName == "player")
			{
				PlayerStartTile = tile;
			}
			else
			{
				if (entityName == "coin")
				{
					CoinCount++;
				}

				tile.Entity = EntityData.Get(entityName);
				if (tile.Entity == null)
				{
					Log.Error("Unknown entity '" + entityName + "' at " + tile);
				}
			}
		}

		public DungeonTile GetTileAt(int x, int y)
		{
			if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
			return tiles[x, y];
		}

		public void Iterate(Action<DungeonTile> action)
		{
			for (var ix = 0; ix < Width; ix++)
			{
				for (var iy = 0; iy < Height; iy++)
				{
					action(tiles[ix, iy]);
				}
			}
		}

		public override string ToString()
		{
			return "[DungeonData Name='" + Name + " Width=" + Width + " Height=" + Height + "]";
		}

		private static readonly Dictionary<string, DungeonData> dungeons = new Dictionary<string, DungeonData>();

		public static void Init()
		{
			var maps = Asset.LoadAll<TiledMap>("map");
			foreach (var map in maps.Values)
			{
				if (map.Properties.GetString("type") == "dungeon")
				{
					var name = map.Properties.GetString("name");
					if (dungeons.ContainsKey(name))
					{
						throw new Exception("Dungeon '" + name + "' already exists.");
					}
					dungeons[name] = new DungeonData(map);
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

	// some auto tile algo prototype... might be useful one day, or not...

	//[Flags]
	//enum TilePositions
	//{
	//	None = 0x00,
	//	Left = 0x01,
	//	Right = 0x02,
	//	Up = 0x04,
	//	Down = 0x08
	//}

	//private void RenderFlatTile(Tilemap tilemap, DungeonTile tile, int tid)
	//{
	//	var neighbors = GetNeighbors(tile);

	//	// straight edges
	//	if (SetTile(tilemap, tile, tid + 18, TilePositions.Up | TilePositions.Down | TilePositions.Left)) return;
	//	if (SetTile(tilemap, tile, tid + 16, TilePositions.Up | TilePositions.Down | TilePositions.Right)) return;
	//	if (SetTile(tilemap, tile, tid + 33, TilePositions.Left | TilePositions.Right | TilePositions.Up)) return;
	//	if (SetTile(tilemap, tile, tid + 1, TilePositions.Left | TilePositions.Right | TilePositions.Down)) return;

	//	// corners
	//	if (SetTile(tilemap, tile, tid + 0, TilePositions.Right | TilePositions.Down)) return;
	//	if (SetTile(tilemap, tile, tid + 2, TilePositions.Left | TilePositions.Down)) return;
	//	if (SetTile(tilemap, tile, tid + 32, TilePositions.Right | TilePositions.Up)) return;
	//	if (SetTile(tilemap, tile, tid + 34, TilePositions.Left | TilePositions.Up)) return;

	//	// straight
	//	if (SetTile(tilemap, tile, tid + 49, TilePositions.Up | TilePositions.Down)) return;
	//	if (SetTile(tilemap, tile, tid + 48, TilePositions.Left | TilePositions.Right)) return;

	//	tilemap.SetTileAt(tile.X, tile.Y, tid + 17);
	//}

	//private void RenderWallTile(Tilemap tilemap, DungeonTile tile, int tid)
	//{
	//	var neighbors = GetNeighbors(tile);

	//	// corners
	//	if (SetTile(tilemap, tile, tid + 0, TilePositions.Right | TilePositions.Down)) return;
	//	if (SetTile(tilemap, tile, tid + 2, TilePositions.Left | TilePositions.Down)) return;
	//	if (SetTile(tilemap, tile, tid + 32, TilePositions.Right | TilePositions.Up)) return;
	//	if (SetTile(tilemap, tile, tid + 34, TilePositions.Left | TilePositions.Up)) return;

	//	tilemap.SetTileAt(tile.X, tile.Y, tid + 17);
	//}

	//private bool SetTile(Tilemap tilemap, DungeonTile tile, int tid, TilePositions neighbors)
	//{
	//	if (GetNeighbors(tile) != neighbors)
	//	{
	//		return false;
	//	}

	//	tilemap.SetTileAt(tile.X, tile.Y, tid);
	//	return true;
	//}

	//private TilePositions GetNeighbors(DungeonTile tile)
	//{
	//	TilePositions result = TilePositions.None;
	//	if (GetTileTypeAt(tile.X - 1, tile.Y) == tile.Type) result |= TilePositions.Left;
	//	if (GetTileTypeAt(tile.X + 1, tile.Y) == tile.Type) result |= TilePositions.Right;
	//	if (GetTileTypeAt(tile.X, tile.Y - 1) == tile.Type) result |= TilePositions.Up;
	//	if (GetTileTypeAt(tile.X, tile.Y + 1) == tile.Type) result |= TilePositions.Down;
	//	return result;
	//}
}
