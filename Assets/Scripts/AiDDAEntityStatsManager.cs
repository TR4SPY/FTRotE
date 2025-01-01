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
    public class AIDDAEntityStatsManager : EntityStatsManager
    {
        protected override int CalculateMaxHealth()
        {
            int baseHealth = (int)((level * 10 + vitality * 2 + m_additionalAttributes.health) * m_additionalAttributes.healthMultiplier);
            return (int)(baseHealth * DifficultyManager.Instance.CurrentDexterityMultiplier);
        }

        protected override MinMax CalculateDamage()
        {
            var weaponDamage = GetItemsDamage();
            var baseDamage = new MinMax
            {
                min = (strength / 8) + weaponDamage.min + m_additionalAttributes.damage,
                max = (strength / 4) + weaponDamage.max + m_additionalAttributes.damage
            };

            baseDamage.min = (int)(baseDamage.min * DifficultyManager.Instance.CurrentStrengthMultiplier);
            baseDamage.max = (int)(baseDamage.max * DifficultyManager.Instance.CurrentStrengthMultiplier);

            return baseDamage;
        }

        protected override int CalculateAttackSpeed()
        {
            int baseSpeed = Mathf.Min((dexterity + GetItemsAttackSpeed()) / 10 + m_additionalAttributes.attackSpeed, Game.instance.maxAttackSpeed);
            return (int)(baseSpeed * DifficultyManager.Instance.CurrentSpeedMultiplier);
        }
    }
}