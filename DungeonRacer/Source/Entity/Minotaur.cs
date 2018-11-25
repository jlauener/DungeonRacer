using System;
using Microsoft.Xna.Framework;
using MonoPunk;

namespace DungeonRacer
{
	class Minotaur : Enemy
	{
		//private const float MaxSpeed = 20.0f;
		//private const float SteerForce = 10.0f;
		private const float ShootDistanceMin = 20.0f;
		private const float ShootDistanceMax = 200.0f;
		private const float ShootCooldown = 0.8f;
		private static readonly Vector2 ShootOffset = new Vector2(0.0f, -8.0f);

		private float shootCounter = ShootCooldown;

		public Minotaur(EntityArguments args) : base(args)
		{
		}

		protected override void OnUpdateAlive(float deltaTime)
		{
			if (GameScene.Player.Alive)
			{
				var delta = GameScene.Player.Position - Position;
				var dist = delta.Length();
				if (dist > ShootDistanceMin && dist < ShootDistanceMax)
				{
					delta.Normalize();
					if(shootCounter <= 0.0f)
					{
						Scene.Add(new Projectile(Position + ShootOffset, delta));
						shootCounter += ShootCooldown;
						Sprite.Play("shoot", () => Sprite.Play("shoot_warning"));
					}
					//velocity += delta * deltaTime * SteerForce;
					//if (velocity.Length() > MaxSpeed)
					//{
					//	velocity.Normalize();
					//	velocity *= MaxSpeed;
					//}

					shootCounter -= deltaTime;
				}
				else
				{
					//velocity += -delta * deltaTime * SteerForce;
					//if (velocity.Length() > MaxSpeed)
					//{
					//	velocity.Normalize();
					//	velocity *= MaxSpeed;
					//}
					//shootCounter = ShootCooldown;
					shootCounter = ShootCooldown;
				}

				Sprite.FlipX = delta.X < 0.0f;
			}

			base.OnUpdateAlive(deltaTime);
		}

		protected override void OnStartAlive()
		{
			Sprite.Play("shoot_warning");
		}
	}

	class Projectile : Entity
	{
		private Vector2 velocity;

		public Projectile(Vector2 position, Vector2 direction) : base(position)
		{
			SetHitbox(8, 8, 4, 4);

			velocity = direction * 100.0f;

			var sprite = new Animator("projectile");
			sprite.CenterOrigin();
			sprite.Play("idle");
			Add(sprite);
		}

		protected override void OnUpdate(float deltaTime)
		{
			MoveBy(velocity * deltaTime, Global.TypeMap, Global.TypeSolid, Global.TypePlayer);
			base.OnUpdate(deltaTime);
		}

		protected override bool OnHit(HitInfo info)
		{
			if(info.Other is Player)
			{
				((Player)info.Other).Damage(10, DamageType.Entity);
			}

			RemoveFromScene();
			return true;
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			var anim = new AnimatorData("entities_16_16");
			anim.Add("idle", AnimatorMode.Loop, 0.2f, 240, 241, 242, 243, 244, 245, 246, 247);
			Asset.AddAnimatorData("projectile", anim);
		}
	}
}
