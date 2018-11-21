using MonoPunk;
using System;

namespace DungeonRacer
{
	class Ghost : GameEntity
	{
		public Ghost(EntityArguments args) : base(args)
		{
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			player.Damage(0.1f);
			Asset.LoadSoundEffect("sfx/car_hit_hurt").Play();
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
