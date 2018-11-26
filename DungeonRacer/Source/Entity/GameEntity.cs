using System;
using Microsoft.Xna.Framework;
using MonoPunk;

namespace DungeonRacer
{
	class EntityArguments
	{
		public EntityData Data { get; }
		public int TileX { get; }
		public int TileY { get; }
		public Vector2 Position { get; }
		public Direction Direction { get; }
		public bool Enter { get; }

		public EntityArguments(EntityData data, Vector2 position, Direction direction = Direction.Down)
		{
			Data = data;
			Position = position;
			Direction = direction;
		}

		public EntityArguments(EntityData data, int tileX, int tileY, Direction direction = Direction.Down)
		{
			Data = data;
			TileX = tileX;
			TileY = tileY;
			Direction = direction;
		}
	}

	[Flags]
	enum HitFlags
	{
		Nothing = 0x00,
		Stop = 0x01,
		Blood = 0x02
	}

	class GameEntity : Entity
	{
		protected EntityArguments Args { get; }
		public EntityData Data { get { return Args.Data; } }
		//protected Room Room { get; private set; }
		protected Direction Direction { get; set; }
		protected int Hp;

		protected Animator Sprite { get; }

		private float damageOnHitCooldown;

		public GameEntity(EntityArguments args)
		{
			Args = args;

			if (args.Position != Vector2.Zero)
			{
				Position = args.Position;
			}
			else
			{
				X = Data.TileOffset.X + args.TileX * Global.TileSize;
				Y = Data.TileOffset.Y + args.TileY * Global.TileSize;
			}

			Type = Data.Type;
			Direction = args.Direction;
			Hp = args.Data.Hp;
			Width = Data.Hitbox.Width;
			Height = Data.Hitbox.Height;
			OriginX = Data.Hitbox.X;
			OriginY = Data.Hitbox.Y;
			Collider = Data.PixelMask;

			Layer = Data.Layer;
			Sprite = new Animator(Data.Anim);
			Sprite.Origin = Data.SpriteOrigin;
			Sprite.FlipX = Data.SpriteFlipX;
			Sprite.FlipY = Data.SpriteFlipY;
			Add(Sprite);

			UpdateSortOrder();
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			if (Sprite.Contains("idle")) Sprite.Play("idle");
			UpdateSortOrder();
		}

		public virtual HitFlags HandlePlayerHit(Player player, int dx, int dy)
		{
			if (Data.Pushable)
			{
				if (!CollideAt(X + dx, Y + dy, Global.TypeMap, Global.TypeSolid))
				{
					X += dx;
					Y += dy;
					UpdateSortOrder();
				}
				else
				{
					Collidable = false;
					Sprite.Play("die", RemoveFromScene);
				}
				return HitFlags.Nothing;
			}

			if (Data.DamageOnHit > 0.0f && damageOnHitCooldown <= 0.0f)
			{
				player.Damage(Data.DamageOnHit, DamageType.Entity);
				damageOnHitCooldown = Data.DamageOnHitCooldown;
				if (Data.DamageOnHitSfx != null) Data.DamageOnHitSfx.Play();
			}

			if(Hp > 0)
			{
				if (player.Speed < PlayerData.MinimumHitSpeed) return HitFlags.Stop;

				if (--Hp == 0)
				{
					Collidable = false;
					Sprite.Play("die", RemoveFromScene);

					if (Data.Loot != null)
					{
						Scene.Add(Create(Data.Loot, Position + new Vector2(8.0f, 6.0f)));
					}

					GameScene.Shaker.Shake(Vector2.Normalize(player.Velocity) * 4.0f);
					Asset.LoadSoundEffect("sfx/enemy_hurt").Play();

					return HitFlags.Nothing;
				}
			}

			return Type == Global.TypeSolid ? HitFlags.Stop : HitFlags.Nothing;
		}

		protected void UpdateSortOrder(int offset = 1)
		{
			Sprite.SortOrder = Mathf.Floor(Bottom) * 10 + offset;
		}

		protected override void OnUpdate(float deltaTime)
		{
			OnUpdateActive(deltaTime);
			base.OnUpdate(deltaTime);
			if (damageOnHitCooldown > 0.0f) damageOnHitCooldown -= deltaTime;
		}

		protected virtual void OnUpdateActive(float deltaTime)
		{
		}

		public static GameEntity Create(EntityArguments args)
		{
			return (GameEntity)Activator.CreateInstance(args.Data.Class, args);
		}

		public static GameEntity Create(EntityData data, int tileX, int tileY, Direction direction = Direction.Down)
		{
			return Create(new EntityArguments(data, tileX, tileY, direction));
		}

		public static GameEntity Create(EntityData data, Vector2 position, Direction direction = Direction.Down)
		{
			return Create(new EntityArguments(data, position, direction));
		}
	}
}
