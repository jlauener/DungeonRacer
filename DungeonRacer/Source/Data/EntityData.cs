using System;
using System.Collections.Generic;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	enum EntityType
	{
		Default,
		Collectible,
		Door
	}

	class EntityData
	{
		public string Name { get; }
		public EntityType Type { get; }
		public Rectangle Hitbox { get; private set; }

		public int Layer { get; private set; } = Global.LayerMain;
		public AnimatorData Anim { get; private set; }
		public Vector2 SpriteOrigin { get; private set; }

		public bool Solid { get; private set; } = true;
		public bool Pushable { get; private set; }
		public int DamageOnHit { get; private set; }
		public int DamagePerSec { get; private set; }

		private EntityData(string name, EntityType type)
		{
			Name = name;
			Type = type;
			switch (type)
			{
				case EntityType.Default:
					SetHitbox(Global.TileSize, Global.TileSize);
					break;
				case EntityType.Collectible:
					SetHitbox(8, 12, -8, -4);
					break;
			}
		}

		private static EntityData Create(string name, EntityType type = EntityType.Default)
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
			Asset.AddTileset("entities_16_16", "gfx/entities", 16, 16);
			Asset.AddTileset("entities_16_32", "gfx/entities", 16, 32);
			Asset.AddTileset("entities_32_16", "gfx/entities", 32, 16);

			EntityData e;

			e = Create("block1").AddAnim("idle", 0);
			e = Create("block2").AddAnim("idle", 1);
			e = Create("push_block").AddAnim("idle", 2);
			e.Pushable = true;

			e = Create("pillar_top");
			e.CreateAnimator("entities_16_32").AddAnim("idle", 24);
			e.SpriteOrigin = new Vector2(0.0f, Global.TileSize);

			e = Create("pillar_bottom");
			e.CreateAnimator("entities_16_32").AddAnim("idle", 25);
			e.SpriteOrigin = new Vector2(0.0f, -Global.TileSize);

			e = Create("pillar_left");
			e.CreateAnimator("entities_32_16").AddAnim("idle", 32);
			e.SpriteOrigin = new Vector2(Global.TileSize, 0.0f);

			e = Create("pillar_right");
			e.CreateAnimator("entities_32_16").AddAnim("idle", 37);
			e.SpriteOrigin = new Vector2(-Global.TileSize, 0.0f);

			e = Create("key", EntityType.Collectible).SetLayer(Global.LayerFront).AddAnim("idle", AnimatorMode.Loop, 0.2f, 20, 21, 22, 23);

			e = Create("coin", EntityType.Collectible).SetLayer(Global.LayerFront);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 24, 25, 26, 27).AddAnim("hit", AnimatorMode.Loop, 0.05f, 24, 25, 26, 27).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);

			e = Create("spike");
			e.Solid = false;
			e.DamagePerSec = 50;
			e.SetLayer(Global.LayerBack).AddAnim("idle", 10);

			e = Create("door_left", EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(16, 32).CreateAnimator("entities_16_32").AddAnim("idle", 20).AddAnim("open", AnimatorMode.OneShot, 0.2f, 21);
			e = Create("door_right", EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(16, 32).CreateAnimator("entities_16_32").AddAnim("idle", 22).AddAnim("open", AnimatorMode.OneShot, 0.2f, 23);
			e = Create("door_up", EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(32, 16).CreateAnimator("entities_32_16").AddAnim("idle", 30).AddAnim("open", AnimatorMode.OneShot, 0.2f, 31);
			e = Create("door_down", EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(32, 16).CreateAnimator("entities_32_16").AddAnim("idle", 35).AddAnim("open", AnimatorMode.OneShot, 0.2f, 36);
		}

		public static EntityData Get(string name)
		{
			EntityData entityData;
			if (!store.TryGetValue(name, out entityData))
			{
				throw new Exception("EntityData with name '" + name + "' not found.");
			}
			return entityData;
		}
	}
}
