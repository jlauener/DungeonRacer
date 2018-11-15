using System;

namespace DungeonRacer
{
	class Item : GameEntity
	{		
		public Item(EntityData data, Dungeon dungeon, DungeonTile tile) : base(data, dungeon, tile)
		{
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			player.Collect(Data.ItemType);
			Collidable = false;

			if (Data.CollectSfx != null) Data.CollectSfx.Play();
			Sprite.Play("collect", RemoveFromScene);
			return false;
		}
	}
}
