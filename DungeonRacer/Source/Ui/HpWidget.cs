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

			bar = new Bar("gfx/ui/hp_bar");
			bar.Percent = player.Hp / player.MaxHp;
			Add(bar);

			player.OnModifyHp += HandleModifyHp;
		}

		private void HandleModifyHp(Player player, float delta)
		{
			bar.Percent = player.Hp / player.MaxHp;
		}
	}
}
