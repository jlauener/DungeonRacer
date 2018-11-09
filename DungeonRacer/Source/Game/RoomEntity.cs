using System;
using MicroPunk;

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
					if (data.Name == "key")
					{
						player.AddKey();
					}
					else if (data.Name == "coin")
					{
						player.AddMoney(1);
					}
					Collidable = false;
					room.RemoveEntity(this);

					//sprite.Play("hit");
					Scene.Tween(this, new { Y = Y - Global.TileSizePx * 1.8f }, 0.35f).Ease(Ease.QuadOut).OnComplete(() =>
					{
						sprite.Play("collect", RemoveFromScene);
					});
					return false;
				case EntityType.Door:
					if (player.UseKey())
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
				if (!Collide(X + dx, Y + dy, Global.TypeMap, Global.TypeEntity))
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
				var info = Collide(X, Y, Global.TypePlayer);
				if (info.Other != null)
				{
					((Player)info.Other).Damage(data.DamagePerSec * deltaTime);
				}
			}
		}
	}
}
