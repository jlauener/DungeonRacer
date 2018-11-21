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
		private State state = State.Alive;

		private Vector2 velocity;

		private bool hasBlood = true;

		public Enemy(Room room, EntityArguments args) : base(room, args)
		{
			velocity = DirectionUtils.GetNormal(Direction) * 20.0f;
			Collidable = false;

			//Sprite.Play("enter", () =>
			//{
			//	Collidable = true;
			//	state = State.Alive;
			//	UpdateSprite();
			//});
			UpdateSprite();
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);
			if (!Room.Active) return;

			Collidable = true;
			switch (state)
			{
				case State.Alive:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					break;
				case State.DeadBouncing:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					velocity *= 0.92f;
					if (velocity.Length() < 8.0f)
					{
						velocity = Vector2.Zero;
						state = State.Dead;
						Sprite.Play("dead");
						Sprite.SortOrder = Mathf.Floor(Bottom) * 10 - 10000;
					}
					break;
				case State.Dead:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					velocity *= 0.8f;
					break;
			}
		}

		protected override bool OnHit(HitInfo info)
		{
			if (state != State.Alive)
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
				DungeonMap.Instance.DrawGroundEffect(X + dx * 6, Y + dy * 6, "blood" + Rand.NextInt(3), 1.0f, Rand.NextFloat(Mathf.Pi2));

				Asset.LoadSoundEffect("sfx/enemy_hurt").Play();
				return true; // FIXME
			}


			if (state == State.Dead)
			{
				if (hasBlood)
				{
					player.AddTireBlood(0.25f);
					DungeonMap.Instance.DrawGroundEffect(X + dx * 4, Y + dy * 4, "blood_small" + Rand.NextInt(2), 1.0f, Rand.NextFloat(Mathf.Pi2));
					hasBlood = false;

					// TODO play sfx
				}
	
				player.AddTireBlood(0.02f);
				velocity = player.Velocity * 0.22f;
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
