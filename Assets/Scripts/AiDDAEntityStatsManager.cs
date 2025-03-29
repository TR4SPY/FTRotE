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

            strength = Mathf.Max(1, Mathf.RoundToInt(baseStrength * strengthMultiplier));
            dexterity = Mathf.Max(1, Mathf.RoundToInt(baseDexterity * dexterityMultiplier));
            vitality = Mathf.Max(1, Mathf.RoundToInt(baseVitality * vitalityMultiplier));
            energy = Mathf.Max(1, Mathf.RoundToInt(baseEnergy * energyMultiplier));

            Recalculate();
        }
    }
}