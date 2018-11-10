using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DungeonRacer
{
	class Boot : Engine
	{
		private Microsoft.Xna.Framework.Graphics.Effect crtEffect;

#if DEBUG
		private const bool FullScreen = false;
#else
		private const bool FullScreen = true;
#endif

		public Boot() : base(width: Global.ScreenWidth * Global.Scale, height: Global.ScreenHeight * Global.Scale, windowScale: 1, fullScreen: FullScreen)
		{
			OnViewportChanged += HandleViewportChanged;
		}

		protected override void OnLoadContent()
		{
			Input.Define("left", Keys.A, Keys.Left);
			Input.Define("left", Buttons.DPadLeft);

			Input.Define("right", Keys.D, Keys.Right);
			Input.Define("right", Buttons.DPadRight);

			Input.Define("up", Keys.W, Keys.Up);
			Input.Define("up", Buttons.DPadUp);

			Input.Define("down", Keys.S, Keys.Down);
			Input.Define("down", Buttons.DPadDown);

			Input.Define("a", Keys.Z, Keys.N);
			Input.Define("a", Buttons.A);

			Input.Define("b", Keys.X, Keys.M);
			Input.Define("b", Buttons.B);

			Input.Define("start", Keys.Enter, Keys.Space);
			Input.Define("start", Buttons.Start);

			Input.Define("back", Keys.Escape);
			Input.Define("back", Buttons.Back);

//#if DEBUG
			Input.Define("reset", Keys.R);
			Input.Define("debug_1", Keys.D1);
			Input.Define("debug_2", Keys.D2);
			Input.Define("debug_3", Keys.D3);
			Input.Define("debug_4", Keys.D4);
			Input.Define("debug_5", Keys.D5);
			Input.Define("debug_6", Keys.D6);
			Input.Define("debug_7", Keys.D7);
			Input.Define("debug_8", Keys.D8);
			Input.Define("debug_9", Keys.D9);
			Input.Define("debug_10", Keys.D0);
			//#endif

			EntityData.Init();
			PlayerData.Init();
			RoomData.Init();

			crtEffect = Asset.LoadEffect("effects/CRT-easymode");
			crtEffect.Parameters["InputSize"].SetValue(new Vector2(Width, Height));
			if (Global.CrtEnabled)
			{
				PostProcessor = crtEffect;
			}
		}

		protected override void OnStart()
		{
			Scene = new GameScene();		
		}

		private void HandleViewportChanged(int width, int height, bool fullscreen)
		{
			crtEffect.Parameters["OutputSize"].SetValue(new Vector2(width, height));
		}
	}

#if WINDOWS || LINUX

	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using (var game = new Boot())
				game.Run();
		}
	}

#endif
}
