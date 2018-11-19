using System;
using Microsoft.Xna.Framework;
using MonoPunk;

namespace DungeonRacer
{
	struct EntityArguments
	{
		public DungeonTile Tile { get; }
		public Vector2 Position { get; set; }
		public Direction Direction { get; }

		public EntityArguments(DungeonTile tile, Vector2 position, Direction direction)
		{
			Tile = tile;
			Position = position;
			Direction = direction;
		}
	}

	class GameEntity : Entity
	{
		protected EntityData Data { get; }
		protected Direction Direction { get; set; }
		protected Animator Sprite { get; }

		public GameEntity(EntityData data, EntityArguments args)
		{
			if (args.Tile != null)
			{
				X = data.TileOffset.X + args.Tile.X * Global.TileSize;
				Y = data.TileOffset.Y + args.Tile.Y * Global.TileSize;
			}
			else
			{
				Position = args.Position;
			}

			Data = data;
			Type = data.Type;
			Direction = args.Direction;
			Layer = data.Layer;
			Width = data.Hitbox.Width;
			Height = data.Hitbox.Height;
			OriginX = data.Hitbox.X;
			OriginY = data.Hitbox.Y;
			Collider = data.PixelMask;

			Sprite = new Animator(data.Anim);
			Sprite.Origin = data.SpriteOrigin;
			Sprite.FlipX = data.SpriteFlipX;
			Sprite.FlipY = data.SpriteFlipY;
			Add(Sprite);

			UpdateSortOrder();
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Collidable = true;
			if(Sprite.Contains("idle")) Sprite.Play("idle");
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
					return false;
				}
			}

			return Type == Global.TypeSolid;
		}

		private void UpdateSortOrder()
		{
			Sprite.SortOrder = Mathf.Floor(Bottom) * 10 + 1;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (Data.DamageOnTouch > 0.0f)
			{
				var info = CollideAt(X, Y, Global.TypePlayer);
				if (info.Other != null)
				{
					((Player)info.Other).Damage(Data.DamageOnTouch);
				}
			}
		}

		public static GameEntity Create(EntityData data, EntityArguments args)
		{
			return (GameEntity) Activator.CreateInstance(data.Class, data, args);
		}

		public static GameEntity Create(EntityData data, DungeonTile tile)
		{
			return Create(data, new EntityArguments(tile, Vector2.Zero, tile.Direction));
		}

		public static GameEntity Create(string name, Vector2 position, Direction direction = Direction.Down)
		{
			return Create(EntityData.Get(name), new EntityArguments(null, position, direction));
		}
	}
}
