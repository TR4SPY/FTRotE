using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{       
    [System.Serializable]
    public struct ResistanceRequirement
    {
        public string statName;
        public int minimumValue;
    }

    [CreateAssetMenu(fileName = "New Buff", menuName = "PLAYER TWO/ARPG Project/Buff/Buff")]
    public class Buff : ScriptableObject
    {
        [Header("General Settings")]
        public Sprite icon;
        public float duration = 5f;
        public float cooldown = 0f;

        [Header("Restrictions")]
        [Tooltip("Set on which scenes/maps this buff can be used..")]
        public string[] allowedScenes;

        [Tooltip("Buffs that cannot be active at the same time as this buff.")]
        public Buff[] incompatibleBuffs;

        [Tooltip("Classes that are allowed to use this buff.")]
        public CharacterClassRestrictions allowedClasses = CharacterClassRestrictions.None;

        [Tooltip("Skips applying debuffs when the entity meets any configured resistance requirement.")]
        public ResistanceRequirement[] ignoreIfResistant;

        [Tooltip("If true, this buff will be removed on logout and won't be saved.")]
        public bool removeOnLogout;

        [Header("Debuff Immunity")]
        public Item[] immunityItems;
        public bool requireAllItems;

        public bool isDebuff;
        public ParticleSystem particlePrefab;

        [Header("Stat Modifiers")]
        public int strength;
        public int dexterity;
        public int vitality;
        public int energy;
        public int defense;
        public int magicResistance;
        public int fireResistance;
        public int waterResistance;
        public int iceResistance;
        public int earthResistance;
        public int airResistance;
        public int lightningResistance;
        public int shadowResistance;
        public int lightResistance;
        public int arcaneResistance;

        public int manaRegenPerSecond;
        public int manaRegenPer5Seconds;
        public int manaRegenPer30Seconds;
        public int healthRegenPerSecond;
        public int healthRegenPer5Seconds;
        public int healthRegenPer30Seconds;
        public int experiencePerSecondPercent;
        public int experiencePer5SecondsPercent;
        public int experiencePer30SecondsPercent;
        public int additionalMana;
        public int additionalManaPercent;
        public int additionalHealth;
        public int additionalHealthPercent;
        public int increaseAttackSpeedPercent;
        public int increaseAttackSpeedValue;
        public int increaseDamagePercent;
        public int increaseDamageValue;
        public int increaseMagicalDamagePercent;
        public int increaseMagicalDamageValue;

        public bool magicImmunity;
        public bool fireImmunity;
        public bool waterImmunity;
        public bool iceImmunity;
        public bool earthImmunity;
        public bool airImmunity;
        public bool lightningImmunity;
        public bool shadowImmunity;
        public bool lightImmunity;
        public bool arcaneImmunity;

        public int additionalAmberlingsPerMinute;
        public int additionalLunarisPerMinute;
        public int additionalSolmiresPerMinute;
        public int itemPricePercent;

        public static readonly IReadOnlyDictionary<string, string> StatDisplayNames =
            new Dictionary<string, string>
            {
                { nameof(strength), "Strength" },
                { nameof(dexterity), "Dexterity" },
                { nameof(vitality), "Vitality" },
                { nameof(energy), "Energy" },
                { nameof(defense), "Defense" },
                { nameof(magicResistance), "Magic Resistance" },
                { nameof(fireResistance), "Fire Resistance" },
                { nameof(waterResistance), "Water Resistance" },
                { nameof(iceResistance), "Ice Resistance" },
                { nameof(earthResistance), "Earth Resistance" },
                { nameof(airResistance), "Air Resistance" },
                { nameof(lightningResistance), "Lightning Resistance" },
                { nameof(shadowResistance), "Shadow Resistance" },
                { nameof(lightResistance), "Light Resistance" },
                { nameof(arcaneResistance), "Arcane Resistance" },
                { nameof(manaRegenPerSecond), "Mana Regen /s" },
                { nameof(manaRegenPer5Seconds), "Mana Regen /5s" },
                { nameof(manaRegenPer30Seconds), "Mana Regen /30s" },
                { nameof(healthRegenPerSecond), "Health Regen /s" },
                { nameof(healthRegenPer5Seconds), "Health Regen /5s" },
                { nameof(healthRegenPer30Seconds), "Health Regen /30s" },
                { nameof(experiencePerSecondPercent), "Experience /s %" },
                { nameof(experiencePer5SecondsPercent), "Experience /5s %" },
                { nameof(experiencePer30SecondsPercent), "Experience /30s %" },
                { nameof(additionalMana), "Additional Mana" },
                { nameof(additionalManaPercent), "Additional Mana %" },
                { nameof(additionalHealth), "Additional Health" },
                { nameof(additionalHealthPercent), "Additional Health %" },
                { nameof(increaseAttackSpeedPercent), "Attack Speed %" },
                { nameof(increaseAttackSpeedValue), "Attack Speed" },
                { nameof(increaseDamagePercent), "Damage %" },
                { nameof(increaseDamageValue), "Damage" },
                { nameof(increaseMagicalDamagePercent), "Magical Damage %" },
                { nameof(increaseMagicalDamageValue), "Magical Damage" },
                { nameof(magicImmunity), "Magic Immunity" },
                { nameof(fireImmunity), "Fire Immunity" },
                { nameof(waterImmunity), "Water Immunity" },
                { nameof(iceImmunity), "Ice Immunity" },
                { nameof(earthImmunity), "Earth Immunity" },
                { nameof(airImmunity), "Air Immunity" },
                { nameof(lightningImmunity), "Lightning Immunity" },
                { nameof(shadowImmunity), "Shadow Immunity" },
                { nameof(lightImmunity), "Light Immunity" },
                { nameof(arcaneImmunity), "Arcane Immunity" },
                { nameof(additionalAmberlingsPerMinute), "Amberlings /min" },
                { nameof(additionalLunarisPerMinute), "Lunaris /min" },
                { nameof(additionalSolmiresPerMinute), "Solmires /min" },
                { nameof(itemPricePercent), "Item Price %" },
            };
    }
}