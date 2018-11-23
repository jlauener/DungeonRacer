﻿using System;
using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;

namespace DungeonRacer
{
	class GameScene : Scene
	{
		public event Action<GameScene, Room> OnEnterRoom;

		private enum State
		{
			Start,
			Play,
			Switch,
			Enter,
			Finished
		}
		private State state = State.Play;

		public bool TimePaused
		{
			get
			{
				return state != State.Play || currentRoom.Data.Type != RoomType.Normal;
			}
		}
		public float Time { get; private set; }

		public bool Finished
		{
			get { return state == State.Finished; }
		}

		public static Player Player { get; private set; }
		public static DungeonMap Map { get; private set;  }
		public static Shaker Shaker { get; private set; }

		private Room previousRoom;
		private Room currentRoom;
		private readonly Room[,] rooms;

		public GameScene(DungeonData dungeonData)
		{
			Map = new DungeonMap(dungeonData); ;
			Add(Map);

			rooms = new Room[dungeonData.Width, dungeonData.Height];
			dungeonData.IterateRooms((roomData) =>
			{
				var room = new Room(roomData, roomData.X, roomData.Y);
				rooms[roomData.X, roomData.Y] = room;
				Add(room);
				if (roomData.Type == RoomType.Start)
				{
					currentRoom = room;
				}
			});
			Camera.Position = GetCameraPosition(currentRoom);

			Player = new Player(PlayerData.Get("normal"), Map, dungeonData.PlayerStartTile.X, dungeonData.PlayerStartTile.Y, dungeonData.PlayerStartDirection);
			Add(Player);

			var uiCamera = Engine.CreateCamera();
			SetCamera(Global.LayerUi, uiCamera);

			var uiBack = new Sprite("gfx/ui/background");
			uiBack.Layer = Global.LayerUi;
			Add(uiBack);

			Add(new TimeWidget(this, Engine.HalfWidth, 3));

			Add(new CoinWidget(Player, 2, 2));
			Add(new HpWidget(Player, 3, 19));

			Add(new RoomWidget(this, Engine.Width - 2, 1));
			Add(new InventoryWidget(Player, 202, 14));

			Shaker = new Shaker(Camera);
			Add(Shaker);

			Engine.Track(this, "state");
			Engine.Track(this, "currentRoom");
		}

		protected override void OnBegin()
		{
			base.OnBegin();

			OnEnterRoom?.Invoke(this, currentRoom);
			currentRoom.Enter();
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			switch (state)
			{
				case State.Play:
					UpdatePlay(deltaTime);
					break;
				case State.Enter:
					UpdateEnter(deltaTime);
					break;
				case State.Finished:
					UpdateFinished(deltaTime);
					break;
			}

			if (Input.WasPressed("back"))
			{
				Engine.Quit();
			}

			if (Input.WasPressed("reset"))
			{
				Engine.Scene = new GameScene(Map.Data);
			}

			if (Input.WasPressed("debug_1"))
			{
				//Global.ScrollingEnabled = !Global.ScrollingEnabled;
				//Engine.Scene = new GameScene(DungeonData.Get(Global.ScrollingEnabled ? "dungeon_1" : "dungeon_room"));
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

		private void CheckRoomSwitch()
		{
			if (Player.Velocity.X < 0.0f && Player.X < currentRoom.Left + Global.RoomSwitchMargin)
			{
				GotoRoom(-1, 0);
			}
			else if (Player.Velocity.X > 0.0f && Player.X > currentRoom.Right - Global.RoomSwitchMargin)
			{
				GotoRoom(1, 0);
			}
			else if (Player.Velocity.Y < 0.0f && Player.Y < currentRoom.Top + Global.RoomSwitchMargin)
			{
				GotoRoom(0, -1);
			}
			else if (Player.Velocity.Y > 0.0f && Player.Y > currentRoom.Bottom - Global.RoomSwitchMargin)
			{
				GotoRoom(0, 1);
			}
		}

		private void UpdateEnter(float deltaTime)
		{
			CheckRoomSwitch();

			var x = currentRoom.Left + Global.TileSize + Global.HalfTileSize;
			var y = currentRoom.Top + Global.TileSize + Global.HalfTileSize;
			var width = Global.RoomWidthPx - 3 * Global.TileSize;
			var height = Global.RoomHeightPx - 3 * Global.TileSize;

			if (Player.InsideRect(x, y, width, height))
			{
				if (previousRoom != null) previousRoom.Leave();
				currentRoom.Enter();
				OnEnterRoom?.Invoke(this, currentRoom);
				state = State.Play;
			}

			if (!Player.Alive)
			{
				SetGameOver();
			}
		}

		private void UpdatePlay(float deltaTime)
		{
			CheckRoomSwitch();
			if (!TimePaused) Time += deltaTime;

			if (!Player.Alive)
			{
				SetGameOver();
			}
		}

		private void GotoRoom(int dx, int dy)
		{
			previousRoom = currentRoom;

			var x = currentRoom.RoomX + dx;
			var y = currentRoom.RoomY + dy;
			if (x < 0 || y < 0 || x >= rooms.GetLength(0) || y >= rooms.GetLength(1))
			{
				SetGameFinished();
				return;
			}

			currentRoom = rooms[x, y];

			Player.Paused = true;
			state = State.Switch;
			var target = GetCameraPosition(currentRoom);
			Shaker.StopAll();
			Shaker.Enabled = false;
			Tween(Camera, new { Position = target }, 0.5f).Ease(Ease.QuadInOut).OnComplete(() =>
			 {
				 Player.Paused = false;
				 state = State.Enter;
				 Shaker.ResetCameraOrigin();
				 Shaker.Enabled = true;
			 });

			Asset.LoadSoundEffect("sfx/room_switch").Play();
		}

		private Vector2 GetCameraPosition(Room room)
		{
			return new Vector2(room.X, room.Y - Global.UiHeight);
		}

		private void SetGameFinished()
		{
			state = State.Finished;

			var label = new Label(Global.Font, "FINISHED");
			label.Scale = 3.0f;
			label.Layer = Global.LayerUi;
			label.Center();
			Add(label, Engine.HalfWidth, Engine.HalfHeight);

			Input.Enabled = false;
			Callback(1.0f, () => Input.Enabled = true);
		}

		private void SetGameOver()
		{
			state = State.Finished;

			var label = new Label(Global.Font, "GAME OVER");
			label.Scale = 3.0f;
			label.Layer = Global.LayerUi;
			label.Center();
			Add(label, Engine.HalfWidth, Engine.HalfHeight);

			Input.Enabled = false;
			Callback(1.0f, () => Input.Enabled = true);
		}

		private void UpdateFinished(float deltaTime)
		{
			if (Input.WasPressed("start"))
			{
				Engine.Scene = new GameScene(DungeonData.Get("dungeon1"));
			}
		}

		protected override void OnRenderDebug(SpriteBatch spriteBatch)
		{
			base.OnRenderDebug(spriteBatch);

			var x = currentRoom.X * Global.RoomWidthPx + 2 * Global.TileSize;
			var y = currentRoom.Y * Global.RoomHeightPx + 2 * Global.TileSize;
			var width = Global.RoomWidthPx - 3 * Global.TileSize;
			var height = Global.RoomHeightPx - 3 * Global.TileSize;
			var rect = new RectangleF(x, y, width, height);
			spriteBatch.DrawRectangle(rect, Color.White);
		}
	}
}
