using System;
using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;

namespace DungeonRacer
{
	class GameScene : Scene
	{
		public DungeonData Data { get; }

		private enum State
		{
			Start,
			Play,
			Finished
		}
		private State state = State.Play;

		public bool TimePaused
		{
			get
			{
				return state != State.Play;
			}
		}
		public float Time { get; private set; } = 60.0f;

		public bool Finished
		{
			get { return state == State.Finished; }
		}

		private readonly GameManager game;
		private Player player;
		private Vector2 cameraPosition;

		public GameScene(GameManager game, DungeonData dungeonData)
		{
			this.game = game;
			Data = dungeonData;

			var dungeonMap = new DungeonMap(dungeonData);
			Add(dungeonMap);

			var uiCamera = Engine.CreateCamera();
			SetCamera(Global.LayerUi, uiCamera);

			//Add(new TimeWidget(this, Engine.HalfWidth, 0));
			Add(new ComboWidget(game.Player, Engine.HalfWidth, 1));

			Add(new CoinWidget(game.Player, Engine.Width - 2, 1));
			Add(new HpWidget(game.Player, 2, 2));

			//Add(new RoomWidget(this, Engine.Width - 2, 1));
			Add(new InventoryWidget(game.Player, 202, 2));

			Add(new Shaker());

			//Engine.Track(this, "state");
		}

		public void InitPlayer(PlayerData data, DungeonTile tile, Direction direction)
		{
			if(player != null)
			{
				Remove(player);
			}

			player = new Player(data, tile, direction);
			Add(player);
			cameraPosition = player.Position - new Vector2(Engine.HalfWidth, Engine.HalfHeight);
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			switch (state)
			{
				case State.Play:
					UpdatePlay(deltaTime);
					break;
				//case State.Enter:
				//	UpdateEnter(deltaTime);
				//	break;
				case State.Finished:
					UpdateFinished(deltaTime);
					break;
			}

			Camera.Position = cameraPosition + GetEntity<Shaker>().Offset;

			if (Input.WasPressed("back"))
			{
				Engine.Quit();
			}

			if (Input.WasPressed("reset"))
			{
				GameManager.StartNewGame();
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
				//Engine.Scene = new GameScene(DungeonData.Get("test"));
			}
		}

		public void AddTime(float value)
		{
			Time += value;
		}

		//private void CheckRoomSwitch()
		//{
		//	if (Player.Velocity.X < 0.0f && Player.X < currentRoom.Left + Global.RoomSwitchMargin)
		//	{
		//		GotoRoom(-1, 0);
		//	}
		//	else if (Player.Velocity.X > 0.0f && Player.X > currentRoom.Right - Global.RoomSwitchMargin)
		//	{
		//		GotoRoom(1, 0);
		//	}
		//	else if (Player.Velocity.Y < 0.0f && Player.Y < currentRoom.Top + Global.RoomSwitchMargin)
		//	{
		//		GotoRoom(0, -1);
		//	}
		//	else if (Player.Velocity.Y > 0.0f && Player.Y > currentRoom.Bottom - Global.RoomSwitchMargin)
		//	{
		//		GotoRoom(0, 1);
		//	}
		//}

		//private void UpdateEnter(float deltaTime)
		//{
		//	CheckRoomSwitch();

		//	var x = currentRoom.Left + Global.TileSize + Global.HalfTileSize;
		//	var y = currentRoom.Top + Global.TileSize + Global.HalfTileSize;
		//	var width = Global.RoomWidthPx - 3 * Global.TileSize;
		//	var height = Global.RoomHeightPx - 3 * Global.TileSize;

		//	if (Player.InsideRect(x, y, width, height))
		//	{
		//		if (previousRoom != null) previousRoom.Leave();
		//		currentRoom.Enter();
		//		OnEnterRoom?.Invoke(this, currentRoom);
		//		state = State.Play;
		//	}

		//	if (!Player.Alive)
		//	{
		//		SetGameOver();
		//	}
		//}

		private void UpdatePlay(float deltaTime)
		{
			var cameraEdge = new Vector2(96.0f, 96.0f);

			if (player.X < cameraPosition.X + cameraEdge.X) cameraPosition.X = player.X - cameraEdge.X;
			else if (player.X > cameraPosition.X + Engine.Width - cameraEdge.X) cameraPosition.X = player.X - Engine.Width + cameraEdge.X;

			if (player.Y < cameraPosition.Y + cameraEdge.Y) cameraPosition.Y = player.Y - cameraEdge.Y;
			else if (player.Y > cameraPosition.Y + Engine.Height - cameraEdge.Y) cameraPosition.Y = player.Y - Engine.Height + cameraEdge.Y;

			var map = GetEntity<DungeonMap>();
			cameraPosition.X = Mathf.Clamp(cameraPosition.X, 0.0f, map.Width - Engine.Width);
			cameraPosition.Y = Mathf.Clamp(cameraPosition.Y, 0.0f, map.Height - Engine.Height);

			if (!TimePaused) Time -= deltaTime;

			if (!GetEntity<Player>().Alive)
			{
				SetGameOver("CAR BROKEN!");
			}
			else if (Time <= 0.0f)
			{
				//SetGameOver("TIME'S UP!");
			}
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

		private void SetGameOver(string reason)
		{
			state = State.Finished;

			var label = new Label(Global.Font, reason);
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
				GameManager.StartNewGame();
			}
		}

		public void GotoDungeon(DungeonData dungeon)
		{
			// TODO transition
			game.LoadScene(dungeon.Name);
			Asset.LoadSoundEffect("sfx/room_switch").Play();
		}

		public void ShowInfo(string text)
		{
			Log.Debug("INFO " + text);
		}

		//protected override void OnRenderDebug(SpriteBatch spriteBatch)
		//{
		//	base.OnRenderDebug(spriteBatch);

		//	var x = currentRoom.X * Global.RoomWidthPx + 2 * Global.TileSize;
		//	var y = currentRoom.Y * Global.RoomHeightPx + 2 * Global.TileSize;
		//	var width = Global.RoomWidthPx - 3 * Global.TileSize;
		//	var height = Global.RoomHeightPx - 3 * Global.TileSize;
		//	var rect = new RectangleF(x, y, width, height);
		//	spriteBatch.DrawRectangle(rect, Color.White);
		//}
	}
}
