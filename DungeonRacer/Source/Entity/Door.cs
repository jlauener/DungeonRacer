using System;

namespace DungeonRacer
{
	class Door : GameEntity
	{
		public Door(EntityData data, EntityArguments args) : base(data, args)
		{
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (!player.UseItem(Data.ItemType))
			{
				return true;
			}

			Collidable = false;
			Sprite.Play("open", RemoveFromScene);
			return false;
		}
	}
}
