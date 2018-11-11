using System;
using System.Collections.Generic;
using MonoPunk;
using MonoGame.Extended.Tiled;

namespace DungeonRacer
{
	// FIXME there should be a door class with all that...
	enum DoorId
	{
		Left,
		Right,
		Up,
		Down
	}

	static class DoorUtils
	{
		public static DoorId GetMatching(DoorId doorId)
		{
			switch (doorId)
			{
				case DoorId.Left:
					return DoorId.Right;
				case DoorId.Right:
					return DoorId.Left;
				case DoorId.Up:
					return DoorId.Down;
				default: // DoorId.Down
					return DoorId.Up;
			}
		}
	}

	class Room : Entity
	{
		public const int TileWall = 0;
		public const int TileWallDecoration = 40;
		public const int TileGround = 3;
		public const int RoofHoriz = 24;
		public const int RoofVert = 25;
		public const int RoofCorner = 26;

		public RoomData Data { get; }
		private readonly RoomFlags flags;
		private readonly bool flipX;
		private readonly bool flipY;

		private readonly GridCollider grid;
		private readonly Tilemap backMap;
		private readonly Tilemap frontMap;

		private readonly Dictionary<DoorId, Room> nextRooms = new Dictionary<DoorId, Room>();
		private readonly List<RoomEntity> entities = new List<RoomEntity>();

		public Room(RoomData data, RoomFlags flags, bool flipX = false, bool flipY = false)
		{
			Data = data;
			this.flags = flags;
			this.flipX = flipX;
			this.flipY = flipY;

			Type = Global.TypeMap;
			Width = Global.RoomWidthPx;
			Height = Global.RoomHeightPx;
			grid = new GridCollider(Global.RoomWidth, Global.RoomHeight, Global.TileSizePx, Global.TileSizePx);
			Collider = grid;

			backMap = new Tilemap("room_default", Global.RoomWidth, Global.RoomHeight);
			backMap.Layer = Global.LayerMapBack;
			backMap.Scale = Global.Scale;
			Add(backMap);

			frontMap = new Tilemap("room_default", Global.RoomWidth, Global.RoomHeight);
			frontMap.Layer = Global.LayerMapFront;
			frontMap.Scale = Global.Scale;
			Add(frontMap);

			// FIXME use map data to property draw the walls

			// roof
			frontMap.SetTileAt(0, 0, RoofCorner);
			frontMap.SetTileAt(Global.RoomWidth - 1, 0, RoofCorner);
			frontMap.SetTileAt(0, Global.RoomHeight - 1, RoofCorner);
			frontMap.SetTileAt(Global.RoomWidth - 1, Global.RoomHeight - 1, RoofCorner);
			for (var ix = 1; ix < Global.RoomWidth - 1; ix++)
			{
				frontMap.SetTileAt(ix, 0, RoofHoriz);
				frontMap.SetTileAt(ix, Global.RoomHeight - 1, RoofHoriz);
			}
			for (var iy = 1; iy < Global.RoomHeight - 1; iy++)
			{
				frontMap.SetTileAt(0, iy, RoofVert);
				frontMap.SetTileAt(Global.RoomWidth - 1, iy, RoofVert);
			}

			// wall
			DrawRect(backMap, 1, 1, Global.RoomWidth - 2, Global.RoomHeight - 2, TileWall);

			// ground
			DrawRect(backMap, 2, 2, Global.RoomWidth - 4, Global.RoomHeight - 4, TileGround, true);

			// first pass set solid flag
			data.IterateTiles(flipX, flipY, (x, y, properties) =>
			{
				if (properties.ContainsKey("wall"))
				{
					grid.SetTileAt(x, y, TileSolidType.Full);

					if(properties.GetString("wall") == "decoration")
					{
						// FIXME
						var tid = TileWallDecoration;
						if (x == 1) tid += 2;
						else if (x == Global.RoomWidth - 2) tid += 3;
						else if (y == Global.RoomHeight - 2) tid += 1;
						backMap.SetTileAt(x, y, tid);
					}
				}
			});
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			// FIXME better way to handle special tiles, more will come...
			// Maybe store wall/entity/wall data in a better way in RoomData.
			var extraTimeTiles = new List<Tuple<int, int>>();

			Data.IterateTiles(flipX, flipY, (x, y, properties) =>
			{
				if (properties.ContainsKey("door"))
				{
					HandleDoor(x, y, properties.GetString("door"));
				}
				else if (properties.ContainsKey("entity"))
				{
					var entityName = properties.GetString("entity");
					if (entityName == "extra_time")
					{
						extraTimeTiles.Add(new Tuple<int, int>(x, y));
					}
					else
					{
						CreateEntity(properties.GetString("entity"), x, y);
					}
				}
			});

			if((flags & RoomFlags.ExtraTime) > 0)
			{
				var tile = Rand.GetRandomElement(extraTimeTiles);
				if(tile != null)
				{
					CreateEntity("extra_time", tile.Item1, tile.Item2);
				}
				else
				{
					Log.Error("RoomFlags.ExtraTime present but no 'extra_time' entity found.");
				}
			}
		}

		private void CreateEntity(string name, float x, float y)
		{
			var entityData = EntityData.Get(name);
			if (entityData != null)
			{
				var entity = new RoomEntity(this, entityData, x, y);
				entities.Add(entity);
				Scene.Add(entity);
			}
		}

		protected override void OnRemoved()
		{
			base.OnRemoved();

			foreach (var entity in entities)
			{
				Scene.Remove(entity);
			}
		}

		public void RemoveEntity(RoomEntity entity)
		{
			entities.Remove(entity);
		}

		public Room GetNextRoom(DoorId doorId)
		{
			Room nextRoom;
			if (nextRooms.TryGetValue(doorId, out nextRoom))
			{
				return nextRoom;
			}
			return null;
		}

		public void SetNextRoom(DoorId doorId, Room nextRoom)
		{
			nextRooms[doorId] = nextRoom;
			nextRoom.nextRooms[DoorUtils.GetMatching(doorId)] = this;
		}

		private void HandleDoor(int x, int y, string name)
		{
			if (x == 1)
			{
				// left
				for (var ix = -1; ix <= 0; ix++)
				{
					grid.SetTileAt(x + ix, y - 1, TileSolidType.HalfTop);
					grid.SetTileAt(x + ix, y, TileSolidType.None);
					grid.SetTileAt(x + ix, y + 1, TileSolidType.HalfBottom);
				}

				backMap.SetTileAt(x, y - 1, 6);
				backMap.SetTileAt(x, y, 14);
				backMap.SetTileAt(x, y + 1, 22);

				CreateEntity("door_left_" + name, x, y);
			}
			else if (x == Global.RoomWidth - 2)
			{
				// right
				for (var ix = 0; ix <= 1; ix++)
				{
					grid.SetTileAt(x + ix, y - 1, TileSolidType.HalfTop);
					grid.SetTileAt(x + ix, y, TileSolidType.None);
					grid.SetTileAt(x + ix, y + 1, TileSolidType.HalfBottom);
				}

				backMap.SetTileAt(x, y - 1, 7);
				backMap.SetTileAt(x, y, 15);
				backMap.SetTileAt(x, y + 1, 23);

				CreateEntity("door_right_" + name, x, y);
			}
			else if (y == 1)
			{
				// up
				for (var iy = -1; iy <= 0; iy++)
				{
					grid.SetTileAt(x - 1, y + iy, TileSolidType.HalfLeft);
					grid.SetTileAt(x, y + iy, TileSolidType.None);
					grid.SetTileAt(x + 1, y + iy, TileSolidType.HalfRight);
				}

				backMap.SetTileAt(x - 1, y, 27);
				backMap.SetTileAt(x, y, 28);
				backMap.SetTileAt(x + 1, y, 29);

				CreateEntity("door_up_" + name, x, y);
			}
			else if (y == Global.RoomHeight - 2)
			{
				// down
				for (var iy = 0; iy <= 1; iy++)
				{
					grid.SetTileAt(x - 1, y + iy, TileSolidType.HalfLeft);
					grid.SetTileAt(x, y + iy, TileSolidType.None);
					grid.SetTileAt(x + 1, y + iy, TileSolidType.HalfRight);
				}

				backMap.SetTileAt(x - 1, y, 35);
				backMap.SetTileAt(x, y, 36);
				backMap.SetTileAt(x + 1, y, 37);

				CreateEntity("door_down_" + name, x, y);
			}
			else
			{
				Log.Error("Unknown door location at " + x + "," + y + ".");
			}
		}

		private void CreateDoorEntity(string name, float x, float y)
		{
			if (name == "exit") CreateEntity("door_left", x, y);
			else if (name == "locked") CreateEntity("door_left_locked", x, y);
		}

		private void DrawRect(Tilemap tilemap, int x, int y, int width, int height, int tileId, bool fill = false)
		{
			// corners
			tilemap.SetTileAt(x, y, tileId);
			tilemap.SetTileAt(x + width - 1, y, tileId + 2);
			tilemap.SetTileAt(x, y + height - 1, tileId + 16);
			tilemap.SetTileAt(x + width - 1, y + height - 1, tileId + 18);

			// horizontal
			for (var ix = x + 1; ix < x + width - 1; ix++)
			{
				tilemap.SetTileAt(ix, y, tileId + 1);
				tilemap.SetTileAt(ix, y + height - 1, tileId + 17);
			}

			// vertical
			for (var iy = y + 1; iy < y + height - 1; iy++)
			{
				tilemap.SetTileAt(x, iy, tileId + 8);
				tilemap.SetTileAt(x + width - 1, iy, tileId + 10);
			}

			// fill
			if (fill)
			{
				for (var ix = x + 1; ix < x + width - 1; ix++)
				{
					for (var iy = y + 1; iy < y + height - 1; iy++)
					{
						tilemap.SetTileAt(ix, iy, tileId + 9);
					}
				}
			}
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			Asset.AddTileset("room_default", "gfx/tileset", 16, 16);
		}
	}
}
