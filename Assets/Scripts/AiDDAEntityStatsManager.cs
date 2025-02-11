// - DODANO 29 GRUDNIA 2024 - 0001

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class AiDDAEntityStatsManager : EntityStatsManager
    {
        public static AiDDAEntityStatsManager Instance { get; private set; }
        private Dictionary<string, float> DifficultyMultipliers = new Dictionary<string, float>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected override void Start()
        {
            base.Start();

            // Inicjalizacja bazowych mnożników dla AI-DDA
            DifficultyMultipliers["Strength"] = 1.0f;
            DifficultyMultipliers["Dexterity"] = 1.0f;
            DifficultyMultipliers["Vitality"] = 1.0f;
            DifficultyMultipliers["Energy"] = 1.0f;
        }

        public void SetMultiplier(string key, float value)
        {
            if (DifficultyMultipliers.ContainsKey(key))
            {
                DifficultyMultipliers[key] = value;
            }
            else
            {
                DifficultyMultipliers.Add(key, value);
            }
        }

        public float GetMultiplier(string key)
        {
            return key switch
            {
                "Strength" => DifficultyManager.Instance?.CurrentStrengthMultiplier ?? 1.0f,
                "Dexterity" => DifficultyManager.Instance?.CurrentDexterityMultiplier ?? 1.0f,
                "Vitality" => DifficultyManager.Instance?.CurrentVitalityMultiplier ?? 1.0f,
                "Energy" => DifficultyManager.Instance?.CurrentEnergyMultiplier ?? 1.0f,
                _ => 1.0f
            };
        }

        public float GetAverageMultiplier()
        {
            return DifficultyMultipliers.Values.Average();
        }

        public void ApplyDDAAdjustments()
        {
            float strengthMultiplier = GetMultiplier("Strength");
            float dexterityMultiplier = GetMultiplier("Dexterity");
            float vitalityMultiplier = GetMultiplier("Vitality");
            float energyMultiplier = GetMultiplier("Energy");

            strength = Mathf.Max(1, (int)(strength * strengthMultiplier));
            dexterity = Mathf.Max(1, (int)(dexterity * dexterityMultiplier));
            vitality = Mathf.Max(1, (int)(vitality * vitalityMultiplier));
            energy = Mathf.Max(1, (int)(energy * energyMultiplier));

            Recalculate(); // Automatyczne przeliczenie reszty statystyk
        }
    }
}


    /*
        protected override int CalculateMaxHealth()
        {
            int baseHealth = (int)((level * 10 + vitality * 2 + m_additionalAttributes.health) * m_additionalAttributes.healthMultiplier);
            float dexterityMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Dexterity");
            return (int)(baseHealth * dexterityMultiplier);
        }

        protected override MinMax CalculateDamage()
        {
            var weaponDamage = GetItemsDamage();
            var baseDamage = new MinMax
            {
                min = (strength / 8) + weaponDamage.min + m_additionalAttributes.damage,
                max = (strength / 4) + weaponDamage.max + m_additionalAttributes.damage
            };

            float strengthMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Strength");
            baseDamage.min = (int)(baseDamage.min * strengthMultiplier);
            baseDamage.max = (int)(baseDamage.max * strengthMultiplier);

            return baseDamage;
        }

        protected override int CalculateAttackSpeed()
        {
            int baseSpeed = Mathf.Min((dexterity + GetItemsAttackSpeed()) / 10 + m_additionalAttributes.attackSpeed, Game.instance.maxAttackSpeed);
            float speedMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Speed");
            return (int)(baseSpeed * speedMultiplier);
        }

        protected override MinMax CalculateMagicDamage()
        {
            MinMax baseMagicDamage = base.CalculateMagicDamage();
            float energyMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Energy");

            baseMagicDamage.min = (int)(baseMagicDamage.min * energyMultiplier);
            baseMagicDamage.max = (int)(baseMagicDamage.max * energyMultiplier);

            return baseMagicDamage;
        }

        protected override int CalculateMaxMana()
        {
            int baseMana = base.CalculateMaxMana();
            float energyMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("MaxMana");
            return (int)(baseMana * energyMultiplier);
        }

        protected override float CalculateCriticalChance()
        {
            float baseCriticalChance = base.CalculateCriticalChance();
            float dexterityMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Dexterity");
            return baseCriticalChance * dexterityMultiplier;
        }

        protected override int CalculateDefense()
        {
            int baseDefense = base.CalculateDefense();
            float defenseMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Defense");
            return (int)(baseDefense * defenseMultiplier);
        }

        protected override float CalculateChanceToBlock()
        {
            float baseBlockChance = base.CalculateChanceToBlock();
            float dexterityMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Dexterity");
            return baseBlockChance * dexterityMultiplier;
        }

        protected override int CalculateBlockSpeed()
        {
            int baseBlockSpeed = base.CalculateBlockSpeed();
            float blockSpeedMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("BlockSpeed");
            return (int)(baseBlockSpeed * blockSpeedMultiplier);
        }

        protected override float CalculateStunChance()
        {
            float baseStunChance = base.CalculateStunChance();
            float strengthMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Strength");
            return baseStunChance * strengthMultiplier;
        }

        protected override int CalculateStunSpeed()
        {
            int baseStunSpeed = base.CalculateStunSpeed();
            float dexterityMultiplier = AiDDAEntityStatsManager.Instance.GetMultiplier("Dexterity");
            return (int)(baseStunSpeed * dexterityMultiplier);
        }
        */