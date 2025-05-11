using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class Item : ScriptableObject
    {
        public enum Rarity
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2,
            Epic = 3,
            Legendary = 4
        }

        public enum ItemGroup
        {
            Swords = 0,
            Axes = 1,
            Bows = 2,
            Staffs = 3,
            Shields = 4,
            Books = 5,
            Helmets = 6,
            Chests = 7,
            Pants = 8,
            Gloves = 9,
            Boots = 10,
            Wings = 11,
            Jewels = 12,
            Consumables = 13,
            Misc = 14,
        }

        [Header("Item Settings")]
        [Tooltip("The unique identifier for this Item.")]
        public int id;
        
        [Tooltip("The prefab that represents this Item in the game scene.")]
        public GameObject prefab;

        [Tooltip("The base price of this Item.")]
        public int price;

        [Tooltip("The rarity of the item.")]
        public Rarity rarity;

        [Header("Drop Settings")]
        [Tooltip("The position relative to the drop point to place the Item when dropping it.")]
        public Vector3 dropPosition;

        [Tooltip("The rotation relative to world space to place the Item when dropping it.")]
        public Vector3 dropRotation = new Vector3(-90, 0, 45);

        [Header("Inventory Settings")]
        [Tooltip("The sprite that represents this Item on the Inventory.")]
        public Sprite image;

        [Tooltip("The number of rows this Item occupies in the Inventory.")]
        public int rows = 1;

        [Tooltip("The number of column this Item occupies in the Inventory.")]
        public int columns = 1;

        [Tooltip("If true, this Item can be stacked on the Inventory.")]
        public bool canStack;

        [Tooltip("The maximum stack size of this Item on the Inventory.")]
        public int stackCapacity;

        [Tooltip("Is this blade a quest item?")]
        public bool isQuestSpecific = false;

        [Tooltip("This item cannot be dropped from inventory.")]
        public bool cannotBeDropped = false;

        [Tooltip("This item cannot be sold to vendors.")]
        public bool cannotBeSold = false;

        [Header("Class Restriction (Bitmask)")]
        [Tooltip("Define, which classes can equip the item.")]
        public CharacterClassRestrictions allowedClasses = CharacterClassRestrictions.None;

        [Tooltip("The group/category this item belongs to.")]
        public ItemGroup group = ItemGroup.Misc;


        /// <summary>
        /// Instantiates the Item's prefab as child of a given Transform.
        /// </summary>
        /// <param name="slot">The slot to attach the Item to.</param>
        /// <returns>Returns the instance of the newly instantiated Game Object.</returns>
        public virtual GameObject Instantiate(Transform slot)
        {
            return Instantiate(prefab, slot);
        }

        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Returns true if this item is quest related.
        /// </summary>
        public bool IsQuestSpecific => isQuestSpecific;
    }
}
