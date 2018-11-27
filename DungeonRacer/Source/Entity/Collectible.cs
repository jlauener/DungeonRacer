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
				var targetY = Y - 16.0f;
				//Y -= 8.0f;
				Scene.Tween(this, new { Y = Y - 32.0f}, 0.3f).Ease(Ease.QuadOut).OnComplete(() =>
				{
					Collect(Scene.GetEntity<Player>());
				});
			}
		}

		private void Collect(Player player)
		{
			Collidable = false;
			Data.OnCollect?.Invoke(player.Data);

			if (Data.CollectSfx != null) Data.CollectSfx.Play();
			Sprite.Play("collect", RemoveFromScene);
		}

		public override HitFlags HandlePlayerHit(Player player, int dx, int dy)
		{
			Collect(player);
			return HitFlags.Nothing;
		}
	}
}
