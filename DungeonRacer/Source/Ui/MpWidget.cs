using System;
using MonoPunk;

namespace DungeonRacer
{
	class MpWidget : Entity
	{
		private readonly Bar bar;

		public MpWidget(Player player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			var back = new Sprite("gfx/ui/mp_bar_back");
			Add(back);

			bar = new Bar("gfx/ui/mp_bar_front");
			bar.Percent = player.Mp / player.MaxMp;
			Add(bar, 2, 2);

			player.OnModifyMp += HandleModifyMp;
		}

		private void HandleModifyMp(Player player, float delta)
		{
			bar.Percent = player.Mp / player.MaxMp;
		}
	}
}
