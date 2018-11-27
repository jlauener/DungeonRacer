using System;
using MonoPunk;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DungeonRacer
{
	class DungeonMap : Entity
	{
		private const int TileGround = 8;
		private const int TileWall = 0;
		private const int TileRoof = 48;

		public DungeonData Data { get; }

		private readonly Dictionary<int, EntityGroup> groups = new Dictionary<int, EntityGroup>();

		private readonly DrawLayer tireLayer;

		public DungeonMap(DungeonData data)
		{
			Data = data;

			Type = Global.TypeMap;
			Width = data.WidthTiles * Global.TileSize;
			Height = data.HeightTiles * Global.TileSize;

			var solidGrid = new GridCollider(data.WidthTiles, data.HeightTiles, Global.TileSize, Global.TileSize, "tileset");
			Collider = solidGrid;

			var groundMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			groundMap.Layer = Global.LayerMapGround;
			Add(groundMap);

			var backMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			backMap.Layer = Global.LayerMapWall;
			Add(backMap);

			var frontMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			frontMap.Layer = Global.LayerMapRoof;
			Add(frontMap);

			Data.IterateTiles((tile) =>
			{
				if (tile.Trigger == null)
				{
					if (tile.SolidType == TileSolidType.PixelMask)
					{
						solidGrid.SetPixelMaskAt(tile.X, tile.Y, tile.DisplayTid, tile.Properties);
					}
					else
					{
						solidGrid.SetTileAt(tile.X, tile.Y, tile.SolidType, tile.Properties);
					}
				}

				switch (tile.Type)
				{
					case DungeonTileType.Ground:
						RenderFlatTile(groundMap, tile, TileGround);
						break;
					case DungeonTileType.Wall:
						RenderWallTile(solidGrid, backMap, tile, TileWall);
						break;
					case DungeonTileType.Roof:
						RenderFlatTile(frontMap, tile, TileRoof);
						break;
				}

				if (tile.Trigger != null)
				{
					backMap.SetTileAt(tile.X, tile.Y, tile.DisplayTid);
				}
			});

			tireLayer = new DrawLayer(data.WidthTiles * Global.TileSize, data.HeightTiles * Global.TileSize);
			tireLayer.Layer = Global.LayerTireEffect;
			Add(tireLayer);
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			var triggerGrid = new GridCollider(Data.WidthTiles, Data.HeightTiles, Global.TileSize, Global.TileSize, "tileset");
			Data.IterateTiles((tile) =>
			{
				if (tile.Trigger != null)
				{
					if (tile.SolidType == TileSolidType.PixelMask)
					{
						triggerGrid.SetPixelMaskAt(tile.X, tile.Y, tile.DisplayTid, tile.Properties);
					}
					else
					{
						triggerGrid.SetTileAt(tile.X, tile.Y, tile.SolidType, tile.Properties);
					}
				}
			});

			var triggerEntity = new Entity();
			triggerEntity.Type = Global.TypeTrigger;
			triggerEntity.Collider = triggerGrid;
			Scene.Add(triggerEntity);

			Data.IterateObjects((obj) => Scene.Add(new Object(obj)));

			Log.Debug("Loading groups...");
			Data.IterateEntities((args) =>
			{
				var entity = GameEntity.Create(args);
				if (args.GroupId != -1)
				{
					EntityGroup group;
					if (!groups.TryGetValue(args.GroupId, out group))
					{
						group = new EntityGroup(args.GroupId, Data.GetGroupReward(args.GroupId));
						groups[args.GroupId] = group;
						Log.Debug("Created group " + args.GroupId);
					}

					group.Add(entity);
					entity.Group = group;
				}
				Scene.Add(entity);
			});
		}

		public void DrawGroundEffect(float x, float y, string name, float alpha = 1.0f, float angle = 0.0f)
		{
			DrawGroundEffect(x, y, name, Color.White, alpha, angle);
		}

		public void DrawGroundEffect(float x, float y, string name, Color color, float alpha = 1.0f, float angle = 0.0f)
		{
			tireLayer.BeginDraw();
			tireLayer.Draw(Asset.LoadTexture("gfx/game/fx_" + name), x - X, y - Y, color * alpha, 8.0f, 8.0f, angle);
			tireLayer.EndDraw();
		}

		[Flags]
		enum TilePositions
		{
			None = 0x00,
			Left = 0x01,
			Right = 0x02,
			Up = 0x04,
			Down = 0x08
		}

		private DungeonTileType GetTileTypeAt(int x, int y)
		{
			var tile = Data.GetTileAt(x, y);
			return tile != null ? tile.Type : DungeonTileType.Ground;
		}

		private void RenderFlatTile(Tilemap tilemap, DungeonTile tile, int tid)
		{
			var neighbors = GetNeighbors(tile);

			// straight edges
			if (TrySetTile(tilemap, tile, tid + 18, TilePositions.Up | TilePositions.Down | TilePositions.Left)) return;
			if (TrySetTile(tilemap, tile, tid + 16, TilePositions.Up | TilePositions.Down | TilePositions.Right)) return;
			if (TrySetTile(tilemap, tile, tid + 33, TilePositions.Left | TilePositions.Right | TilePositions.Up)) return;
			if (TrySetTile(tilemap, tile, tid + 1, TilePositions.Left | TilePositions.Right | TilePositions.Down)) return;

			// corners
			if (TrySetTile(tilemap, tile, tid + 0, TilePositions.Right | TilePositions.Down)) return;
			if (TrySetTile(tilemap, tile, tid + 2, TilePositions.Left | TilePositions.Down)) return;
			if (TrySetTile(tilemap, tile, tid + 32, TilePositions.Right | TilePositions.Up)) return;
			if (TrySetTile(tilemap, tile, tid + 34, TilePositions.Left | TilePositions.Up)) return;

			// vertical
			if (TrySetTile(tilemap, tile, tid + 20, TilePositions.Up | TilePositions.Down)) return;

			// horizontal
			if (TrySetTile(tilemap, tile, tid + 36, TilePositions.Left | TilePositions.Right)) return;

			var rnd = Rand.NextFloat();
			if (rnd < 0.9f)
			{
				tilemap.SetTileAt(tile.X, tile.Y, tid + 17);
			}
			else if (rnd < 0.94f)
			{
				tilemap.SetTileAt(tile.X, tile.Y, tid + 3);
			}
			else if (rnd < 0.975f)
			{
				tilemap.SetTileAt(tile.X, tile.Y, tid + 19);
			}
			else
			{
				tilemap.SetTileAt(tile.X, tile.Y, tid + 35);
			}
		}

		private bool TrySetTile(Tilemap tilemap, DungeonTile tile, int tid, TilePositions neighbors)
		{
			if (GetNeighbors(tile) != neighbors)
			{
				return false;
			}

			SetTile(tilemap, tile, tid);
			return true;
		}

		private void RenderWallTile(GridCollider solidGrid, Tilemap tilemap, DungeonTile tile, int tid)
		{
			var neighbors = GetNeighbors(tile);

			// vertical
			if (GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Roof && GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Ground)
			{
				SetTile(tilemap, tile, tid + 16, Direction.Left);
			}
			else if (GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Roof && GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Ground)
			{
				SetTile(tilemap, tile, tid + 18, Direction.Right);
			}

			// horizontal
			else if (GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Roof && GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Ground)
			{
				SetTile(tilemap, tile, tid + 1, Direction.Up);
			}
			else if (GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Roof && GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Ground)
			{
				SetTile(tilemap, tile, tid + 33, Direction.Down);
			}

			// corner outer
			else if (GetTileTypeAt(tile.X + 1, tile.Y + 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 0);
			}
			else if (GetTileTypeAt(tile.X - 1, tile.Y + 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 2);
			}
			else if (GetTileTypeAt(tile.X + 1, tile.Y - 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 32);
			}
			else if (GetTileTypeAt(tile.X - 1, tile.Y - 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 34);
			}

			// corner inner
			else if (GetTileTypeAt(tile.X - 1, tile.Y - 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 3);
				solidGrid.SetPixelMaskAt(tile.X, tile.Y, tid + 3);
			}
			else if (GetTileTypeAt(tile.X + 1, tile.Y - 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y + 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 4);
				solidGrid.SetPixelMaskAt(tile.X, tile.Y, tid + 4);
			}
			else if (GetTileTypeAt(tile.X - 1, tile.Y + 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X + 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 19);
				solidGrid.SetPixelMaskAt(tile.X, tile.Y, tid + 19);
			}
			else if (GetTileTypeAt(tile.X + 1, tile.Y + 1) == DungeonTileType.Ground && GetTileTypeAt(tile.X - 1, tile.Y) == DungeonTileType.Wall && GetTileTypeAt(tile.X, tile.Y - 1) == DungeonTileType.Wall)
			{
				SetTile(tilemap, tile, tid + 20);
				solidGrid.SetPixelMaskAt(tile.X, tile.Y, tid + 20);
			}

			// error
			else
			{
				Log.Error("Unkown wall tile at " + tile);
			}
		}

		private void SetTile(Tilemap tilemap, DungeonTile tile, int tid, Direction direction = Direction.Down)
		{
			if (tile.Anim != null)
			{
				var sprite = new Animator(tile.Anim);
				switch (tile.Type)
				{
					case DungeonTileType.Ground:
						sprite.Layer = Global.LayerMapGround;
						break;
					case DungeonTileType.Wall:
						sprite.Layer = Global.LayerMapWall;
						break;
					case DungeonTileType.Roof:
						sprite.Layer = Global.LayerMapRoof;
						break;
				}
				sprite.Play(direction.ToString());
				Add(sprite, tile.X * Global.TileSize, tile.Y * Global.TileSize);
			}
			else if (tile.DisplayTid != -1)
			{
				tilemap.SetTileAt(tile.X, tile.Y, tile.DisplayTid);
			}
			else
			{
				tilemap.SetTileAt(tile.X, tile.Y, tid);
			}
		}

		private TilePositions GetNeighbors(DungeonTile tile)
		{
			TilePositions result = TilePositions.None;
			if (GetTileTypeAt(tile.X - 1, tile.Y) == tile.Type) result |= TilePositions.Left;
			if (GetTileTypeAt(tile.X + 1, tile.Y) == tile.Type) result |= TilePositions.Right;
			if (GetTileTypeAt(tile.X, tile.Y - 1) == tile.Type) result |= TilePositions.Up;
			if (GetTileTypeAt(tile.X, tile.Y + 1) == tile.Type) result |= TilePositions.Down;
			return result;
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			Asset.AddPixelMaskSet("tileset", "mask/tileset", Global.TileSize, Global.TileSize);

			var anim = new AnimatorData("gfx/game/tileset", 16, 16);
			anim.Add("Left", AnimatorMode.Loop, 0.2f, 224, 225, 226, 227);
			anim.Add("Right", AnimatorMode.Loop, 0.2f, 240, 241, 242, 243);
			anim.Add("Up", AnimatorMode.Loop, 0.2f, 192, 193, 194, 195);
			anim.Add("Down", AnimatorMode.Loop, 0.2f, 208, 209, 210, 211);
			Asset.AddAnimatorData("torch", anim);
		}
	}
}
