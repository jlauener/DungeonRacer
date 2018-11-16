using System;
using MonoPunk;

namespace DungeonRacer
{
	class CoinWidget : Entity
	{
		private readonly int maxCoin;
		private readonly Label label;

		public CoinWidget(Player player, int maxCoin, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			this.maxCoin = maxCoin;

			var icon = new TileSprite(Asset.GetTileset("entities_16_16"), 20);
			Add(icon, -6, -4);

			label = new Label(Global.Font, player.GetItemCount(ItemType.Coin) + "/" + maxCoin);
			label.HAlign = TextAlign.Left;
			Add(label, 10, 2);

			player.OnCollect += HandleCollect;
		}

		private void HandleCollect(Player player, ItemType item)
		{
			if (item != ItemType.Coin) return;

			label.Text = player.GetItemCount(ItemType.Coin) + "/" + maxCoin;
		}
	}
}
