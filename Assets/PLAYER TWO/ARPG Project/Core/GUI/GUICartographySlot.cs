using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Cartography Slot")]
    public class GUICartographySlot : GUIItemSlot
    {
        public override bool CanEquip(GUIItem item) => !this.item && item != null;
        public override bool CanUnequip() => true;

        protected override void HandleRightClick()
        {
            if (item && item.TryMoveToLastPosition())
                Unequip();
        }
    }
}
