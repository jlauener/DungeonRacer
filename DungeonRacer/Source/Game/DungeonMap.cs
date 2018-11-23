﻿using System;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class DungeonMap : Entity
	{
		public DungeonData Data { get; }

		public Room StartingRoom { get; private set; }

		private readonly DrawLayer tireLayer;

		public DungeonMap(DungeonData data)
		{	
			Data = data;

			Type = Global.TypeMap;
			Width = data.WidthTiles * Global.TileSize;
			Height = data.HeightTiles * Global.TileSize;

			var solidGrid = new GridCollider(data.WidthTiles, data.HeightTiles, Global.TileSize, Global.TileSize);
			Collider = solidGrid;

			var groundMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			groundMap.Layer = Global.LayerMapGround;
			Add(groundMap);

			var backMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			backMap.Layer = Global.LayerMapBack;
			Add(backMap);

			var frontMap = new Tilemap(data.Tileset, data.WidthTiles, data.HeightTiles);
			frontMap.Layer = Global.LayerMapFront;
			Add(frontMap);

			Data.IterateTiles((tile) =>
			{
				if (tile.Trigger == TriggerType.None)
				{
					if (tile.SolidType == TileSolidType.PixelMask)
					{
						solidGrid.SetPixelMaskAt(tile.X, tile.Y, Asset.GetPixelMaskSet("dungeon").GetMask(tile.Id), tile.Properties);
					}
					else
					{
						solidGrid.SetTileAt(tile.X, tile.Y, tile.SolidType, tile.Properties);
					}
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

			tireLayer = new DrawLayer(data.WidthTiles * Global.TileSize, data.HeightTiles * Global.TileSize);
			tireLayer.Layer = Global.LayerTireEffect;
			Add(tireLayer);
		}

		protected override void OnAdded()
		{
			base.OnAdded();

			var triggerGrid = new GridCollider(Data.WidthTiles, Data.HeightTiles, Global.TileSize, Global.TileSize);
			Data.IterateTiles((tile) =>
			{
				if (tile.Trigger != TriggerType.None)
				{
					if (tile.SolidType == TileSolidType.PixelMask)
					{
						triggerGrid.SetPixelMaskAt(tile.X, tile.Y, Asset.GetPixelMaskSet("dungeon").GetMask(tile.Id), tile.Properties);
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

		[AssetLoader]
		public static void LoadAssets()
		{
			// TODO maybe we don't need this huge mask, but only few masks shared between map, entity and player.
			// e.g. "circle", "edged_square", "edge_corder_top_left", ...
			Asset.AddPixelMaskSet("dungeon", "gfx/mask/tileset_mask", Global.TileSize, Global.TileSize);
		}
	}
}
