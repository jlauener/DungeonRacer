using System;
using System.Collections.Generic;
using MonoPunk;

namespace DungeonRacer
{
	class PlayerData
	{
		public event Action<PlayerData, int, DamageType> OnDamage;
		public event Action<PlayerData, int> OnHeal;

		public event Action<PlayerData, ItemType> OnCollect;
		public event Action<PlayerData, ItemType> OnUse;

		public event Action<PlayerData, EntityData, int> OnCombo;
		public event Action<PlayerData> OnComboStop;

		public const float BounceRestitution = 1.5f;
		public const float BounceFriction = 0.85f;

		public const float AngleResolution = 32.0f;
		public const float InvincibleDuration = 0.8f;
		public const float InvincibleBlinkInterval = 0.15f;

		public const float WallDamageSpeedMin = 0.6f;

		public const float SpikeDamageSpeedMin = 10.0f;

		public const int EntityDamageFeedbackMax = 30;

		public const float MinimumHitSpeed = 20.0f;
		public const float ComboWindow = 1.0f;

		public const int BloodFrontMax = 4;
		public const int BloodBackMax = 4;

		public string Name { get; }
		public int Hp { get; private set; }
		public int MaxHp { get; private set; }

		public PixelMask PixelMask { get; private set; }
		public AnimatorData Anim { get; private set; }

		public float Friction { get; set; }
		public float MaxSpeed { get; set; }
		public float FrontGearForce { get; set; }
		public float FrontGearSpeed { get; set; }
		public float RearGearForce { get; set; }
		public float RearGearSpeed { get; set; }
		public float EngineDecay { get; set; }
		public float BreakFriction { get; set; }
		public float TurnSpeed { get; set; }
		public float AngularFriction { get; set; }

		public int WallDamageMin { get; set; }
		public int WallDamageMax { get; set; }

		public int SpikeDamage { get; set; }
		public float SpikeImmuneDuration { get; set; }

		public int LavaDamage { get; set; }
		public float LavaImmuneDuration { get; set; }

		private readonly Dictionary<ItemType, int> inventory = new Dictionary<ItemType, int>();

		private PlayerData(string name, int hp)
		{
			Name = name;
			MaxHp = hp;
			Hp = hp;
		}

		public static PlayerData Create(string name)
		{
			var p = new PlayerData(name, 100);

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

			return p;
		}

		public void Damage(int value, DamageType damageType)
		{
			Hp -= value;
			Hp = Mathf.Max(Hp, 0);
			OnDamage?.Invoke(this, value, damageType);
		}

		public void Heal(int value)
		{
			Hp += value;
			Hp = Mathf.Min(Hp, MaxHp);
			OnHeal?.Invoke(this, value);
		}

		public void AddItem(ItemType item)
		{
			inventory.TryGetValue(item, out int count);
			inventory[item] = count + 1;
			OnCollect?.Invoke(this, item);
		}

		public bool UseItem(ItemType item)
		{
			if (!inventory.TryGetValue(item, out int count) || count == 0)
			{
				return false;
			}

			inventory[item] = count - 1;
			OnUse?.Invoke(this, item);
			return true;
		}

		public int GetItemCount(ItemType item)
		{
			inventory.TryGetValue(item, out int count);
			return count;
		}

		public void NotifyCombo(EntityData entity, int combo)
		{
			OnCombo?.Invoke(this, entity, combo);
		}

		public void NotifyComboStop()
		{
			OnComboStop?.Invoke(this);
		}

		//private static PlayerData Create(string name)
		//{
		//	if (store.ContainsKey(name))
		//	{
		//		throw new Exception("PlayerData with name '" + name + "' already exists.");
		//	}

		//	var playerData = new PlayerData(name);
		//	store[name] = playerData;
		//	return playerData;
		//}

		//private static readonly Dictionary<string, PlayerData> store = new Dictionary<string, PlayerData>();

		//public static void Init()
		//{
		//	PlayerData p;

		//	p = Create("normal");
		//	p.Hp = 100;
		//	p.Friction = 0.92f;
		//	p.MaxSpeed = 125.0f;
		//	p.FrontGearForce = 650.0f;
		//	p.FrontGearSpeed = 1.8f;
		//	p.RearGearForce = 300.0f;
		//	p.RearGearSpeed = 1.5f;
		//	p.EngineDecay = 0.95f;
		//	p.BreakFriction = 0.98f;
		//	p.TurnSpeed = 5.0f;
		//	p.AngularFriction = 0.0f;

		//	p.WallDamageMin = 1;
		//	p.WallDamageMax = 10;

		//	p.SpikeDamage = 5;
		//	p.SpikeImmuneDuration = 0.5f;

		//	p.LavaDamage = 3;
		//	p.LavaImmuneDuration = 0.25f;

		//	p.PixelMask = Asset.GetPixelMask("circle_small");
		//	p.Anim = new AnimatorData("gfx/game/player", 16, 16);
		//	p.Anim.Add("idle", 0);
		//	p.Anim.Add("hurt", 7);
		//	p.Anim.Add("blood_front_1", 8);
		//	p.Anim.Add("blood_front_2", 9);
		//	p.Anim.Add("blood_front_3", 10);
		//	p.Anim.Add("blood_front_4", 11);
		//	p.Anim.Add("blood_back_1", 16);
		//	p.Anim.Add("blood_back_2", 17);
		//	p.Anim.Add("blood_back_3", 18);
		//	p.Anim.Add("blood_back_4", 19);
		//}

		//public static PlayerData Get(string name)
		//{
		//	if (!store.TryGetValue(name, out PlayerData playerData))
		//	{
		//		throw new Exception("PlayerData with name '" + name + "' not found.");
		//	}
		//	return playerData;
		//}
	}
}
