using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public partial class ItemInstance
    {
        /// <summary>
        /// Indicates whether this item is sealed due to unmet requirements.
        /// </summary>
        public bool isSealed { get; private set; }

        /// <summary>
        /// Represents the current effectiveness of this item (0-1 range).
        /// </summary>
        public float effectiveness { get; private set; } = 1f;

        /// <summary>
        /// Indicates the current seal type applied to the item.
        /// </summary>
        public ItemSealType ItemSealType { get; private set; } = ItemSealType.None;

        /// <summary>
        /// Manually sets the seal state of this item.
        /// </summary>
        public void SetSealState(ItemSealType sealType, float effectiveness)
        {
            ItemSealType = sealType;
            this.effectiveness = effectiveness;
            isSealed = sealType != ItemSealType.None || effectiveness < 1f;
        }

        /// <summary>
        /// Evaluates item requirements against an entity and updates seal state and effectiveness.
        /// </summary>
        public void EvaluateRequirements(Entity entity)
        {
            // bool meets = MeetsRequirements(entity);

            bool classAllowed = true;
            if (entity)
            {
                string className = entity.name.Replace("(Clone)", "").Trim();
                if (ClassHierarchy.NameToBits.TryGetValue(className, out var playerClass))
                    classAllowed = IsClassAllowed(playerClass);
            }

            if (!classAllowed)
            {
                SetSealState(ItemSealType.Incompatible, 0f);
                return;
            }

            float effectiveness = 1f;

            int requiredStrength = GetRequiredStrength();
            if (requiredStrength > 0)
                effectiveness = Mathf.Min(effectiveness, entity.stats.strength / (float)requiredStrength);

            int requiredDexterity = GetRequiredDexterity();
            if (requiredDexterity > 0)
                effectiveness = Mathf.Min(effectiveness, entity.stats.dexterity / (float)requiredDexterity);

            int requiredLevel = GetRequiredLevel();
            if (requiredLevel > 0)
                effectiveness = Mathf.Min(effectiveness, entity.stats.level / (float)requiredLevel);

            effectiveness = Mathf.Clamp01(effectiveness);

            if (effectiveness < 1f)
            {
                SetSealState(ItemSealType.Restricted, effectiveness);
            }
            else
            {
                SetSealState(ItemSealType.None, 1f);
            }
        }

        public bool IsSealed() => isSealed;
    }
}