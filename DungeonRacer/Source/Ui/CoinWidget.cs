using System;
using MonoPunk;

namespace DungeonRacer
{
	class CoinWidget : Entity
	{
		private readonly Label label;

		public CoinWidget(Player player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			label = new Label(Global.Font, "$ " + player.GetItemCount(ItemType.Coin));
			label.HAlign = TextAlign.Left;
			Add(label);

			player.OnCollect += HandleCollect;
		}

		private void HandleCollect(Player player, ItemType item)
		{
			if (item != ItemType.Coin) return;

			label.Text = "$ " + player.GetItemCount(ItemType.Coin);
		}
	}
}
