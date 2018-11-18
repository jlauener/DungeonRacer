using System;
using MonoPunk;

namespace DungeonRacer
{
	class GameEntity : Entity
	{
		protected EntityData Data { get; }
		protected Animator Sprite { get; }

		public GameEntity(EntityData data, Dungeon dungeon, DungeonTile tile) : base(tile.X * Global.TileSize, tile.Y * Global.TileSize)
		{
			Data = data;
			Type = Global.TypeEntity;
			Layer = data.Layer;
			Width = data.Hitbox.Width;
			Height = data.Hitbox.Height;
			OriginX = data.Hitbox.X;
			OriginY = data.Hitbox.Y;
			Collider = data.PixelMask;

			Sprite = new Animator(data.Anim);
			Sprite.Origin = data.SpriteOrigin;
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
				if (!CollideAt(X + dx, Y + dy, Global.TypeMap, Global.TypeEntity))
				{
					X += dx;
					Y += dy;
					UpdateSortOrder();
					return false;
				}
			}

			return Data.Solid;
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

		public static GameEntity Create(EntityData data, Dungeon dungeon, DungeonTile tile)
		{
			return (GameEntity) Activator.CreateInstance(data.Type, data, dungeon, tile);
		}
	}
}
