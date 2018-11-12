using System;

namespace DungeonRacer
{
	static class Global
	{
		public static bool CrtEnabled = false;

		public const float PlayerTimeInitial = 30.0f;

		public static int ScreenWidth = 256;
		public static int ScreenHeight = 224;

		public const int RoomWidth = 17;
		public const int RoomHeight = 13;
		public const int RoomWidthPx = RoomWidth * TileSize;
		public const int RoomHeightPx = RoomHeight * TileSize;

		public const int TileSize = 16;

		public const int UiHeight = 2 * TileSize;

		// Prevents player car from being already visible on the next room when switching room.
		public const int RoomSwitchMargin = 3;

		public const int TypeMap = 1;
		public const int TypePlayer = 2;
		public const int TypeEntity = 3;

		public const int LayerUi = 20;
		public const int LayerMapFront = 30;
		public const int LayerFront = 40;
		public const int LayerMain = 50;
		public const int LayerBack = 60;
		public const int LayerMapBack = 100;

		public const string Font = "font/04b03";
	}
}
