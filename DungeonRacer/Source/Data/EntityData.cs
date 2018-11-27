using System;
using System.Collections.Generic;
using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DungeonRacer
{
	enum ItemType
	{
		Coin,
		KeyNone,
		KeyA,
		KeyB,
		KeyC,
	}

	class EntityData
	{
		public string Name { get; }
		public Type Class { get; }
		public int Type { get; }
		public Vector2 TileOffset { get; private set; }
		public Rectangle Hitbox { get; private set; }
		public PixelMask PixelMask { get; private set; }

		public int Layer { get; private set; } = Global.LayerMain;
		public AnimatorData Anim { get; private set; }
		public Vector2 SpriteOrigin { get; private set; }
		public bool SpriteFlipX { get; private set; }
		public bool SpriteFlipY { get; private set; }

		public bool Groupable { get; private set; }
		public Vector2 LootSpawnOffset { get; private set; }

		public int Hp { get; private set; }
		public bool Pushable { get; private set; }

		public int DamageOnHit { get; private set; }
		public float DamageOnHitCooldown { get; private set; }
		public SoundEffect DamageOnHitSfx { get; private set; }

		public ItemType ItemType { get; private set; }

		public Action<PlayerData> OnCollect { get; private set; }
		public Sfx CollectSfx { get; private set; }

		public EntityData Loot { get; private set; }

		private EntityData(string name, int type, Type clazz)
		{
			Name = name;
			Type = type;
			Class = clazz;
		}

		private static EntityData Create(string name, int type = Global.TypeSolid)
		{
			return Create(name, type, typeof(GameEntity));
		}

		private static EntityData Create(string name, Type clazz)
		{
			return Create(name, Global.TypeSolid, clazz);
		}

		private static EntityData Create(string name, int type, Type clazz)
		{
			if (store.ContainsKey(name))
			{
				throw new Exception("EntityData with name '" + name + "' already exists.");
			}

			var entityData = new EntityData(name, type, clazz);
			store[name] = entityData;
			return entityData;
		}

		private EntityData SetHitbox(int width, int height, int originX = 0, int originY = 0)
		{		
			Hitbox = new Rectangle(originX, originY, width, height);
			return this;
		}

		private EntityData SetPixelMask(string name)
		{
			PixelMask = Asset.GetPixelMask(name);
			return this;
		}

		private EntityData CreateAnimator(string name)
		{
			Anim = new AnimatorData(name);
			return this;
		}

		private EntityData AddAnim(string name, int frame)
		{
			return AddAnim(name, AnimatorMode.Loop, 0.0f, frame);
		}

		private EntityData AddAnim(string name, AnimatorMode mode, float interval, params int[] frames)
		{
			if (Anim == null)
			{
				Anim = new AnimatorData("entities_16_16");
			}
			Anim.Add(name, mode, interval, frames);
			return this;
		}

		private EntityData SetLayer(int value)
		{
			Layer = value;
			return this;
		}

		private static readonly Dictionary<string, EntityData> store = new Dictionary<string, EntityData>();

		public static void Init()
		{
			Asset.AddTileset("entities_16_16", "gfx/game/entities", 16, 16);
			Asset.AddTileset("entities_16_32", "gfx/game/entities", 16, 32);
			Asset.AddTileset("entities_32_16", "gfx/game/entities", 32, 16);

			EntityData e;

			e = CreateItem("coin");
			e.OnCollect = (player) => player.AddItem(ItemType.Coin);
			e.AddAnim("idle", AnimatorMode.Loop, 0.03f, 64, 65, 66, 67).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/item_coin");

			e = CreateItem("key_a");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyA);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 68, 69, 70, 71).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/item_key");

			e = CreateItem("key_b");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyB);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 72, 73, 74, 75).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/item_key");

			e = CreateItem("key_c");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyC);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 76, 77, 78, 79).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/item_key");

			e = CreateItem("potion_hp");
			e.OnCollect = (player) => player.Heal(40);
			e.AddAnim("idle", 84).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/item_hp_potion");

			//e = CreateItem("potion_mp");
			//e.OnCollect = (player) => player.GainMp(1);
			//e.AddAnim("idle", 81).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			//e.CollectSfx = new Sfx("sfx/key"); // TODO sfx

			e = Create("block").SetPixelMask("circle_big"); //.SetHitbox(Global.TileSize, Global.TileSize);
			e.Hp = 2;
			e.AddAnim("idle", 0);

			e = Create("crate").SetHitbox(Global.TileSize, Global.TileSize);
			//e.Pushable = true;
			e.Hp = 1;
			e.Loot = Get("coin");
			e.LootSpawnOffset = new Vector2(8.0f, 6.0f);
			e.AddAnim("idle", 16);
			e.AddAnim("die", AnimatorMode.OneShot, 0.1f, 34, 35, 36);

			e = Create("pillar").SetPixelMask("circle_big");
			e.CreateAnimator("entities_16_32").AddAnim("idle", 1);
			e.SpriteOrigin = new Vector2(0.0f, Global.TileSize);

			e = Create("goblin", Global.TypeEnemy, typeof(Goblin));
			e.Hp = 1;
			e.Groupable = true;
			e.LootSpawnOffset = new Vector2(0.0f, -4.0f);
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SetHitbox(8, 8, 4, 8);

			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.AddAnim("walk_down", AnimatorMode.Loop, 0.25f, 192, 193);
			e.AddAnim("walk_up", AnimatorMode.Loop, 0.25f, 194, 195);
			e.AddAnim("walk_horiz", AnimatorMode.Loop, 0.25f, 196, 197);
			e.AddAnim("hurt", 210);
			e.AddAnim("dead_bouncing", AnimatorMode.Loop, 0.15f, 210, 211, 212, 213);
			e.AddAnim("dead", 199);

			e = Create("minotaur", Global.TypeEnemy, typeof(Minotaur));
			e.Hp = 3;
			e.Groupable = true;
			e.Loot = Get("key_a");
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SetHitbox(8, 8, 4, 8);
			e.LootSpawnOffset = new Vector2(0.0f, -4.0f);

			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.AddAnim("walk", AnimatorMode.Loop, 0.25f, 226, 227);
			e.AddAnim("shoot_warning", 228);
			e.AddAnim("shoot", AnimatorMode.OneShot, 0.4f, 229);
			e.AddAnim("hurt", 231);
			e.AddAnim("dead_bouncing", AnimatorMode.Loop, 0.15f, 231, 232, 233, 234);
			e.AddAnim("dead", 230);

			e = Create("ogre", Global.TypeEnemy, typeof(Ogre));
			e.Hp = 10;
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SetHitbox(8, 8, 4, 8);

			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.AddAnim("walk", AnimatorMode.Loop, 0.25f, 226, 227);
			e.AddAnim("hurt", 225);
			e.AddAnim("dead_bouncing", 225);
			e.AddAnim("dead", 230);

			e = Create("ghost", Global.TypeEnemy, typeof(Ghost));
			e.Layer = Global.LayerFront;
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SetHitbox(8, 8, 4, 8);
			e.DamageOnHit = 10;
			e.DamageOnHitCooldown = 1.0f;
			e.DamageOnHitSfx = Asset.LoadSoundEffect("sfx/ghost_attack");
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.AddAnim("idle", AnimatorMode.Loop, 0.25f, 208, 209);
			e.AddAnim("die", AnimatorMode.OneShot, 0.1f, 34, 35, 36);

			CreateDoor("door_normal", typeof(Door), 64, 80);
			CreateDoor("door_a", typeof(LockedDoor), 66, 81, ItemType.KeyA);
			CreateDoor("door_b", typeof(LockedDoor), 68, 82, ItemType.KeyB);
			CreateDoor("door_c", typeof(LockedDoor), 70, 83, ItemType.KeyC);
		}

		private static void CreateDoor(string name, Type type, int idVert, int idHoriz, ItemType keyType = ItemType.KeyA)
		{
			var e = Create(name + "_left", type).SetLayer(Global.LayerBack).SetHitbox(Global.TileSize, Global.TileSize * 3);
			e.TileOffset = new Vector2(0, -Global.TileSize);
			e.ItemType = keyType;
			e.CreateAnimator("entities_16_32").AddAnim("idle", idVert);
			e.AddAnim("open", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.AddAnim("close", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.SpriteOrigin = new Vector2(0, -Global.TileSize / 2);

			e = Create(name + "_right", type).SetLayer(Global.LayerBack).SetHitbox(Global.TileSize, Global.TileSize * 3);
			e.TileOffset = new Vector2(0, -Global.TileSize);
			e.ItemType = keyType;
			e.CreateAnimator("entities_16_32").AddAnim("idle", idVert);
			e.AddAnim("open", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.AddAnim("close", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.SpriteOrigin = new Vector2(0, -Global.TileSize / 2);
			e.SpriteFlipX = true;

			e = Create(name + "_up", type).SetLayer(Global.LayerBack).SetHitbox(Global.TileSize * 3, Global.TileSize);
			e.TileOffset = new Vector2(-Global.TileSize, 0);
			e.ItemType = keyType;
			e.CreateAnimator("entities_32_16").AddAnim("idle", idHoriz);
			e.AddAnim("open", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.AddAnim("close", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.SpriteOrigin = new Vector2(-Global.TileSize / 2, 0);

			e = Create(name + "_down", type).SetLayer(Global.LayerBack).SetHitbox(Global.TileSize * 3, Global.TileSize);
			e.TileOffset = new Vector2(-Global.TileSize, 0);
			e.ItemType = keyType;
			e.CreateAnimator("entities_32_16").AddAnim("idle", idHoriz);
			e.AddAnim("open", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.AddAnim("close", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.SpriteOrigin = new Vector2(-Global.TileSize / 2, 0);
			e.SpriteFlipY = true;
		}

		private static EntityData CreateItem(string name)
		{
			var e = Create(name, Global.TypeCollectible, typeof(Collectible));
			e.SetHitbox(12, 12, 6, 12);
			e.Layer = Global.LayerFront;
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			return e;
		}

		public static EntityData Get(string name)
		{
			if (store.TryGetValue(name, out EntityData entityData))
			{
				return entityData;
			}
			return null;
		}
	}
}
