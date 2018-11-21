using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class Room : Entity
	{
		public RoomData Data { get; }
		public int RoomX { get; }
		public int RoomY { get; }

		public bool Cleared { get { return enemies.Count == 0; } }

		private bool active;
		private bool initialized;

		// TODO make this more generic, win condition?
		private readonly List<Enemy> enemies = new List<Enemy>();

		private readonly List<Door> doors = new List<Door>();
		//private readonly List<Ghost> ghosts = new List<Ghost>();

		public Room(RoomData data, int roomX, int roomY) : base(roomX * Global.RoomWidthPx, roomY * Global.RoomHeightPx)
		{
			Data = data;
			RoomX = roomX;
			RoomY = roomY;

			Width = Global.RoomWidthPx;
			Height = Global.RoomHeightPx;
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Data.IterateEntities((args) =>
			{
				var entity = GameEntity.Create(args);
				if (entity is Enemy)
				{
					enemies.Add((Enemy)entity);
				}
				else if (entity is Door)
				{
					doors.Add((Door)entity);
				}
				else
				{
					Scene.Add(entity);
				}
			});
		}

		public void Enter()
		{
			if (!initialized)
			{
				foreach (var door in doors) Scene.Add(door);
				foreach (var enemy in enemies) Scene.Add(enemy);
				initialized = true;
			}

			//foreach (var ghost in ghosts) Scene.Add(ghost);
			//ghosts.Clear();

			active = true;
		}

		public void Leave()
		{
			active = false;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!active) return;

			if (!Cleared)
			{
				for (var i = enemies.Count - 1; i >= 0; i--)
				{
					if (!enemies[i].Alive)
					{
						//ghosts.Add(new Ghost(new EntityArguments(EntityData.Get("ghost"), enemies[i].Position)));
						enemies.RemoveAt(i);
					}
				}

				if (Cleared)
				{
					// TODO create event, delegate...
					Asset.LoadSoundEffect("sfx/room_clear").Play();

					foreach (var door in doors) door.Open();
				}
			}
		}

		public override string ToString()
		{
			return "[Room Pos=" + RoomX + "," + RoomY + " Type=" + Data.Type + "]";
		}
	}
}
