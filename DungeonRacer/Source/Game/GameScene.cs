using System;
using System.Collections.Generic;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class GameScene : Scene
	{
		public float TimeLeft { get; private set; } = Global.PlayerTimeInitial;
		private readonly Player player;

		private int roomIndex;
		private Room currentRoom;

		private readonly Label timeLabel;
		private readonly Bar hpBar;

		public GameScene()
		{
			var uiCamera = Engine.CreateCamera();
			SetCamera(Global.LayerUi, uiCamera);

			currentRoom = new Room(RoomData.GetRandom(RoomFlags.Initial), RoomFlags.Initial);
			Add(currentRoom);

			Camera.Position = GetCameraPosition(currentRoom);

			player = new Player(PlayerData.Get("normal"), Global.RoomWidthPx / 2, Global.RoomHeightPx / 2);
			Add(player);

			var uiBack = new RectangleShape(Engine.Width, 32, Color.Black);
			uiBack.Layer = Global.LayerUi;
			Add(uiBack);

			timeLabel = new Label("font/04b");
			timeLabel.Layer = Global.LayerUi;
			timeLabel.HAlign = TextAlign.Center;
			Add(timeLabel, Engine.Width / 2, 7);

			var hpBarBack = new Sprite("gfx/ui/hp_bar_back");
			hpBarBack.Layer = Global.LayerUi;
			hpBarBack.X = 4;
			hpBarBack.Y = 1;
			Add(hpBarBack);

			hpBar = new Bar("gfx/ui/hp_bar_front");
			hpBar.Layer = Global.LayerUi;
			Add(hpBar, hpBarBack.X + 2, hpBarBack.Y + 3);
		}

		public void AddTime(float duration)
		{
			TimeLeft += duration;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!player.Paused)
			{
				if (player.VelocityX < 0.0f && player.X < currentRoom.Left + Global.TileSize / 2 + Global.RoomSwitchMargin)
				{
					GotoRoom(-1, 0, DoorId.Left);
				}
				else if (player.VelocityX > 0.0f && player.X > currentRoom.Right - Global.TileSize / 2 - Global.RoomSwitchMargin)
				{
					GotoRoom(1, 0, DoorId.Right);
				}
				else if (player.VelocityY < 0.0f && player.Y < currentRoom.Top + Global.TileSize / 2 + Global.RoomSwitchMargin)
				{
					GotoRoom(0, -1, DoorId.Up);
				}
				else if (player.VelocityY > 0.0f && player.Y > currentRoom.Bottom - Global.TileSize / 2 - Global.RoomSwitchMargin)
				{
					GotoRoom(0, 1, DoorId.Down);
				}

				if (!currentRoom.Data.HasFlag(RoomFlags.Initial))
				{
					TimeLeft -= deltaTime;
					if (TimeLeft <= 0.0f)
					{
						TimeLeft = 0.0f;
					}
				}
			}

			hpBar.Percent = player.Hp / player.MaxHp;
			timeLabel.Text = TimeLeft.ToString("0.00");

			if (Input.WasPressed("back"))
			{
				Engine.Quit();
			}

			if (Input.WasPressed("reset"))
			{
				Engine.Scene = new GameScene();
			}

			if (Input.WasPressed("debug_1"))
			{
			}

			if (Input.WasPressed("debug_2"))
			{
			}

			if (Input.WasPressed("debug_3"))
			{
			}

			if (Input.WasPressed("debug_4"))
			{
			}

			if (Input.WasPressed("debug_5"))
			{
			}

			if (Input.WasPressed("debug_6"))
			{
			}

			if (Input.WasPressed("debug_7"))
			{
			}

			if (Input.WasPressed("debug_8"))
			{
			}

			if (Input.WasPressed("debug_9"))
			{
			}

			if (Input.WasPressed("debug_10"))
			{
			}
		}

		private void GotoRoom(int dx, int dy, DoorId doorId)
		{
			var nextRoom = currentRoom.GetNextRoom(doorId);
			if (nextRoom == null)
			{
				nextRoom = CreateRandomNextRoom(doorId);
				nextRoom.X += currentRoom.X + dx * (Global.RoomWidthPx - Global.TileSize);
				nextRoom.Y += currentRoom.Y + dy * (Global.RoomHeightPx - Global.TileSize);
				currentRoom.SetNextRoom(doorId, nextRoom);
			}
			Add(nextRoom);

			var target = GetCameraPosition(nextRoom);
			player.Paused = true;
			Tween(Camera, new { Position = GetCameraPosition(nextRoom) }, 1.0f).Ease(Ease.QuadInOut).OnComplete(() =>
			 {
				 player.Paused = false;
				 Remove(currentRoom);
				 currentRoom = nextRoom;
			 });
		}

		private Room CreateRandomNextRoom(DoorId doorId)
		{
			var flags = RoomFlags.None;

			roomIndex++;
			if (roomIndex % 6 == 0) flags |= RoomFlags.ExtraTime;
			if ((roomIndex + 2) % 6 == 0) flags |= RoomFlags.Spike;
			if ((roomIndex + 4) % 6 == 0) flags |= RoomFlags.Bonus;

			bool flipX = false;
			bool flipY = false;
			switch (doorId)
			{
				case DoorId.Left:
					flags |= RoomFlags.Horizontal;
					flipX = true;
					break;
				case DoorId.Right:
					flags |= RoomFlags.Horizontal;
					break;
				case DoorId.Up:
					flags |= RoomFlags.Vertical;
					flipY = true;
					break;
				default: // DoorId.Down
					flags |= RoomFlags.Vertical;
					break;
			}

			Log.Debug("Querying room with flags " + flags + ".");
			var data = RoomData.GetRandom(flags);
			Log.Debug("Found " + data);
			return new Room(data, flags, flipX, flipY);
		}

		private Vector2 GetCameraPosition(Room room)
		{
			return new Vector2(room.X + Global.TileSize / 2, room.Y - Global.UiHeight + Global.TileSize / 2);
		}
	}
}
