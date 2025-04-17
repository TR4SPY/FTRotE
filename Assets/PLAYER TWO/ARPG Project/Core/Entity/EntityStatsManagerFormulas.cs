using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public partial class EntityStatsManager
    {
        /// <summary>
        /// Calculates the minimum and maximum damage of the entity.
        /// </summary>
        protected virtual MinMax CalculateDamage()
        {
            var weaponDamage = GetItemsDamage();

            return new MinMax
            {
                min = Mathf.RoundToInt((strength / 4) + weaponDamage.min + m_additionalAttributes.damage),
                max = Mathf.RoundToInt((strength / 4) + weaponDamage.max + m_additionalAttributes.damage)
            };
        }

        /// <summary>
        /// Calculates the minimum and maximum magic damage of the entity.
        /// </summary>
        protected virtual MinMax CalculateMagicDamage()
        {
            var weaponMagicDamage = GetMagicDamage();

            return new MinMax
            {
                min = Mathf.RoundToInt((energy / 9) + weaponMagicDamage.min + m_additionalAttributes.magicDamage),
                max = Mathf.RoundToInt((energy / 9) + weaponMagicDamage.max + m_additionalAttributes.magicDamage)
            };
        }

        protected virtual int CalculateMagicResistance()
        {
            return (int)((level * 2) + GetMagicResistance() + m_additionalAttributes.magicResistance);
        }

        /// <summary>
        /// Calculates the amount of experience points needed to reach the next level.
        /// </summary>
        protected virtual int CalculateNextLevelExperience()
        {
            if (level == 1)
                return Game.instance.baseExperience;

            if (level == 2)
                return Game.instance.baseExperience + Mathf.RoundToInt(Game.instance.expMultiplier);

            return Mathf.RoundToInt(Game.instance.baseExperience + (level - 1) * Game.instance.expMultiplier + 
                                    (100 * level + level * 10 * (level - 1)) * level * Game.instance.expMultiplier);
        }
        
        public int CalculateEnemyExperience(Entity enemy)
        {
            return Mathf.RoundToInt(enemy.stats.level * Game.instance.baseEnemyDefeatExperience * Game.instance.expMultiplier);
        }

        /// <summary>
        /// Calculates the max health points of the entity.
        /// </summary>
        protected virtual int CalculateMaxHealth()
        {
            return Mathf.RoundToInt(((vitality * 5) + (level * 10) + m_additionalAttributes.health) * m_additionalAttributes.healthMultiplier);
        }

        /// <summary>
        /// Calculates the max mana points of the entity.
        /// </summary>
        protected virtual int CalculateMaxMana()
        {
            return Mathf.RoundToInt(((energy * 4) + (level * 5) + m_additionalAttributes.mana) * m_additionalAttributes.manaMultiplier);
        }

        /// <summary>
        /// Calculates the attack speed which is used by the entity attack animations.
        /// </summary>
        protected virtual int CalculateAttackSpeed()
        {
            return Mathf.Min((dexterity + GetItemsAttackSpeed()) / 10 + m_additionalAttributes.attackSpeed, Game.instance.maxAttackSpeed);
        }

        /// <summary>
        /// Calculates the percentage chance of performing a critical attack.
        /// </summary>
        protected virtual float CalculateCriticalChance()
        {
            return (dexterity / 10 + 20) / 100f * m_additionalAttributes.criticalChanceMultiplier;
        }

        /// <summary>
        /// Calculates the defense points of the entity.
        /// </summary>
        protected virtual int CalculateDefense()
        {
            return Mathf.RoundToInt(((dexterity / 3) + GetItemsDefense() + m_additionalAttributes.defense) * m_additionalAttributes.defenseMultiplier);
        }

        /// <summary>
        /// Calculates the chance of blocking attacks.
        /// </summary>
        protected virtual float CalculateChanceToBlock()
        {
            return Mathf.Min((dexterity / 20 + 5 + level) / 100f * GetItemsChanceToBlock(), Game.instance.maxBlockChance);
        }

        /// <summary>
        /// Calculates the block recover speed which will be used by the entity block animations.
        /// </summary>
        protected virtual int CalculateBlockSpeed()
        {
            return Mathf.Min(dexterity / 5 + 100 + level * 10, Game.instance.maxBlockSpeed);
        }

        /// <summary>
        /// Calculates the chance of stunning the enemy after a successful attack.
        /// </summary>
        protected virtual float CalculateStunChance()
        {
            return Mathf.Min((strength / 10 + level) / 100f, Game.instance.maxStunChance);
        }

        /// <summary>
        /// Calculates the stun recover speed which will be used by the entity stun animations.
        /// </summary>
        /// <returns></returns>
        protected virtual int CalculateStunSpeed()
        {
            return Mathf.Min(dexterity / 2 + 100 + level * 20, Game.instance.maxStunSpeed);
        }
    }
}
