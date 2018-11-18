using System;
using MonoPunk;

namespace DungeonRacer
{
	class HpWidget : Entity
	{
		private readonly Bar bar;

		public HpWidget(Player player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			var back = new Sprite("gfx/ui/hp_bar_back");
			Add(back);

			bar = new Bar("gfx/ui/hp_bar_front");
			bar.Percent = player.Hp / player.MaxHp;
			Add(bar, 2, 2);

			player.OnModifyHp += HandleModifyHp;
		}

		private void HandleModifyHp(Player player, int delta)
		{
			bar.Percent = player.Hp / ((float) player.MaxHp);
		}
	}
}
