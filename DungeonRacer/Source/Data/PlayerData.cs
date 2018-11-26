using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class PlayerData
	{
		public const float BounceRestitution = 1.5f;
		public const float BounceFriction = 0.85f;

		public const float AngleResolution = 32.0f;
		public const float InvincibleDuration = 0.8f;
		public const float InvincibleBlinkInterval = 0.15f;

		public const float WallDamageSpeedMin = 0.6f;

		public const float SpikeDamageSpeedMin = 10.0f;

		public const int EntityDamageFeedbackMax = 30;

		public const float MinimumHitSpeed = 20.0f;

		public const int BloodFrontMax = 4;
		public const int BloodBackMax = 4;

		public string Name { get; }
		public int Hp { get; private set; }
		public PixelMask PixelMask { get; private set; }
		public AnimatorData Anim { get; private set; }

		public float Friction { get; private set; }
		public float MaxSpeed { get; private set; }
		public float FrontGearForce { get; private set; }
		public float FrontGearSpeed { get; private set; }
		public float RearGearForce { get; private set; }
		public float RearGearSpeed { get; private set; }
		public float EngineDecay { get; private set; }
		public float BreakFriction { get; private set; }
		public float TurnSpeed { get; private set; }
		public float AngularFriction { get; private set; }

		public int WallDamageMin { get; private set; }
		public int WallDamageMax { get; private set; }

		public int SpikeDamage { get; private set; }
		public float SpikeImmuneDuration { get; private set; }

		public int LavaDamage { get; private set; }
		public float LavaImmuneDuration { get; private set; }

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
			p.Hp = 100;
			p.Friction = 0.92f;
			p.MaxSpeed = 125.0f;
			p.FrontGearForce = 650.0f;
			p.FrontGearSpeed = 1.8f;
			p.RearGearForce = 300.0f;
			p.RearGearSpeed = 1.5f;
			p.EngineDecay = 0.95f;
			p.BreakFriction = 0.98f;
			p.TurnSpeed = 5.0f;
			p.AngularFriction = 0.0f;

			p.WallDamageMin = 1;
			p.WallDamageMax = 10;

			p.SpikeDamage = 5;
			p.SpikeImmuneDuration = 0.5f;

			p.LavaDamage = 3;
			p.LavaImmuneDuration = 0.25f;

			p.PixelMask = Asset.GetPixelMask("circle_small");
			p.Anim = new AnimatorData("gfx/game/player", 16, 16);
			p.Anim.Add("idle", 0);
			p.Anim.Add("hurt", 7);
			p.Anim.Add("blood_front_1", 8);
			p.Anim.Add("blood_front_2", 9);
			p.Anim.Add("blood_front_3", 10);
			p.Anim.Add("blood_front_4", 11);
			p.Anim.Add("blood_back_1", 16);
			p.Anim.Add("blood_back_2", 17);
			p.Anim.Add("blood_back_3", 18);
			p.Anim.Add("blood_back_4", 19);
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
