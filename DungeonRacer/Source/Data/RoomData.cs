using MonoGame.Extended.Tiled;
using System;
using MonoPunk;
using System.Collections.Generic;

namespace DungeonRacer
{
	[Flags]
	enum RoomFlags
	{
		None = 0x0000,
		Initial = 0x0001,
		ExtraTime = 0x0002,
		Bonus = 0x0004,
		Spike = 0x0008,
		Vertical = 0x1000,
		Horizontal = 0x2000,
		Test = 0x8000
	}

	class RoomData
	{
		public string Name { get { return map.Name; } }
		public RoomFlags Flags { get; private set; }

		private readonly TiledMap map;
		private readonly TiledMapTileLayer layer;

		private RoomData(TiledMap map)
		{
			this.map = map;
			layer = map.GetLayer<TiledMapTileLayer>("main");

			if (map.Properties.GetBool("initial")) Flags |= RoomFlags.Initial;
			if (map.Properties.GetBool("bonus")) Flags |= RoomFlags.Bonus;
			if (map.Properties.GetBool("spike")) Flags |= RoomFlags.Spike;
			if (map.Properties.GetBool("test")) Flags |= RoomFlags.Test;

			if (HasEntrance(0, 5)) Flags |= RoomFlags.Horizontal;
			if (HasEntrance(7, 0)) Flags |= RoomFlags.Vertical;

			// TODO create a list of extra time tiles, remove them from map, store them somehwere
			IterateTiles(false, false, (x, y, properties) =>
			{
				if(properties.ContainsKey("entity") && properties.GetString("entity") == "extra_time")
				{
					Flags |= RoomFlags.ExtraTime;
				}
			});

			Log.Debug("Created room data with flags " + Flags + ".");
		}

		private bool HasEntrance(int x, int y)
		{
			return "entrance" == map.GetTilePropertiesAt(layer, x, y).GetString("door");
		}

		public bool HasFlag(RoomFlags flag)
		{
			return (Flags & flag) > 0;
		}

		private bool Match(RoomFlags flags)
		{
			// FIXME standardize mentadory and optional flags.

			var extraTime = (flags & RoomFlags.ExtraTime) > 0;
			if (extraTime)
			{
				return Flags == flags;
			}
			else
			{
				return (Flags & ~RoomFlags.ExtraTime) == flags;
			}
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

		public override string ToString()
		{
			return "[RoomData Name='" + Name + " Flags=" + Flags + "]";
		}

		private static readonly List<RoomData> rooms = new List<RoomData>();

		public static void Init()
		{
			var maps = Asset.LoadAll<TiledMap>("map");
			foreach(var map in maps.Values)
			{
				rooms.Add(new RoomData(map));
			}
		}

		public static void Query(RoomFlags flags, Action<RoomData> action)
		{
			foreach(var room in rooms)
			{
				if(room.Match(flags))
				{
					action(room);
				}
			}
		}

		public static RoomData GetRandom(RoomFlags flags)
		{
			var candidates = new List<RoomData>();
			Query(flags, (room) => candidates.Add(room));
			return Rand.GetRandomElement(candidates);
		}
	}
}
