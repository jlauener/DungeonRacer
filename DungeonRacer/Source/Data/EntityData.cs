using System;
using System.Collections.Generic;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	enum ItemType
	{
		Coin,
		Key
	}

	class EntityData
	{
		public string Name { get; }
		public Type Type { get; }
		public Rectangle Hitbox { get; private set; }
		public PixelMask PixelMask { get; private set; }

		public int Layer { get; private set; } = Global.LayerMain;
		public AnimatorData Anim { get; private set; }
		public Vector2 SpriteOrigin { get; private set; }

		public bool Solid { get; private set; } = true;
		public bool Pushable { get; private set; }
		public int DamageOnHit { get; private set; }
		public int DamagePerSec { get; private set; }

		public ItemType ItemType { get; private set; }

		public Action<Player> OnCollect { get; private set; }
		public Sfx CollectSfx { get; private set; }

		private EntityData(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		private static EntityData Create(string name)
		{
			return Create(name, typeof(GameEntity));
		}

		private static EntityData Create(string name, Type type)
		{
			if (store.ContainsKey(name))
			{
				throw new Exception("EntityData with name '" + name + "' already exists.");
			}

			var entityData = new EntityData(name, type);
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

			e = Create("block").SetPixelMask("edged_square");
			e.AddAnim("idle", 0);

			e = Create("push_block").SetHitbox(Global.TileSize, Global.TileSize);			
			e.Pushable = true;
			e.AddAnim("idle", 1);

			e = Create("pillar").SetPixelMask("edged_square");
			e.CreateAnimator("entities_16_32").AddAnim("idle", 24);
			e.SpriteOrigin = new Vector2(0.0f, Global.TileSize);

			e = CreateItem("coin");
			e.OnCollect = (player) => player.AddItem(ItemType.Coin);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 20, 21, 22, 23).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			e.CollectSfx = new Sfx("sfx/coin");

			e = CreateItem("key");
			e.OnCollect = (player) => player.AddItem(ItemType.Key);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 24, 25, 26, 27).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			e.CollectSfx = new Sfx("sfx/key");

			e = CreateItem("potion_hp");
			e.OnCollect = (player) => player.Heal(20);
			e.AddAnim("idle", 30).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			// TODO e.CollectSfx = new Sfx("sfx/key");

			e = CreateItem("potion_mp");
			e.OnCollect = (player) => player.GainMp(20);
			e.AddAnim("idle", 31).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			// TODO e.CollectSfx = new Sfx("sfx/key");

			e = Create("spike").SetPixelMask("edged_square");
			e.Solid = false;
			e.DamagePerSec = 50;
			e.SetLayer(Global.LayerBack).AddAnim("idle", 10);

			CreateDoor("normal", 20);
		}

		private static void CreateDoor(string name, int id)
		{
			var e = Create("door_left_" + name, typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.CreateAnimator("entities_16_32").AddAnim("idle", id + 3).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 2);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);

			e = Create("door_right_" + name, typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.CreateAnimator("entities_16_32").AddAnim("idle", id + 0).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 1);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);

			e = Create("door_up_" + name, typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.CreateAnimator("entities_32_16").AddAnim("idle", id + 10).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 11);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);

			e = Create("door_down_" + name, typeof(Door)).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.CreateAnimator("entities_32_16").AddAnim("idle", id + 15).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 16);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);
		}

		private static EntityData CreateItem(string name)
		{
			var e = Create(name, typeof(Collectible));
			e.SetHitbox(8, 12, -4, -4);
			e.Layer = Global.LayerFront;
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
