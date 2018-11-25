using System;
using MonoPunk;

namespace DungeonRacer
{
	class Ogre : Enemy
	{
		private const float MaxSpeed = 6.0f;
		private const float SteerForce = 10.0f;
		private const float DistanceToPlayerMin = 10.0f;

		public Ogre(EntityArguments args) : base(args)
		{
		}

		protected override void OnUpdateAlive(float deltaTime)
		{
			if (GameScene.Player.Alive)
			{
				var delta = GameScene.Player.Position - Position;
				if (delta.Length() > 10.0f)
				{
					delta.Normalize();
					velocity += delta * deltaTime * SteerForce;
					if (velocity.Length() > MaxSpeed)
					{
						velocity.Normalize();
						velocity *= MaxSpeed;
					}
				}
			}

			base.OnUpdateAlive(deltaTime);
			Sprite.FlipX = velocity.X < 0.0f;
		}

		protected override void OnStartAlive()
		{
			Sprite.Play("walk");
		}
	}
}
