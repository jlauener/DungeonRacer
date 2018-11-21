using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	enum RoomType
	{
		Normal,
		Start,
		Shop,
	}

	class RoomData
	{
		public RoomType Type { get; set; }
		public int X { get; }
		public int Y { get; }

		private readonly List<EntityArguments> entities = new List<EntityArguments>();

		public RoomData(int x, int y)
		{
			X = x;
			Y = y;
		}

		public void AddEntity(EntityData entity, int x, int y, Direction direction)
		{
			entities.Add(new EntityArguments(entity, x, y, direction));
		}

		public void IterateEntities(Action<EntityArguments> action)
		{
			foreach(var entity in entities)
			{
				action(entity);
			}
		}

		public override string ToString()
		{
			return "[RoomData Pos=" + X + "," + Y + " Type=" + Type + "]";
		}
	}
}
