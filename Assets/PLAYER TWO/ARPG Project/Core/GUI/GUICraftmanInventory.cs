using UnityEngine;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Craftman Inventory")]
    public class GUICraftmanInventory : GUIInventory
    {
        protected GUICraftman m_craftman;
        public Inventory inventory => m_inventory;

        protected virtual void InitializeCraftman() =>
                    m_craftman = GetComponentInParent<GUICraftman>();

                public override bool TryRemove(GUIItem item)
        {
            bool success = base.TryRemove(item);
            if (success)
            {
                m_craftman?.UpdateCraftingPreview(
                    inventory.items.Keys.ToList()
                );
            }
            return success;
        }

        public override bool TryPlace(GUIItem item)
        {
            if (item == null || item.item == null)
                return false;

            if (item.item.data.cannotBeDropped)
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            if (GUIWindowsManager.instance.GetInventory().Contains(item) &&
                !GUIWindowsManager.instance.GetInventory().TryRemove(item))
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            if (!TryPlaceInClosestCell(item))
            {
                GameAudio.instance?.PlayDeniedSound();
                return false;
            }

            m_craftman?.UpdateCraftingPreview(
                inventory.items.Keys.ToList()
            );
            return true;
        }

        private bool TryPlaceInClosestCell(GUIItem item)
        {
            var cell = FindClosestCell(item);
            if (m_inventory.TryInsertItem(item.item, cell.x, cell.y))
            {
                Place(item, cell.x, cell.y);
                return true;
            }
            return false;
        }

        protected virtual void Start()
        {
            InitializeCraftman();
        }
    }
}
