using System;
using MonoPunk;

namespace DungeonRacer
{
	class HpWidget : Entity
	{
		private readonly Bar bar;

		public HpWidget(PlayerData player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			bar = new Bar("gfx/ui/hp_bar");
			Add(bar);

			player.OnDamage += HandleDamage;
			player.OnHeal += HandleHeal;
		}

		private void HandleDamage(PlayerData player, int value, DamageType damageType)
		{
			bar.Percent = player.Hp / (float)player.MaxHp;
		}

		private void HandleHeal(PlayerData player, int value)
		{
			bar.Percent = player.Hp / (float)player.MaxHp;
		}


	}
}
