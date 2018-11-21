using Microsoft.Xna.Framework;
using MonoPunk;
using System;

namespace DungeonRacer
{
	class Enemy : GameEntity
	{
		public bool Alive { get { return state == State.Alive || state == State.Enter; } }

		private enum State
		{
			Enter,
			Alive,
			DeadBouncing,
			Dead
		}
		private State state = State.Enter;

		private Vector2 velocity;

		public Enemy(EntityArguments args) : base(args)
		{
			velocity = DirectionUtils.GetNormal(Direction) * 20.0f;

			Sprite.Play("enter", () =>
			{
				Collidable = true;
				state = State.Alive;
				UpdateSprite();
			});
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			if (!Collidable) return;

			switch (state)
			{
				case State.Alive:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					break;
				case State.DeadBouncing:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					velocity *= 0.95f;
					if (velocity.Length() < 10.0f)
					{
						state = State.Dead;
						Sprite.Play("dead");
					}
					break;
			}
		}

		protected override bool OnHit(HitInfo info)
		{
			if (state == State.DeadBouncing)
			{
				if (info.IsHorizontalMovement) velocity.X *= -1; else velocity.Y *= -1;
				//Asset.LoadSoundEffect("sfx/hit").Play();
				return true;
			}

			Direction = DirectionUtils.GetOpposite(Direction);
			velocity *= -1;
			UpdateSprite();
			return true;
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (state == State.Alive)
			{
				Sprite.Play("dead_bouncing");
				velocity = player.Velocity * 2.0f;
				state = State.DeadBouncing;
				//Add(new Blinker(0.1f, Sprite));

				//Scene.GetEntity<Shaker>().Shake(dx * 4.0f, dy * 4.0f);
				Scene.GetEntity<DungeonMap>().DrawGroundEffect(X, Y - 6, "blood" + Rand.NextInt(3));

				Asset.LoadSoundEffect("sfx/enemy_hurt").Play();
				return true; // FIXME
			}


			if (state == State.Dead)
			{
				// TODO blood marks
				//Asset.LoadSoundEffect("sfx/enemy_hurt").Play();
			}

			return false;
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
