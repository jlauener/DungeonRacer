using Microsoft.Xna.Framework;
using MonoPunk;
using System;

namespace DungeonRacer
{
	class Enemy : GameEntity
	{
		private bool dead;

		private Vector2 velocity;

		public Enemy(EntityData data, Dungeon dungeon, DungeonTile tile) : base(data, dungeon, tile)
		{
			var moveSpeed = 20.0f;
			if (data.Name == "goblin_h") velocity = Vector2.UnitX * moveSpeed;
			else velocity = Vector2.UnitY * moveSpeed;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!Collidable) return;

			if (dead)
			{
				MoveBy(velocity * deltaTime, Global.TypeMap);
				velocity *= 0.9f;
				if (velocity.Length() < 10.0f)
				{
					Collidable = false;
					Sprite.Play("poof", RemoveFromScene);
				}
				return;
			}

			MoveBy(velocity * deltaTime, Global.TypeMap);

			if (Data.Name == "goblin_h")
			{
				Sprite.Play("walk");
				Sprite.FlipX = velocity.X < 0.0f;
			}
			else Sprite.Play(velocity.Y < 0.0f ? "walk_up" : "walk_down");
		}

		protected override bool OnHit(HitInfo info)
		{
			if(dead)
			{
				Collidable = false;
				Sprite.Play("poof", RemoveFromScene);
				return true;
			}

			velocity = -velocity;
			return true;
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (dead) return false;

			player.Damage(1);
			Sprite.Play("die");
			velocity = player.Velocity;
			dead = true;
			return true;
		}
	}
}
