using Microsoft.Xna.Framework;
using MonoPunk;
using System;

namespace DungeonRacer
{
	class Ghost : GameEntity
	{
		private const float MaxSpeed = 6.0f;
		private const float SteerForce = 10.0f;
		private const float DistanceToPlayerMin = 10.0f;
		
		private Vector2 velocity;

		public Ghost(Room room, Vector2 position) : base(room, new EntityArguments(EntityData.Get("ghost"), position))
		{
			Sprite.Alpha = 0.3f;
		}

		protected override void OnUpdateActive(float deltaTime)
		{
			if (GameScene.Player.Alive)
			{
				var delta = GameScene.Player.Position - Position;
				if (delta.Length() > DistanceToPlayerMin)
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

			MoveBy(velocity * deltaTime);

			if (velocity.X > 0.0f && Right > Room.Right - Global.TileSize) velocity.X = -velocity.X;
			else if(velocity.X < 0.0f && Left < Room.Left + Global.TileSize) velocity.X = -velocity.X;

			if (velocity.Y > 0.0f && Bottom > Room.Bottom - Global.TileSize) velocity.Y = -velocity.Y;
			else if (velocity.Y < 0.0f && Top < Room.Top + Global.TileSize) velocity.Y = -velocity.Y;

			Sprite.FlipX = velocity.X < 0.0f;
		}
	}
}
