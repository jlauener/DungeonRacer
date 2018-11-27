using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoPunk;

namespace DungeonRacer
{
	class GameManager
	{
		public PlayerData Player { get; }

		private readonly Dictionary<string, GameScene> scenes = new Dictionary<string, GameScene>();

		private GameManager()
		{
			Player = PlayerData.Create("player");

			AddScene("menu");
			AddScene("dungeon1");
		}

		public void AddScene(string name)
		{
			scenes[name] = new GameScene(this, DungeonData.Get(name));
		}

		public void LoadScene(string name)
		{
			var scene = scenes[name];
			scene.InitPlayer(Player, scene.Data.PlayerStartTile, scene.Data.PlayerStartDirection);
			Engine.Scene = scene;
		}

		public static void StartNewGame()
		{
			var game = new GameManager();
			game.LoadScene("menu");
		}
	}
}
