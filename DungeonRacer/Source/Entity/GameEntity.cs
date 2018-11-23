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

	class GameEntity : Entity
	{
		protected EntityArguments Args { get; }
		public EntityData Data { get { return Args.Data; } }
		protected Room Room { get; private set; }
		protected Direction Direction { get; set; }

		protected Animator Sprite { get; }

		private float damageOnHitCooldown;

		public GameEntity(Room room, EntityArguments args)
		{
			Room = room;
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
			Layer = Data.Layer;
			Width = Data.Hitbox.Width;
			Height = Data.Hitbox.Height;
			OriginX = Data.Hitbox.X;
			OriginY = Data.Hitbox.Y;
			Collider = Data.PixelMask;

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

		public virtual bool HandlePlayerHit(Player player, int dx, int dy)
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
				return false;
			}

			if (Data.DamageOnHit > 0.0f && damageOnHitCooldown <= 0.0f)
			{
				player.Damage(Data.DamageOnHit, DamageType.Entity);
				damageOnHitCooldown = Data.DamageOnHitCooldown;
				if (Data.DamageOnHitSfx != null) Data.DamageOnHitSfx.Play();
			}

			return Type == Global.TypeSolid;
		}

		protected void UpdateSortOrder(int offset = 1)
		{
			Sprite.SortOrder = Mathf.Floor(Bottom) * 10 + offset;
		}

		protected override void OnUpdate(float deltaTime)
		{
			if (Room.Active)
			{
				OnUpdateActive(deltaTime);
				base.OnUpdate(deltaTime);
				if (damageOnHitCooldown > 0.0f) damageOnHitCooldown -= deltaTime;
			}
			else
			{
				damageOnHitCooldown = 0.0f;
			}
		}

		protected virtual void OnUpdateActive(float deltaTime)
		{
		}

		public static GameEntity Create(Room room, EntityArguments args)
		{
			return (GameEntity)Activator.CreateInstance(args.Data.Class, room, args);
		}

		public static GameEntity Create(Room room, EntityData data, int tileX, int tileY, Direction direction = Direction.Down)
		{
			return Create(room, new EntityArguments(data, tileX, tileY, direction));
		}

		public static GameEntity Create(Room room, string name, Vector2 position, Direction direction = Direction.Down)
		{
			return Create(room, new EntityArguments(EntityData.Get(name), position, direction));
		}
	}
}
