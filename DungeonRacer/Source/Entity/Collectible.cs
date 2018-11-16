using System;

namespace DungeonRacer
{
	class Collectible : GameEntity
	{		
		public Collectible(EntityData data, Dungeon dungeon, DungeonTile tile) : base(data, dungeon, tile)
		{
		}

		public override bool HandlePlayerHit(Player player, int dx, int dy)
		{
			Collidable = false;		
			Data.OnCollect?.Invoke(player);

			if (Data.CollectSfx != null) Data.CollectSfx.Play();
			Sprite.Play("collect", RemoveFromScene);
			return false;
		}
	}
}
