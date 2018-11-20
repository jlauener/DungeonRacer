using System;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class GameScene : Scene
	{
		private enum State
		{
			Start,
			Play,
			Switch
		}
		private State state = State.Play;

		public bool Paused { get { return state != State.Play; } }
		public float Time { get; private set; }

		private readonly Player player;
		private readonly Dungeon dungeon;

		private int roomX;
		private int roomY;

		public GameScene(DungeonData dungeonData)
		{
			dungeon = new Dungeon(dungeonData); ;
			Add(dungeon);

			player = new Player(PlayerData.Get("normal"), dungeonData.PlayerStartTile, dungeon);
			Add(player);

			roomX = dungeonData.PlayerStartTile.X / Global.RoomWidth;
			roomY = dungeonData.PlayerStartTile.Y / Global.RoomHeight;
			Camera.Position = GetCameraPosition(roomX, roomY);

			var uiCamera = Engine.CreateCamera();
			SetCamera(Global.LayerUi, uiCamera);

			if (!Global.ScrollingEnabled)
			{
				var uiBack = new RectangleShape(Engine.Width, 32, Color.Black);
				uiBack.Layer = Global.LayerUi;
				Add(uiBack);
			}

			Add(new HpWidget(player, 1, 1));
			Add(new MpWidget(player, 1, 12));
			Add(new TimeWidget(this, Engine.HalfWidth, 2));
			Add(new InventoryWidget(player, Engine.Width - 80, 4));
			Add(new CoinWidget(player, dungeon.Data.CoinCount, Engine.Width - 42, 4));

			//Add(new Shaker(Camera));

			Engine.Track(this, "roomX");
			Engine.Track(this, "roomY");
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (state == State.Play)
			{
				if (Global.ScrollingEnabled)
				{
					UpdateScrollingCamera(deltaTime);
				}
				else
				{
					UpdateRoomCamera(deltaTime);
				}

				Time += deltaTime;
			}

			if (Input.WasPressed("back"))
			{
				Engine.Quit();
			}

			if (Input.WasPressed("reset"))
			{
				Engine.Scene = new GameScene(dungeon.Data);
			}

			if (Input.WasPressed("debug_1"))
			{
				Global.ScrollingEnabled = !Global.ScrollingEnabled;
				Engine.Scene = new GameScene(DungeonData.Get(Global.ScrollingEnabled ? "dungeon_1" : "dungeon_room"));
			}

			if (Input.WasPressed("debug_2"))
			{
				Global.CrtEnabled = !Global.CrtEnabled;
				Engine.PostProcessor = Global.CrtEnabled ? Boot.CrtEffect : null;
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
				Engine.Scene = new GameScene(DungeonData.Get("test"));
			}
		}

		private void UpdateScrollingCamera(float deltaTime)
		{
			var cameraEdge = new Vector2(96.0f, 96.0f);

			if (player.X < Camera.X + cameraEdge.X) Camera.X = player.X - cameraEdge.X;
			else if (player.X > Camera.X + Engine.Width - cameraEdge.X) Camera.X = player.X - Engine.Width + cameraEdge.X;

			if (player.Y < Camera.Y + cameraEdge.Y) Camera.Y = player.Y - cameraEdge.Y;
			else if (player.Y > Camera.Y + Engine.Height - cameraEdge.Y) Camera.Y = player.Y - Engine.Height + cameraEdge.Y;

			Camera.X = Mathf.Clamp(Camera.X, 0.0f, dungeon.Width - Engine.Width);
			Camera.Y = Mathf.Clamp(Camera.Y, 0.0f, dungeon.Height - Engine.Height);
		}

		private void UpdateRoomCamera(float deltaTime)
		{
			if (player.Velocity.X < 0.0f && player.X < roomX * Global.RoomWidthPx + Global.TileSize / 2 + Global.RoomSwitchMargin)
			{
				GotoRoom(-1, 0);
			}
			else if (player.Velocity.X > 0.0f && player.X > roomX * Global.RoomWidthPx + Global.RoomWidthPx + Global.TileSize / 2 - Global.RoomSwitchMargin)
			{
				GotoRoom(1, 0);
			}
			else if (player.Velocity.Y < 0.0f && player.Y < roomY * Global.RoomHeightPx + Global.TileSize / 2 + Global.RoomSwitchMargin)
			{
				GotoRoom(0, -1);
			}
			else if (player.Velocity.Y > 0.0f && player.Y > roomY * Global.RoomHeightPx + Global.RoomHeightPx + Global.TileSize / 2 - Global.RoomSwitchMargin)
			{
				GotoRoom(0, 1);
			}
		}

		private void GotoRoom(int dx, int dy)
		{
			roomX += dx;
			roomY += dy;
			player.Paused = true;
			state = State.Switch;
			var target = GetCameraPosition(roomX, roomY);
			Tween(Camera, new { Position = target }, 0.5f).Ease(Ease.QuadInOut).OnComplete(() =>
			 {
				 player.Paused = false;
				 state = State.Play;
			 });

			Asset.LoadSoundEffect("sfx/car_room_switch").Play();
		}

		private Vector2 GetCameraPosition(int roomX, int roomY)
		{
			var pos = new Vector2(roomX * Global.RoomWidthPx + Global.TileSize / 2, roomY * Global.RoomHeightPx);
			if (!Global.ScrollingEnabled)
			{
				pos.Y -= Global.UiHeight - Global.TileSize / 2;
			}
			return pos;
		}
	}
}
