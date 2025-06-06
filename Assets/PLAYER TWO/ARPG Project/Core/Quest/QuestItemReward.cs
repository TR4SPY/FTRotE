using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class QuestItemReward
    {
        [Tooltip("The scriptable object of the Item.")]
        public Item data;

        [Tooltip("The amount of additional attributes the Item will have.")]
        public int attributes;

        [Tooltip("The amount of elemental resistances the Item will have.")]
        public int elements;

        [Tooltip("The number of items to reward.")]
        public int amount = 1;

        /// <summary>
        /// Returns a new Item Instance based on this object's attributes.
        /// </summary>
        public ItemInstance CreateItemInstance()
        {
            if (attributes > 0 || elements > 0)
                return new ItemInstance(
                    data,
                    attributes > 0,
                    elements > 0,
                    attributes, attributes,
                    elements, elements
                );

            return new ItemInstance(data, false, false);
        }
    }
}
