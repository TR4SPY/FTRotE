using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public abstract class ItemEquippable : Item
    {
        [Header("Equipment Settings")]
        [Tooltip("The maximum durability points of this Item.")]
        public int maxDurability;
        [Tooltip("Maximum upgrade level allowed for this item.")]
        public int maxUpgradeLevel = 25;

        [Tooltip("The minimum required level to equip this Item.")]
        public int requiredLevel;

        [Tooltip("The minimum required strength to equip this Item.")]
        public int requiredStrength;

        [Tooltip("The minimum required dexterity to equip this Item.")]
        public int requiredDexterity;
    }
}
