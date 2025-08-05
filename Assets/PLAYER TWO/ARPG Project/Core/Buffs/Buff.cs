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
            };
    }
}