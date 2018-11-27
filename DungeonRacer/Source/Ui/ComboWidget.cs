using System;
using MonoPunk;

namespace DungeonRacer
{
	class ComboWidget : Entity
	{
		private readonly Label label;

		public ComboWidget(PlayerData player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			label = new Label(Global.Font);
			label.Scale = 2.0f;
			label.HAlign = TextAlign.Center;
			Add(label);

			player.OnCombo += HandleCombo;
			player.OnComboStop += HandleComboStop;
		}

		private void HandleCombo(PlayerData player, EntityData entity, int count)
		{
			label.Text = entity.Name;
			if (count > 1) label.Text += " x " + count;
		}


		private void HandleComboStop(PlayerData player)
		{
			label.Text = "";
		}
	}
}
