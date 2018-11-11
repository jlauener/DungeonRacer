using System;
using MonoPunk;

namespace DungeonRacer
{
	class RoomEntity : Entity
	{
		private readonly Room room;
		private readonly EntityData data;
		private readonly Animator sprite;

		public RoomEntity(Room room, EntityData data, float x, float y) : base(room.X + x * Global.TileSizePx, room.Y + y * Global.TileSizePx)
		{
			this.room = room;
			this.data = data;
			Type = Global.TypeEntity;
			Layer = data.Layer;
			Width = data.Hitbox.Width * Global.Scale;
			Height = data.Hitbox.Height * Global.Scale;
			OriginX = data.Hitbox.X;
			OriginY = data.Hitbox.Y;

			sprite = new Animator(data.Anim);
			sprite.Scale = Global.Scale;
			sprite.Origin = data.SpriteOrigin;
			Add(sprite);

			UpdateSortOrder();
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Collidable = true;
			sprite.Play("idle");
			UpdateSortOrder();
		}

		public bool HandlePlayerHit(Player player, int dx, int dy)
		{
			switch (data.Type)
			{
				case EntityType.Collectible:
					data.CollectAction?.Invoke(Scene, this, player);
					if (data.CollectSfx != null) data.CollectSfx.Play();

					Collidable = false;
					room.RemoveEntity(this);
					sprite.Play("collect", RemoveFromScene);
					return false;

				case EntityType.Door:
					if (!data.Name.Contains("locked") || player.UseKey())
					{
						Collidable = false;
						room.RemoveEntity(this);
						sprite.Play("open", RemoveFromScene);
						return false;
					}
					return true;
			}

			if (data.Pushable)
			{
				if (!CollideAt(X + dx, Y + dy, Global.TypeMap, Global.TypeEntity))
				{
					X += dx;
					Y += dy;
					UpdateSortOrder();
					return false;
				}
			}

			return data.Solid;
		}

		private void UpdateSortOrder()
		{
			sprite.SortOrder = Mathf.Floor(Bottom) * 10 + 1;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (data.DamagePerSec > 0.0f)
			{
				var info = CollideAt(X, Y, Global.TypePlayer);
				if (info.Other != null)
				{
					((Player)info.Other).Damage(data.DamagePerSec * deltaTime);
				}
			}
		}
	}
}
