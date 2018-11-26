using System;
using MonoPunk;

namespace DungeonRacer
{
	class LockedDoor : GameEntity
	{
		public LockedDoor(EntityArguments args) : base(args)
		{
		}

		public override HitFlags HandlePlayerHit(Player player, int dx, int dy)
		{
			if (!player.UseItem(Data.ItemType))
			{
				return HitFlags.Stop;
			}

			Collidable = false;
			Sprite.Play("open", RemoveFromScene);
			Asset.LoadSoundEffect("sfx/car_hit").Play();
			return HitFlags.Nothing;
		}
	}
}
