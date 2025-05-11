using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public abstract class ItemWeapon : ItemEquippable
    {
        [Header("Weapon Settings")]
        [Tooltip("The base minimum damage of this Item.")]
        public int minDamage;

        [Tooltip("The base maximum damage of this Item.")]
        public int maxDamage;

        [Tooltip("The base minimum magic damage of this Item.")]
        public int minMagicDamage;

        [Tooltip("The base maximum magic damage of this Item.")]
        public int maxMagicDamage;

        [Tooltip("The base attack speed of this Item.")]
        public int attackSpeed;

        [Tooltip("Optional skill granted while this weapon is equipped.")]
        public Skill skill;

        [Tooltip("Optional skill requirement source (typically a skill book).")]
        public ItemSkill skillSource;

        [Tooltip("The list of audio clips this Item can play when used to perform attacks.")]
        public AudioClip[] attackClips;

        [Header("Combo Settings")]
        [Tooltip("The maximum number of combos this Weapon can perform.")]
        public int maxCombos = 3;

        [Tooltip("The time it takes to stop a combo it no new attack is performed.")]
        public float timeToStopCombo = 1f;

        [Tooltip("The time it takes to perform the next combo.")]
        public float nextComboDelay = 0.1f;

        /// <summary>
        /// Get a random damage based on the maximum and minimum base damage settings.
        /// </summary>
        public virtual int GetDamage() => Random.Range(minDamage, maxDamage);

        /// <summary>
        /// Returns a random magic damage value if the weapon has magic damage.
        /// </summary>
        public virtual MinMax GetMagicDamage()
        {
            return new MinMax(minMagicDamage, maxMagicDamage);
        }

        /// <summary>
        /// Returns true if the weapon has magic damage.
        /// </summary>
        public bool HasMagicDamage() => minMagicDamage > 0 || maxMagicDamage > 0;

        public override ItemAttributes CreateDefaultAttributes()
        {
            return new WeaponAttributes();
        }

    }
}
