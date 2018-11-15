using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class PlayerData
	{
		public const float BounceRestitution = 1.0f;

		public string Name { get; }
		public int Hp { get; }
		public PixelMask PixelMask { get; private set; }
		public AnimatorData Anim { get; private set; }

		public float Friction { get; private set; }
		public float FrontGearForce { get; private set; }
		public float RearGearForce { get; private set; }
		public float BreakFriction { get; private set; }
		public float TurnSpeed { get; private set; }
		public float AngularFriction { get; private set; }


		private PlayerData(string name, int hp)
		{
			Name = name;
			Hp = hp;
		}

		private static PlayerData Create(string name, int hp)
		{
			if (store.ContainsKey(name))
			{
				throw new Exception("PlayerData with name '" + name + "' already exists.");
			}

			var playerData = new PlayerData(name, hp);
			store[name] = playerData;
			return playerData;
		}

		private static readonly Dictionary<string, PlayerData> store = new Dictionary<string, PlayerData>();

		public static void Init()
		{
			PlayerData p;

			p = Create("normal", 100);
			p.Friction = 0.92f;
			p.FrontGearForce = 480.0f;
			p.BreakFriction = 0.98f;
			p.RearGearForce = 280.0f;
			p.TurnSpeed = 0.5f;
			p.AngularFriction = 0.8f;
			p.PixelMask = new PixelMask("gfx/game/player_mask");
			p.Anim = new AnimatorData("gfx/game/player", 16, 16);
			p.Anim.Add("idle", 0);
		}

		public static PlayerData Get(string name)
		{
			if (!store.TryGetValue(name, out PlayerData playerData))
			{
				throw new Exception("PlayerData with name '" + name + "' not found.");
			}
			return playerData;
		}
	}
}
