using System;
using MonoPunk;

namespace DungeonRacer
{
	class TimeWidget : Entity
	{
		private readonly GameScene gameScene;

		private readonly Label label;
		private readonly Blinker blinker;

		public TimeWidget(GameScene gameScene, float x, float y) : base(x, y)
		{
			this.gameScene = gameScene;
			Layer = Global.LayerUi;

			label = new Label(Global.Font);
			label.Scale = 2.0f;
			label.HAlign = TextAlign.Center;
			Add(label);

			blinker = new Blinker(0.2f, label);
			Add(blinker);
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);
			label.Text = gameScene.Time.ToString("0.00");
			blinker.Enabled = gameScene.TimePaused && !gameScene.Finished;
		}
	}
}
