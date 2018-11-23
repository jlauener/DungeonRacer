using System;
using MonoPunk;

namespace DungeonRacer
{
	class Goblin : Enemy
	{
		public Goblin(Room room, EntityArguments args) : base(room, args)
		{
			velocity = DirectionUtils.GetNormal(Direction) * 20.0f;
			UpdateSprite();
		}

		protected override bool OnHitAlive(HitInfo hit)
		{
			Direction = DirectionUtils.GetOpposite(Direction);
			velocity *= -1;
			UpdateSprite();
			return true;
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
