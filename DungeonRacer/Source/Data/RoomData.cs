using MonoGame.Extended.Tiled;
using System;
using MonoPunk;
using System.Collections.Generic;

namespace DungeonRacer
{
	enum RoomType
	{
		Initial,
		Horizontal,
		Vertical,
		HorizontalTurn,
		VerticalTurn
	}

	class RoomData
	{
		public RoomType Type { get; }
		private readonly TiledMap map;

		private RoomData(TiledMap map)
		{
			Type = map.Properties.GetEnum<RoomType>("type");
			this.map = map;
		}

		public void IterateTiles(bool flipX, bool flipY, Action<int, int, TiledMapProperties> action)
		{
			var layer = map.GetLayer<TiledMapTileLayer>("main");
			for (var ix = 0; ix < map.Width; ix++)
			{
				for (var iy = 0; iy < map.Height; iy++)
				{
					var properties = map.GetTilePropertiesAt(layer, ix, iy);
					if (properties != null)
					{
						var x = flipX ? map.Width - (ix + 1): ix;
						var y = flipY ? map.Height - (iy + 1) : iy;
						action(1 + x, 1 + y, properties);
					}
				}
			}
		}

		private static readonly Dictionary<RoomType, List<RoomData>> rooms = new Dictionary<RoomType, List<RoomData>>();

		public static void Init()
		{
			var maps = Asset.LoadAll<TiledMap>("map");
			foreach(var map in maps.Values)
			{
				var room = new RoomData(map);
				GetRooms(room.Type).Add(room);
			}
		}

		public static List<RoomData> GetRooms(RoomType type)
		{
			List<RoomData> typeList;
			if (!rooms.TryGetValue(type, out typeList))
			{
				typeList = new List<RoomData>();
				rooms.Add(type, typeList);
			}
			return typeList;
		}
	}
}
