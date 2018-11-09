using System;
using System.Collections.Generic;
using MicroPunk;
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
			switch(doorId)
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
		public const int TileGround = 3;
		public const int RoofHoriz = 24;
		public const int RoofVert = 25;
		public const int RoofCorner = 26;

		private readonly RoomData data;
		private readonly bool flipX;
		private readonly bool flipY;

		private readonly GridCollider grid;
		private readonly Tilemap backMap;
		private readonly Tilemap frontMap;

		private readonly Dictionary<DoorId, Room> nextRooms = new Dictionary<DoorId, Room>();
		private List<RoomEntity> entities;

		public Room(RoomData data, bool flipX = false, bool flipY = false)
		{
			this.data = data;
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
				if (properties.GetBool("solid"))
				{
					grid.SetTileAt(x, y, TileSolidType.Full);
				}
			});

			// second pass handle doors and entities
			data.IterateTiles(flipX, flipY, (x, y, properties) =>
			{
				if (properties.ContainsKey("door"))
				{
					HandleDoor(x, y, properties);
				}
				else if(properties.ContainsKey("entity"))
				{
					HandleEntity(x, y, properties);
				}
			});
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			if(entities == null)
			{
				entities = new List<RoomEntity>();
				data.IterateTiles(flipX, flipY, (x, y, properties) =>
				{
					if (properties.ContainsKey("entity"))
					{
						entities.Add(new RoomEntity(this, EntityData.Get(properties.GetString("entity")), x, y));
					}
				});
			}

			foreach(var entity in entities)
			{
				Scene.Add(entity);
			}
		}


		protected override void OnRemoved()
		{
			base.OnRemoved();

			foreach(var entity in entities)
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

		private void HandleDoor(int x, int y, TiledMapProperties properties)
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
			}
			else if (y == 1)
			{
				// top
				for (var iy = -1; iy <= 0; iy++)
				{
					grid.SetTileAt(x - 1, y + iy, TileSolidType.HalfLeft);
					grid.SetTileAt(x, y + iy, TileSolidType.None);
					grid.SetTileAt(x + 1, y + iy, TileSolidType.HalfRight);
				}

				backMap.SetTileAt(x - 1, y, 27);
				backMap.SetTileAt(x, y, 28);
				backMap.SetTileAt(x + 1, y, 29);
			}
			else if (y == Global.RoomHeight - 2)
			{
				// bottom
				for (var iy = 0; iy <= 1; iy++)
				{
					grid.SetTileAt(x - 1, y + iy, TileSolidType.HalfLeft);
					grid.SetTileAt(x, y + iy, TileSolidType.None);
					grid.SetTileAt(x + 1, y + iy, TileSolidType.HalfRight);
				}

				backMap.SetTileAt(x - 1, y, 35);
				backMap.SetTileAt(x, y, 36);
				backMap.SetTileAt(x + 1, y, 37);
			}
			else
			{
				Log.Error("Unknown door location at " + x + "," + y + ".");
			}
		}

		private void HandleEntity(int x, int y, TiledMapProperties properties)
		{
			var entity = new RoomEntity(this, EntityData.Get(properties.GetString("entity")), x, y);
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
