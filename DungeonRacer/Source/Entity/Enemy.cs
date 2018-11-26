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

		private bool hasBlood = true;

		public Enemy(EntityArguments args) : base(args)
		{
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
						if (Hp == 0)
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
			MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid, Global.TypePlayer);
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

		public override HitFlags HandlePlayerHit(Player player, int dx, int dy)
		{
			if (state == State.Alive)
			{
				if (player.Speed < PlayerData.MinimumHitSpeed) return HitFlags.Stop;

				Hp--;
				if (Hp == 0)
				{
					if (Data.Loot != null)
					{
						Scene.Add(Create(Data.Loot, Position + new Vector2(0.0f, -4.0f)));
					}

					velocity = player.Velocity * 2.0f;
					state = State.Bouncing;

					Sprite.Play("dead_bouncing");
					player.AddTireBlood(1.0f);
					GameScene.Map.DrawGroundEffect(X + dx * 6, Y + dy * 6, "blood" + Rand.NextInt(3), 1.0f, Rand.NextFloat(Mathf.Pi2));
					GameScene.Shaker.Shake(Vector2.Normalize(player.Velocity) * 7.0f);
					Asset.LoadSoundEffect("sfx/enemy_hurt").Play();

					return HitFlags.Blood;
				}
				else
				{
					velocity = player.Velocity * 1.25f;
					state = State.Bouncing;

					Sprite.Play("hurt");
					player.AddTireBlood(0.25f);
					GameScene.Shaker.Shake(Vector2.Normalize(player.Velocity) * 4.0f);
					GameScene.Map.DrawGroundEffect(X + dx * 4, Y + dy * 4, "blood_small" + Rand.NextInt(2), 1.0f, Rand.NextFloat(Mathf.Pi2));
					Asset.LoadSoundEffect("sfx/enemy_hurt").Play();

					return HitFlags.Stop;
				}
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

			return HitFlags.Nothing;
		}
	}
}
