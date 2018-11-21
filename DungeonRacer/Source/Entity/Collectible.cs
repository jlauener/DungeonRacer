using Microsoft.Xna.Framework;
using MonoPunk;
using System;

namespace DungeonRacer
{
	class Collectible : GameEntity
	{
		public Collectible(EntityArguments args) : base(args)
		{
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			if (Args.Position != Vector2.Zero)
			{
				Collidable = false;
				var targetY = Y;
				Y -= 8.0f;
				Scene.Tween(this, new { Y = targetY}, 0.3f).Ease(Ease.BackIn).OnComplete(() =>
				{
					Collidable = true;
				});
			}
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
