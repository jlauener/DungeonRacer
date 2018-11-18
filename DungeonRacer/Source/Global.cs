using System;

namespace DungeonRacer
{
	static class Global
	{
		public static bool CrtEnabled = false;
		public static bool ScrollingEnabled = true;

		public static int ScreenWidth = 256;
		public static int ScreenHeight = 224;

		public const int TileSize = 16;

		public const int RoomWidth = 16;
		public const int RoomHeight = 12;
		public const int RoomWidthPx = RoomWidth * TileSize;
		public const int RoomHeightPx = RoomHeight * TileSize;

		public const int UiHeight = 32;

		// Prevents player car from being already visible on the next room when switching room.
		public const int RoomSwitchMargin = 3;

		public const int TypeMap = 1;
		public const int TypePlayer = 2;
		public const int TypeEntity = 3;

		public const int LayerUi = 20;
		public const int LayerMapFront = 30;
		public const int LayerFront = 40;
		public const int LayerMain = 50;
		public const int LayerBack = 55;
		public const int LayerMapBack = 60;
		public const int LayerTireEffect = 70;
		public const int LayerMapGround = 80;

		public const string Font = "font/04b03";
	}
}
