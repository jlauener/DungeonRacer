using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class PlayerData
	{
		public string Name { get; }
		public int Hp { get; }
		public int SpriteId { get; }

		public float Friction { get; private set; }
		public float FrontGearForce { get; private set; }
		public float RearGearForce { get; private set; }
		public float BreakFriction { get; private set; }
		public float TurnSpeed { get; private set; }
		public float AngularFriction { get; private set; }


		private PlayerData(string name, int hp, int spriteId)
		{
			Name = name;
			Hp = hp;
			SpriteId = spriteId;
		}

		private static PlayerData Create(string name, int hp, int spriteId)
		{
			if (store.ContainsKey(name))
			{
				throw new Exception("PlayerData with name '" + name + "' already exists.");
			}

			var playerData = new PlayerData(name, hp, spriteId);
			store[name] = playerData;
			return playerData;
		}

		private static readonly Dictionary<string, PlayerData> store = new Dictionary<string, PlayerData>();

		public static void Init()
		{
			Asset.AddTileset("player", "gfx/player", 16, 16);

			PlayerData p;

			p = Create("adventure", 100, 1);
			p.Friction = 0.92f;
			p.FrontGearForce = 480.0f;
			p.BreakFriction = 0.98f;
			p.RearGearForce = 280.0f;
			p.TurnSpeed = 0.5f;
			p.AngularFriction = 0.8f;

			p = Create("race", 80, 0);
			p.Friction = 0.99f;
			p.FrontGearForce = 460.0f;
			p.BreakFriction = 0.98f;
			p.RearGearForce = 460.0f;
			p.TurnSpeed = 2.5f;
			p.AngularFriction = 0.25f;
		}

		public static PlayerData Get(string name)
		{
			PlayerData playerData;
			if (!store.TryGetValue(name, out playerData))
			{
				throw new Exception("PlayerData with name '" + name + "' not found.");
			}
			return playerData;
		}
	}
}
