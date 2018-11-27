using System;
using MonoPunk;

namespace DungeonRacer
{
	class Object : Entity
	{
		private readonly ObjectData data;

		public Object(ObjectData data) :base(data.Bounds.X, data.Bounds.Y)
		{
			this.data = data;
			Type = Global.TypeObject;
			Width = data.Bounds.Width;
			Height = data.Bounds.Height;
		}

		protected override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);

			var hit = CollideAt(X, Y, Global.TypePlayer);

			if(hit)
			{
				switch(data.Type)
				{
					case ObjectType.Link:
						((GameScene)Scene).GotoDungeon(data.Target);
						break;
					case ObjectType.Info:
						((GameScene)Scene).ShowInfo(data.Text);
						break;
				}
			}
		}
	}
}
