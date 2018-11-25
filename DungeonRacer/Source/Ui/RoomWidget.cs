using System;
using MonoPunk;

namespace DungeonRacer
{
	class RoomWidget : Entity
	{
		private readonly Label label;

		public RoomWidget(GameScene gameScene, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			label = new Label(Global.Font);
			label.HAlign = TextAlign.Right;
			Add(label);

			//gameScene.OnEnterRoom += (scene, room) =>
			//{
			//	label.Text = room.RoomX + "-" + room.RoomY;
			//};
		}
	}
}
