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
			Bouncing,
			Dead
		}
		private State state = State.Alive;

		protected Vector2 velocity;
		protected int hp;

		private bool hasBlood = true;

		public Enemy(Room room, EntityArguments args) : base(room, args)
		{
			hp = args.Data.Hp;
			Collidable = false;
			OnStartAlive();
		}

		protected override void OnUpdateActive(float deltaTime)
		{
			Collidable = true;
			switch (state)
			{
				case State.Alive:
					OnUpdateAlive(deltaTime);
					break;
				case State.Bouncing:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					velocity *= 0.92f;
					if (velocity.Length() < 8.0f)
					{
						velocity = Vector2.Zero;
						if (hp == 0)
						{
							state = State.Dead;
							Sprite.Play("dead");
							UpdateSortOrder(-10000);
						}
						else
						{
							state = State.Alive;
							OnStartAlive();
							UpdateSortOrder();
						}
					}
					break;
				case State.Dead:
					MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
					velocity *= 0.8f;
					break;
			}
		}

		protected virtual void OnStartAlive()
		{
		}

		protected virtual void OnUpdateAlive(float deltaTime)
		{
			MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid);
			UpdateSortOrder();
		}

		protected override bool OnHit(HitInfo hit)
		{
			if (state != State.Alive)
			{
				if (hit.IsHorizontalMovement) velocity.X *= -1; else velocity.Y *= -1;
				//Asset.LoadSoundEffect("sfx/hit").Play();
				return true;
			}

			return OnHitAlive(hit);
		}

		protected virtual bool OnHitAlive(HitInfo hit)
		{
			return true;
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (state == State.Alive)
			{
				hp--;
				if (hp == 0)
				{
					velocity = player.Velocity * 2.0f;

					Sprite.Play("dead_bouncing");
					player.AddTireBlood(1.0f);
					GameScene.Map.DrawGroundEffect(X + dx * 6, Y + dy * 6, "blood" + Rand.NextInt(3), 1.0f, Rand.NextFloat(Mathf.Pi2));
					GameScene.Shaker.Shake(Vector2.Normalize(player.Velocity) * 4.0f);
					Asset.LoadSoundEffect("sfx/enemy_hurt").Play();
				}
				else
				{
					velocity = player.Velocity * 1.25f;

					player.AddTireBlood(0.25f);
					GameScene.Shaker.Shake(Vector2.Normalize(player.Velocity) * 2.0f);
					GameScene.Map.DrawGroundEffect(X + dx * 4, Y + dy * 4, "blood_small" + Rand.NextInt(2), 1.0f, Rand.NextFloat(Mathf.Pi2));
				}

				state = State.Bouncing;

				return hp > 0;
			}

			if (state == State.Dead)
			{
				if (hasBlood)
				{
					player.AddTireBlood(0.25f);
					GameScene.Map.DrawGroundEffect(X + dx * 4, Y + dy * 4, "blood_small" + Rand.NextInt(2), 1.0f, Rand.NextFloat(Mathf.Pi2));
					hasBlood = false;

					// TODO play sfx
				}

				player.AddTireBlood(0.02f);
				velocity = player.Velocity * 0.22f;
			}

			return false;
		}
	}
}
