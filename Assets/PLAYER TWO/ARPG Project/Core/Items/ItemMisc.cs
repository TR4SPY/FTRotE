using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Misc Item", menuName = "PLAYER TWO/ARPG Project/Item/Misc Item")]
    public class ItemMisc : ItemEquippable, ItemQuest
    {
        public enum MiscType { Quest, Misc }
        public enum WearableType 
        { 
            None, Helm, Pants, Armor, Gloves, Boots, 
            OneHandedWeapon, TwoHandedWeapon, Shield, 
            Ring, Necklace, Glyph, Pet, Mount, Wings, Consumable 
        }

        [Header("Misc Settings")]
        public MiscType miscType;

        public WearableType wearable;
        public Buff[] buffsOnEquip;
        public bool disappearOnZeroDurability;
        public bool cannotUnequip;
        public bool disappearAfterDuration;
        public float lifetime;
        public bool disappearAfterBuffEnds;
    }
}
