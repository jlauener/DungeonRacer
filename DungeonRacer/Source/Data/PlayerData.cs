using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class PlayerData
	{
		public const float BounceRestitution = 1.0f;
		public const float AngleResolution = 32.0f;
		public const float InvincibleDuration = 0.8f;
		public const float InvincibleBlinkInterval = 0.15f;

		public string Name { get; }
		public int Hp { get; private set; }
		public int Mp { get; private set; }
		public PixelMask PixelMask { get; private set; }
		public AnimatorData Anim { get; private set; }

		public float Friction { get; private set; }
		public float FrontGearForce { get; private set; }
		public float EngineSpeed { get; private set; }
		public float EngineDecay { get; private set; }
		public float RearGearForce { get; private set; }		
		public float BreakFriction { get; private set; }
		public float TurnSpeed { get; private set; }
		public float AngularFriction { get; private set; }
		public float BoostForce { get; private set; }
		public float BoostManaPerSec { get; private set; }

		private PlayerData(string name)
		{
			Name = name;
		}

		private static PlayerData Create(string name)
		{
			if (store.ContainsKey(name))
			{
				throw new Exception("PlayerData with name '" + name + "' already exists.");
			}

			var playerData = new PlayerData(name);
			store[name] = playerData;
			return playerData;
		}

		private static readonly Dictionary<string, PlayerData> store = new Dictionary<string, PlayerData>();

		public static void Init()
		{
			PlayerData p;

			p = Create("normal");
			p.Hp = 5;
			p.Mp = 5;
			p.Friction = 0.92f;
			p.FrontGearForce = 600.0f;
			p.EngineSpeed = 1.75f;
			p.EngineDecay = 0.95f;
			p.BreakFriction = 0.98f;
			p.RearGearForce = 280.0f;
			p.TurnSpeed = 4.0f;
			p.AngularFriction = 0.0f;
			p.BoostForce = 1100.0f;
			p.BoostManaPerSec = 30.0f;
			p.PixelMask = new PixelMask("gfx/mask/player_mask");
			p.Anim = new AnimatorData("gfx/game/player", 16, 16);
			p.Anim.Add("idle", 0);
			p.Anim.Add("blood1", 1);
			p.Anim.Add("blood2", 2);
			p.Anim.Add("blood3", 3);
			p.Anim.Add("hurt", AnimatorMode.OneShot, 0.1f, 7);
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
