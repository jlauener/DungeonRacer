using System;
using System.Collections.Generic;
using MicroPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class GameScene : Scene
	{
		private readonly Player player;

		private Room currentRoom;

		private readonly Label playerLabel;
		private readonly Label roomLabel;
		private readonly Bar hpBar;

		public GameScene()
		{
			var uiCamera = Engine.CreateCamera();
			uiCamera.Zoom = Global.Scale;
			uiCamera.Origin = new Vector2(0.0f, 0.0f);
			SetCamera(Global.LayerUi, uiCamera);

			currentRoom = new Room(GetRandomRoomData(RoomType.Initial));
			Add(currentRoom);

			Camera.Position = GetCameraPosition(currentRoom);

			player = new Player(PlayerData.Get("adventure"), Global.RoomWidthPx / 2, Global.RoomHeightPx / 2);
			Add(player);

			var uiBack = new RectangleShape(Engine.Width, 32, Color.Black);
			uiBack.Layer = Global.LayerUi;
			Add(uiBack);

			playerLabel = new Label(Global.Font);
			playerLabel.Layer = Global.LayerUi;
			Add(playerLabel, 2, 2);

			roomLabel = new Label(Global.Font);
			roomLabel.Layer = Global.LayerUi;
			roomLabel.HAlign = TextAlign.Right;
			Add(roomLabel, Engine.Width - 4, 2);

			var hpBarBack = new Sprite("gfx/ui/hp_bar_back");
			hpBarBack.Layer = Global.LayerUi;
			hpBarBack.X = 4;
			hpBarBack.Y = 9;
			Add(hpBarBack);

			hpBar = new Bar("gfx/ui/hp_bar_front");
			hpBar.Layer = Global.LayerUi;
			Add(hpBar, hpBarBack.X + 2, hpBarBack.Y + 3);

			UpdateRoomLabel();
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!player.Paused)
			{
				if (player.VelocityX < 0.0f && player.X < currentRoom.Left + Global.TileSizePx / 2)
				{
					GotoRoom(-1, 0, DoorId.Left);
				}
				else if (player.VelocityX > 0.0f && player.X > currentRoom.Right - Global.TileSizePx / 2)
				{
					GotoRoom(1, 0, DoorId.Right);
				}
				else if (player.VelocityY < 0.0f && player.Y < currentRoom.Top + Global.TileSizePx / 2)
				{
					GotoRoom(0, -1, DoorId.Up);
				}
				else if (player.VelocityY > 0.0f && player.Y > currentRoom.Bottom - Global.TileSizePx / 2)
				{
					GotoRoom(0, 1, DoorId.Down);
				}
			}

			//playerLabel.Text = "hp " + player.Hp + " money: " + player.Money + " key: " + player.Key;

			hpBar.Percent = player.Hp / player.MaxHp;

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
				player.SetData(PlayerData.Get("adventure"));
			}

			if (Input.WasPressed("debug_2"))
			{
				player.SetData(PlayerData.Get("race"));
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
				nextRoom.X += currentRoom.X + dx * (Global.RoomWidthPx - Global.TileSizePx);
				nextRoom.Y += currentRoom.Y + dy * (Global.RoomHeightPx - Global.TileSizePx);
				currentRoom.SetNextRoom(doorId, nextRoom);
			}
			Add(nextRoom);

			var target = GetCameraPosition(nextRoom);
			player.Paused = true;
			Tween(Camera, new { Position = GetCameraPosition(nextRoom) }, 1.0f).Ease(Ease.QuadInOut).OnComplete(() =>
			 {
				 UpdateRoomLabel();
				 player.Paused = false;
				 Remove(currentRoom);
				 currentRoom = nextRoom;
			 });
		}

		private RoomData GetRandomRoomData(params RoomType[] types)
		{
			var candidates = new List<RoomData>();
			foreach (var type in types)
			{
				candidates.AddRange(RoomData.GetRooms(type));
			}
			return Rand.GetRandomElement(candidates);
		}

		private Room CreateRandomNextRoom(DoorId doorId)
		{
			RoomData data;
			bool flipX = false;
			bool flipY = false;
			switch (doorId)
			{
				case DoorId.Left:
					data = GetRandomRoomData(RoomType.Horizontal, RoomType.HorizontalTurn);
					flipX = true;
					break;
				case DoorId.Right:
					data = GetRandomRoomData(RoomType.Horizontal, RoomType.HorizontalTurn);
					break;
				case DoorId.Up:
					data = GetRandomRoomData(RoomType.Vertical, RoomType.VerticalTurn);
					flipY = true;
					break;
				default: // DoorId.Down
					data = GetRandomRoomData(RoomType.Vertical, RoomType.VerticalTurn);
					break;
			}

			if (data.Type == RoomType.HorizontalTurn)
			{
				flipY = Rand.NextBool();
			}
			else if (data.Type == RoomType.VerticalTurn)
			{
				flipX = Rand.NextBool();
			}

			return new Room(data, flipX, flipY);
		}

		private Vector2 GetCameraPosition(Room room)
		{
			return new Vector2(room.X + Global.TileSizePx / 2, room.Y - Global.UiHeight + Global.TileSizePx / 2);
		}

		private void UpdateRoomLabel()
		{
			//roomLabel.Text = (roomX + 1) + "-" + char.ConvertFromUtf32('A' + roomY);
		}
	}
}
