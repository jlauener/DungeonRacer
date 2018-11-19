using Microsoft.Xna.Framework;
using MonoPunk;
using System;

namespace DungeonRacer
{
	class Enemy : GameEntity
	{
		private bool dead;

		private Vector2 velocity;

		public Enemy(EntityData data, EntityArguments args) : base(data, args)
		{
			velocity = DirectionUtils.GetNormal(Direction) * 20.0f;
			UpdateSprite();
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!Collidable) return;

			MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);

			if (dead)
			{
				velocity *= 0.95f;
				if (velocity.Length() < 5.0f)
				{
					Poof();
				}
				return;
			}
		}

		protected override bool OnHit(HitInfo info)
		{
			if(dead)
			{
				//Poof();
				if (info.IsHorizontalMovement) velocity.X *= -1;
				else velocity.Y *= -1;
				return true;
			}

			Direction = DirectionUtils.GetOpposite(Direction);
			velocity *= -1;
			UpdateSprite();
			return true;
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (dead) return false;

			Sprite.Play("die");
			velocity = player.Velocity * 2.0f;
			dead = true;
			//Add(new Blinker(0.1f, Sprite));

			//Scene.GetEntity<Shaker>().Shake(dx * 4.0f, dy * 4.0f);
			Scene.GetEntity<Dungeon>().DrawGroundEffect(X, Y - 6, "blood" + Rand.NextInt(3));

			return true;
		}

		private void Poof()
		{
			Collidable = false;
			Sprite.Play("poof", RemoveFromScene);

			Scene.Add(Create("coin", Position));
		}

		private void UpdateSprite()
		{
			switch (Direction)
			{
				case Direction.Left:
					Sprite.Play("walk_horiz");
					Sprite.FlipX = true;
					break;
				case Direction.Right:
					Sprite.Play("walk_horiz");
					Sprite.FlipX = false;
					break;
				case Direction.Up:
					Sprite.Play("walk_up");
					break;
				case Direction.Down:
					Sprite.Play("walk_down");
					break;
			}
		}
	}
}
