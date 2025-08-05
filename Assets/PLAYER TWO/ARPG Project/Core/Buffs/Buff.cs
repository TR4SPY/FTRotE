using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Buff", menuName = "PLAYER TWO/ARPG Project/Buff/Buff")]
    public class Buff : ScriptableObject
    {
        [Header("General Settings")]
        public Sprite icon;
        public float duration = 5f;
        public float cooldown = 0f;
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