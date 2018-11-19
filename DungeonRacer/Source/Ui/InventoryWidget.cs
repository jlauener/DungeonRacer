using System;
using MonoPunk;

namespace DungeonRacer
{
	class InventoryWidget : Entity
	{
		private const int ItemMax = 3;
		private const int ItemPan = 12;

		private readonly ItemSlot[] slots = new ItemSlot[ItemMax];

		public InventoryWidget(Player player, float x, float y) : base(x, y)
		{
			Layer = Global.LayerUi;

			for (var i = 0; i < slots.Length; i++)
			{
				slots[i] = new ItemSlot();
				Add(slots[i], -6 + i * ItemPan, -4);
			}
			player.OnCollect += HandleCollect;
			player.OnUse += HandleUse;
		}

		private void HandleCollect(Player player, ItemType item)
		{
			if (item == ItemType.Coin) return;

			foreach(var slot in slots)
			{
				if (slot.Set(item)) return;
			}

			Log.Error("Inventory full, will not diplay item " + item + ".");
		}

		private void HandleUse(Player player, ItemType item)
		{
			if (item == ItemType.Coin) return;

			for (var i = slots.Length - 1; i >= 0; i--)
			{
				if (slots[i].Remove(item)) return;
			}

			Log.Error("Item " + item + " not found in inventory.");
		}

		private class ItemSlot : Renderable
		{
			private bool hasItem;
			private ItemType item;

			private readonly Animator sprite;

			public ItemSlot()
			{
				sprite = new Animator("ui_item");
				Add(sprite);
			}

			public bool Remove(ItemType item)
			{
				if (!hasItem || this.item != item) return false;

				hasItem = false;
				sprite.Stop();
				return true;
			}

			public bool Set(ItemType item)
			{
				if (hasItem) return false;

				hasItem = true;
				this.item = item;
				sprite.Play(item.ToString());
				return true;
			}
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			var anim = new AnimatorData("entities_16_16");
			anim.Add("KeyA", 68);
			anim.Add("KeyB", 72);
			anim.Add("KeyC", 76);
			Asset.AddAnimatorData("ui_item", anim);
		}
	}
}
