using System;

namespace DungeonRacer
{
	class Door : GameEntity
	{
		public Door(EntityArguments args) : base(args)
		{
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Sprite.Play("close", () => Sprite.Play("idle"));
		}

		public override HitFlags HandlePlayerHit(Player player, int dx, int dy)
		{
			return HitFlags.Stop;
		}

		public void Open()
		{
			Collidable = false;
			Sprite.Play("open", RemoveFromScene);
		}
	}
}
