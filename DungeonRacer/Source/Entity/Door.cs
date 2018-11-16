using System;

namespace DungeonRacer
{
	class Door : GameEntity
	{
		public Door(EntityData data, Dungeon dungeon, DungeonTile tile) : base(data, dungeon, tile)
		{
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			if (!player.UseItem(ItemType.Key))
			{
				return true;
			}

			Collidable = false;
			Sprite.Play("open", RemoveFromScene);
			return false;
		}
	}
}
