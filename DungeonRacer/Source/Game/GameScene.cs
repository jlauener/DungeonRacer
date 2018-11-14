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

		public float Time { get; private set; }
		private readonly Player player;

		private readonly DungeonData dungeonData;

		private int roomX;
		private int roomY;

		private Dungeon dungeon;

		//private readonly Label timeLabel;
		//private readonly Bar hpBar;

		public GameScene(DungeonData dungeonData)
		{
			this.dungeonData = dungeonData;

			dungeon = new Dungeon(dungeonData); ;
			Add(dungeon);

			player = new Player(PlayerData.Get("normal"), dungeonData.PlayerStartTile);
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

			//timeLabel = new Label("font/04b");
			//timeLabel.Layer = Global.LayerUi;
			//timeLabel.HAlign = TextAlign.Center;
			//Add(timeLabel, Engine.Width / 2, 7);

			//var hpBarBack = new Sprite("gfx/ui/hp_bar_back");
			//hpBarBack.Layer = Global.LayerUi;
			//hpBarBack.X = 4;
			//hpBarBack.Y = 1;
			//Add(hpBarBack);

			//hpBar = new Bar("gfx/ui/hp_bar_front");
			//hpBar.Layer = Global.LayerUi;
			//Add(hpBar, hpBarBack.X + 2, hpBarBack.Y + 3);

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
				
				if (player.DriftPct > 0.0f)
				{
					dungeon.DrawDriftEffect(player.X, player.Y, player.DriftPct, player.Angle);
				}
			}

			//hpBar.Percent = player.Hp / player.MaxHp;
			//timeLabel.Text = Time.ToString("0.00");

			if (Input.WasPressed("back"))
			{
				Engine.Quit();
			}

			if (Input.WasPressed("reset"))
			{
				Engine.Scene = new GameScene(dungeonData);
			}

			if (Input.WasPressed("debug_1"))
			{
				Global.ScrollingEnabled = !Global.ScrollingEnabled;
				Engine.Scene = new GameScene(dungeonData);
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
			Tween(Camera, new { Position = target }, 1.0f).Ease(Ease.QuadInOut).OnComplete(() =>
			 {
				 player.Paused = false;
				 state = State.Play;
			 });

			Asset.LoadSoundEffect("sfx/room_switch").Play();
		}

		private Vector2 GetCameraPosition(int roomX, int roomY)
		{
			var pos = new Vector2(roomX * Global.RoomWidthPx + Global.TileSize / 2, roomY * Global.RoomHeightPx);
			if(!Global.ScrollingEnabled)
			{
				pos.Y -= Global.UiHeight - Global.TileSize / 2;
			}
			return pos;
		}
	}
}
