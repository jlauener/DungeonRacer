using System;

namespace DungeonRacer
{
	class Door : GameEntity
	{
		public Door(Room room, EntityArguments args) : base(room, args)
		{
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Sprite.Play("close", () => Sprite.Play("idle"));
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			return true;
		}

		public void Open()
		{
			Collidable = false;
			Sprite.Play("open", RemoveFromScene);
		}
	}
}
