using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoPunk;

namespace DungeonRacer
{
	class Room : Entity
	{
		public event Action<Room> OnEnter;
		public event Action<Room> OnLeave;

		public RoomData Data { get; }
		public int RoomX { get; }
		public int RoomY { get; }

		public bool Cleared { get { return enemies.Count == 0; } }

		public bool Active { get; private set; }
		private bool initialized;

		private int ghostCount;

		// TODO make this more generic, win condition?
		private readonly List<Enemy> enemies = new List<Enemy>();

		private readonly List<Door> doors = new List<Door>();
		//private readonly List<Ghost> ghosts = new List<Ghost>();

		public Room(RoomData data, int roomX, int roomY)
		{
			Data = data;
			RoomX = roomX;
			RoomY = roomY;

			X = roomX * Global.RoomWidthPx + Global.TileSize / 2;
			Y = roomY * Global.RoomHeightPx +  Global.TileSize / 2;
			Width = Global.RoomWidthPx;
			Height = Global.RoomHeightPx;
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Data.IterateEntities((args) =>
			{
				var entity = GameEntity.Create(this, args);
				if (entity is Enemy)
				{
					enemies.Add((Enemy)entity);
					Scene.Add(entity);
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

			ghostCount = enemies.Count / 2;
		}

		public void Enter()
		{
			Log.Debug("enter " + this);

			if (!initialized)
			{
				foreach (var door in doors) Scene.Add(door);
				//foreach (var enemy in enemies) Scene.Add(enemy);
				initialized = true;
			}

			//if (ghostCount > 0 && Cleared)
			//{
			//	for(var i = 0; i < ghostCount; i++)
			//	{
			//		var pos = Vector2.Zero;
			//		pos.X = Rand.NextFloat(Left + Global.TileSize * 3, Right - Global.TileSize * 3);
			//		pos.Y = Rand.NextFloat(Top + Global.TileSize * 3, Bottom - Global.TileSize * 2);
			//		var ghost = new Ghost(this, pos);
			//		ghosts.Add(ghost);
			//		Scene.Add(ghost);
			//	}
			//}

			Active = true;
			OnEnter?.Invoke(this);
		}

		public void Leave()
		{
			Log.Debug("leave " + this);

			//foreach (var ghost in ghosts)
			//{
			//	Scene.Remove(ghost);
			//}
			//ghosts.Clear();

			if (ghostCount > 0)
			{
				for (var i = 0; i < ghostCount; i++)
				{
					var pos = Vector2.Zero;
					pos.X = Rand.NextFloat(Left + Global.TileSize * 3, Right - Global.TileSize * 3);
					pos.Y = Rand.NextFloat(Top + Global.TileSize * 3, Bottom - Global.TileSize * 2);
					//var ghost = new Ghost(this, pos);
					//ghosts.Add(ghost);
					Scene.Add(new Ghost(this, pos));
				}
			}
			ghostCount = 0;

			Active = false;
			OnLeave?.Invoke(this);
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!Active) return;

			if (!Cleared)
			{
				for (var i = enemies.Count - 1; i >= 0; i--)
				{
					if (!enemies[i].Alive)
					{
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

		private void SpawnGhost()
		{
			var pos = Vector2.Zero;
			pos.X = Rand.NextFloat(Left + Global.TileSize * 3, Right - Global.TileSize * 3);
			pos.Y = Rand.NextFloat(Top + Global.TileSize * 3, Bottom - Global.TileSize * 2);
			Scene.Add(new Ghost(this, pos));
		}

		public override string ToString()
		{
			return "[Room Pos=" + RoomX + "," + RoomY + " Type=" + Data.Type + "]";
		}
	}
}
