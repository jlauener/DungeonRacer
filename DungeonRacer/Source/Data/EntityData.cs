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
		public PixelMask PixelMask { get; private set; }

		public int Layer { get; private set; } = Global.LayerMain;
		public AnimatorData Anim { get; private set; }
		public Vector2 SpriteOrigin { get; private set; }

		public bool Solid { get; private set; } = true;
		public bool Pushable { get; private set; }
		public int DamageOnHit { get; private set; }
		public int DamagePerSec { get; private set; }

		public Action<Scene, RoomEntity, Player> CollectAction { get; private set; }

		public Sfx CollectSfx { get; private set; }

		private EntityData(string name, EntityType type)
		{
			Name = name;
			Type = type;
			switch (type)
			{
				case EntityType.Default:
					SetHitbox(Global.TileSize, Global.TileSize);
					PixelMask = new PixelMask("gfx/game/entity_mask"); // TODO cache the mask ?
					break;
				case EntityType.Collectible:
					SetHitbox(8, 12, -4, -4);
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
			Asset.AddTileset("entities_16_16", "gfx/game/entities", 16, 16);
			Asset.AddTileset("entities_16_32", "gfx/game/entities", 16, 32);
			Asset.AddTileset("entities_32_16", "gfx/game/entities", 32, 16);

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

			e = Create("key", EntityType.Collectible).SetLayer(Global.LayerFront);
			e.CollectAction = (gameScene, room, player) => player.AddKey();
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 20, 21, 22, 23).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			e.CollectSfx = new Sfx("sfx/key");

			e = Create("coin", EntityType.Collectible).SetLayer(Global.LayerFront);
			e.CollectAction = (gameScene, room, player) => player.AddMoney(5);
			e.AddAnim("idle", AnimatorMode.Loop, 0.2f, 24, 25, 26, 27).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			e.CollectSfx = new Sfx("sfx/coin");

			e = Create("extra_time", EntityType.Collectible).SetLayer(Global.LayerFront);
			e.CollectAction = (scene, room, player) => ((GameScene) scene).AddTime(20.0f);
			e.AddAnim("idle", 30).AddAnim("collect", AnimatorMode.OneShot, 0.08f, 91, 90);
			e.CollectSfx = new Sfx("sfx/coin");

			e = Create("spike");
			e.Solid = false;
			e.DamagePerSec = 50;
			e.SetLayer(Global.LayerBack).AddAnim("idle", 10);

			//CreateDoor("exit", 20);
			CreateDoor("locked", 20);
		}

		private static void CreateDoor(string name, int id)
		{
			EntityData e;

			e = Create("door_left_" + name, EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.CreateAnimator("entities_16_32").AddAnim("idle", id).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 1);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);

			e = Create("door_right_" + name, EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(16, 32, 0, Global.TileSize / 2);
			e.CreateAnimator("entities_16_32").AddAnim("idle", id + 3).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 2);
			e.SpriteOrigin = new Vector2(0, Global.TileSize / 2);

			e = Create("door_up_" + name, EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.CreateAnimator("entities_32_16").AddAnim("idle", id + 10).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 11);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);

			e = Create("door_down_" + name, EntityType.Door).SetLayer(Global.LayerBack).SetHitbox(32, 16, Global.TileSize / 2, 0);
			e.CreateAnimator("entities_32_16").AddAnim("idle", id + 15).AddAnim("open", AnimatorMode.OneShot, 0.1f, id + 16);
			e.SpriteOrigin = new Vector2(Global.TileSize / 2, 0);
		}

		public static EntityData Get(string name)
		{
			EntityData entityData;
			if (!store.TryGetValue(name, out entityData))
			{
				return null;
			}
			return entityData;
		}
	}
}
