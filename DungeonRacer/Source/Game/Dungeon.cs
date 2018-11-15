using System;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class Dungeon : Entity
	{
		public DungeonData Data { get; }

		private readonly DrawLayer tireLayer;

		public Dungeon(DungeonData data)
		{
			Data = data;

			Type = Global.TypeMap;
			Width = data.Width * Global.TileSize;
			Height = data.Height * Global.TileSize;

			var grid = new GridCollider(data.Width, data.Height, Global.TileSize, Global.TileSize);
			Collider = grid;

			var groundMap = new Tilemap(data.Tileset, data.Width, data.Height);
			groundMap.Layer = Global.LayerMapGround;
			Add(groundMap);

			var backMap = new Tilemap(data.Tileset, data.Width, data.Height);
			backMap.Layer = Global.LayerMapBack;
			Add(backMap);

			var frontMap = new Tilemap(data.Tileset, data.Width, data.Height);
			frontMap.Layer = Global.LayerMapFront;
			Add(frontMap);

			data.Iterate((tile) =>
			{
				if (tile.SolidType == TileSolidType.PixelMask)
				{
					grid.SetPixelMaskAt(tile.X, tile.Y, Asset.GetPixelMaskSet("dungeon").GetMask(tile.Id));
				}
				else
				{
					grid.SetTileAt(tile.X, tile.Y, tile.SolidType);
				}

				if (tile.Anim != null)
				{
					if (tile.Anim != null)
					{
						var sprite = new Animator(tile.Anim);
						switch (tile.Layer)
						{
							case DungeonTileLayer.Ground:
								sprite.Layer = Global.LayerMapGround;
								break;
							case DungeonTileLayer.Back:
								sprite.Layer = Global.LayerMapBack;
								break;
							case DungeonTileLayer.Front:
								sprite.Layer = Global.LayerMapFront;
								break;
						}
						sprite.Play("idle");
						Add(sprite, tile.X * Global.TileSize, tile.Y * Global.TileSize);
					}
				}
				else
				{
					switch (tile.Layer)
					{
						case DungeonTileLayer.Ground:
							groundMap.SetTileAt(tile.X, tile.Y, tile.DisplayTid);
							break;
						case DungeonTileLayer.Back:
							backMap.SetTileAt(tile.X, tile.Y, tile.DisplayTid);
							break;
						case DungeonTileLayer.Front:
							frontMap.SetTileAt(tile.X, tile.Y, tile.DisplayTid);
							break;
					}
				}
			});

			tireLayer = new DrawLayer(data.Width * Global.TileSize, data.Height * Global.TileSize);
			tireLayer.Layer = Global.LayerTireEffect;
			Add(tireLayer);
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			Data.Iterate((tile) =>
			{
				if (tile.Entity != null)
				{
					Scene.Add(GameEntity.Create(tile.Entity, this, tile));
				}
			});
		}

		public void DrawDriftEffect(float x, float y, float alpha, float angle)
		{
			tireLayer.BeginDraw();
			tireLayer.Draw(Asset.LoadTexture("gfx/game/tire_fx"), x - X, y - Y, Color.White * alpha, 8.0f, 8.0f, angle);
			tireLayer.EndDraw();
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			// TODO maybe we don't need this huge mask, but only few masks shared between map, entity and player.
			// e.g. "circle", "edged_square", "edge_corder_top_left", ...
			Asset.AddPixelMaskSet("dungeon", "gfx/game/tileset_mask", Global.TileSize, Global.TileSize);
		}
	}
}
