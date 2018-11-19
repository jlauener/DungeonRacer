using Microsoft.Xna.Framework;
using System;

namespace DungeonRacer
{
	enum Direction
	{
		Right,
		Down,
		Left,
		Up,
	}

	static class DirectionUtils
	{
		public static Direction GetOpposite(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return Direction.Right;
				case Direction.Right:
					return Direction.Left;
				case Direction.Up:
					return Direction.Down;
				default: //Direction.Down:
					return Direction.Up;

			}
		}

		public static Direction RotateCW(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return Direction.Up;
				case Direction.Right:
					return Direction.Down;
				case Direction.Up:
					return Direction.Right;
				default: //Direction.Down:
					return Direction.Left;

			}
		}

		public static Direction RotateCCW(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return Direction.Down;
				case Direction.Right:
					return Direction.Up;
				case Direction.Up:
					return Direction.Left;
				default: //Direction.Down:
					return Direction.Right;
			}
		}

		public static Vector2 GetNormal(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return new Vector2(-1.0f, 0.0f);
				case Direction.Right:
					return new Vector2(1.0f, 0.0f);
				case Direction.Up:
					return new Vector2(0.0f, -1.0f);
				default: //Direction.Down:
					return new Vector2(0.0f, 1.0f);
			}
		}

		public static Direction GetDirection(Vector2 vec)
		{
			if (vec.X < 0.0f)
			{
				return Direction.Left;
			}

			if (vec.X > 0.0f)
			{
				return Direction.Right;
			}

			if (vec.Y < 0.0f)
			{
				return Direction.Up;
			}

			if (vec.Y > 0.0f)
			{
				return Direction.Down;
			}

			return Direction.Down;
		}

		public static Direction FromByte(byte value)
		{
			return (Direction)value;
		}

		public static byte ToByte(Direction direction)
		{
			return (byte)direction;
		}
	}
}
