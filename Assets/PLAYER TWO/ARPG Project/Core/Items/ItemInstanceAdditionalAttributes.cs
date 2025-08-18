using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public partial class ItemInstance
    {
        /// <summary>
        /// Returns the amount of additional attributes.
        /// </summary>
        public virtual int GetAttributesCount() => ContainAttributes() ? attributes.GetAttributesCount() : 0;

        /// <summary>
        /// Returns the additional damage points.
        /// </summary>
        public virtual int GetAdditionalDamage() => UseAttributes() ? Mathf.RoundToInt(attributes.damage * (1 + itemLevel * 0.05f) * effectiveness) : 0;

        /// <summary>
        /// Returns the additional attack speed points.
        /// </summary>
        public virtual int GetAttackSpeed() => UseAttributes() ? Mathf.RoundToInt(attributes.attackSpeed * GetItemLevelMultiplier() * effectiveness) : 0;

        /// <summary>
        /// Returns the additional defense points.
        /// </summary>
        public virtual int GetAdditionalDefense() => UseAttributes() ? Mathf.RoundToInt(attributes.defense * effectiveness) : 0;

        /// <summary>
        /// Returns the additional mana points.
        /// </summary>
        public virtual int GetAdditionalMana() => UseAttributes() ? Mathf.RoundToInt(attributes.mana * effectiveness) : 0;

        /// <summary>
        /// Returns the additional health points.
        /// </summary>
        public virtual int GetAdditionalHealth() => UseAttributes() ? Mathf.RoundToInt(attributes.health * effectiveness) : 0;

        /// <summary>
        /// Returns the additional damage multiplier.
        /// </summary>
        public virtual float GetDamageMultiplier() => UseAttributes() ? attributes.GetDamageMultiplier() * effectiveness : 0;

        /// <summary>
        /// Returns the additional critical chance multiplier.
        /// </summary>
        public virtual float GetCriticalChanceMultiplier() => UseAttributes() ? attributes.GetCriticalMultiplier() * effectiveness : 0;

        /// <summary>
        /// Returns the additional defense multiplier.
        /// </summary>
        public virtual float GetDefenseMultiplier() => UseAttributes() ? attributes.GetDefenseMultiplier() * effectiveness : 0;

        /// <summary>
        /// Returns the additional mana multiplier.
        /// </summary>
        public virtual float GetManaMultiplier() => UseAttributes() ? attributes.GetManaMultiplier() * effectiveness : 0;

        /// <summary>
        /// Returns the additional health multiplier.
        /// </summary>
        public virtual float GetHealthMultiplier() => UseAttributes() ? attributes.GetHealthMultiplier() * effectiveness : 0;
    }
}
