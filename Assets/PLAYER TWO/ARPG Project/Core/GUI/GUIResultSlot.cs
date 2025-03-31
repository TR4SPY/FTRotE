using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class GUIResultSlot : GUIItemSlot
    {
        public override bool CanEquip(GUIItem item) => false;
        public override bool CanUnequip() => true;
    }
}
