using System;
using System.Collections.Generic;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	enum ItemType
	{
		Coin,
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

		//public bool Solid { get; private set; } = true;
		public bool Pushable { get; private set; }
		public int DamageOnTouch { get; private set; }

		public ItemType ItemType { get; private set; }

		public Action<Player> OnCollect { get; private set; }
		public Sfx CollectSfx { get; private set; }

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

			Asset.AddPixelMask("edged_square", "gfx/mask/entity_mask");

			EntityData e;

			e = Create("block").SetHitbox(Global.TileSize, Global.TileSize);
			e.AddAnim("idle", 0);

			e = Create("push_block").SetHitbox(Global.TileSize, Global.TileSize);			
			e.Pushable = true;
			e.AddAnim("idle", 16);

			e = Create("pillar").SetPixelMask("edged_square");
			e.CreateAnimator("entities_16_32").AddAnim("idle", 1);
			e.SpriteOrigin = new Vector2(0.0f, Global.TileSize);

			e = CreateItem("coin");
			e.OnCollect = (player) => player.AddItem(ItemType.Coin);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 64, 65, 66, 67).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/coin");

			e = CreateItem("key_a");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyA);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 68, 69, 70, 71).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/key");

			e = CreateItem("key_b");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyB);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 72, 73, 74, 75).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/key");

			e = CreateItem("key_c");
			e.OnCollect = (player) => player.AddItem(ItemType.KeyC);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 76, 77, 78, 79).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/key");

			e = CreateItem("potion_hp");
			e.OnCollect = (player) => player.Heal(40);
			e.AddAnim("idle", 84).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/key"); // TODO sfx

			e = CreateItem("potion_mp");
			e.OnCollect = (player) => player.GainMp(1);
			e.AddAnim("idle", 81).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 32, 33);
			e.CollectSfx = new Sfx("sfx/key"); // TODO sfx

			e = Create("goblin", Global.TypeEnemy, typeof(Enemy));
			e.TileOffset = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.SetHitbox(8, 8, 4, 8);

			e.SpriteOrigin = new Vector2(Global.TileSize / 2, Global.TileSize - 2);
			e.AddAnim("walk_down", AnimatorMode.Loop, 0.25f, 192, 193);
			e.AddAnim("walk_up", AnimatorMode.Loop, 0.25f, 194, 195);
			e.AddAnim("walk_horiz", AnimatorMode.Loop, 0.25f, 196, 197);
			e.AddAnim("die", 198);
			e.AddAnim("poof", AnimatorMode.OneShot, 0.1f, 34, 35, 36);

			CreateDoor("door_a", ItemType.KeyA, 64, 80);
			CreateDoor("door_b", ItemType.KeyB, 66, 81);
			CreateDoor("door_c", ItemType.KeyC, 68, 82);
		}

		private static void CreateDoor(string name, ItemType keyType, int idVert, int idHoriz)
		{
			var e = Create(name + "_left", typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.ItemType = keyType;
			e.CreateAnimator("entities_16_32").AddAnim("idle", idVert).AddAnim("open", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);

			e = Create(name + "_right", typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.ItemType = keyType;
			e.CreateAnimator("entities_16_32").AddAnim("idle", idVert).AddAnim("open", AnimatorMode.OneShot, 0.1f, idVert + 1);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);
			e.SpriteFlipX = true;

			e = Create(name + "_up", typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.ItemType = keyType;
			e.CreateAnimator("entities_32_16").AddAnim("idle", idHoriz).AddAnim("open", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);

			e = Create(name + "_down", typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.ItemType = keyType;
			e.CreateAnimator("entities_32_16").AddAnim("idle", idHoriz).AddAnim("open", AnimatorMode.OneShot, 0.1f, idHoriz + 8);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);
			e.SpriteFlipY = true;
		}

		private static EntityData CreateItem(string name)
		{
			var e = Create(name, Global.TypeCollectible, typeof(Collectible));
			e.SetHitbox(8, 8, 4, 8);
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
