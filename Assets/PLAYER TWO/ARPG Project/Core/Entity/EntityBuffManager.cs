using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Buff Manager")]
    public class EntityBuffManager : MonoBehaviour
    {
        public UnityEvent<BuffInstance> onBuffAdded;
        public UnityEvent<BuffInstance> onBuffRemoved;
        public List<BuffInstance> buffs => m_buffs;

        protected readonly List<BuffInstance> m_buffs = new();
        protected EntityStatsManager m_stats;

        protected virtual void Awake()
        {
            m_stats = GetComponent<EntityStatsManager>();
        }

        protected virtual void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public virtual bool AddBuff(Buff buff)
        {
            return AddBuff(buff, false);
        }

        public virtual bool AddBuff(Buff buff, bool isDebuff)
        {
            if (!buff || !m_stats)
                return false;

            bool isDebuffType = isDebuff || buff.isDebuff;
            if (isDebuffType && buff.immunityItems != null && buff.immunityItems.Length > 0)
            {
                var entity = GetComponent<Entity>();
                var inventory = entity != null ? entity.inventory : null;
                if (inventory != null)
                {
                    var itemsDict = inventory.instance.items;
                    bool hasImmunity = false;

                    if (buff.requireAllItems)
                    {
                        hasImmunity = true;
                        foreach (var required in buff.immunityItems)
                        {
                            bool found = false;
                            foreach (var pair in itemsDict)
                            {
                                if (pair.Key != null && pair.Key.data == required)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                hasImmunity = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var pair in itemsDict)
                        {
                            if (pair.Key == null)
                                continue;

                            foreach (var required in buff.immunityItems)
                            {
                                if (pair.Key.data == required)
                                {
                                    hasImmunity = true;
                                    break;
                                }
                            }

                            if (hasImmunity)
                                break;
                        }
                    }

                    if (hasImmunity)
                        return false;
                }
            }

            if (buff.incompatibleBuffs != null && buff.incompatibleBuffs.Length > 0)
            {
                foreach (var existing in m_buffs)
                {
                    if (!existing.isActive)
                        continue;

                    if (System.Array.IndexOf(buff.incompatibleBuffs, existing.buff) >= 0)
                        return false;
                }
            }
            
            if (buff.allowedScenes != null && buff.allowedScenes.Length > 0)
            {
                var sceneName = Level.instance?.currentScene.name;
                if (string.IsNullOrEmpty(sceneName) || System.Array.IndexOf(buff.allowedScenes, sceneName) < 0)
                    return false;
            }

            string className = gameObject.name.Replace("(Clone)", "").Trim();
            if (ClassHierarchy.NameToBits.TryGetValue(className, out var playerClass))
            {
                var allowed = buff.allowedClasses;
                if ((allowed & playerClass) == 0 && allowed != CharacterClassRestrictions.None)
                    return false;
            }

            if (isDebuff && buff.ignoreIfResistant != null)
            {
                foreach (var requirement in buff.ignoreIfResistant)
                {
                    if (string.IsNullOrEmpty(requirement.statName))
                        continue;

                    var prop = typeof(EntityStatsManager).GetProperty(requirement.statName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        int value = (int)prop.GetValue(m_stats);
                        if (value >= requirement.minimumValue)
                            return false;
                    }
                }
            }

            var instance = m_buffs.Find(b => b.buff == buff && b.isDebuff == isDebuff);

            if (instance != null)
            {
                if (instance.isActive)
                {
                    instance.remainingTime = buff.removeOnLogout ? float.PositiveInfinity : buff.duration;
                    return true;
                }
                else if (instance.remainingCooldown > 0f)
                {
                    return false;
                }
                else
                {
                    ActivateBuff(instance);
                    return true;
                }
            }

            instance = new BuffInstance(buff, isDebuff);
            if (isDebuffType)
            {
                m_buffs.Add(instance);
            }
            else
            {
                m_buffs.Insert(0, instance);
            }
            ApplyModifiers(buff, true);
            onBuffAdded?.Invoke(instance);
            return true;
        }

        public virtual void RestoreBuff(Buff buff, bool isDebuff, float remainingTime, float remainingCooldown, bool isActive)
        {
            if (!buff || !m_stats)
                return;

            var instance = new BuffInstance(buff, isDebuff)
            {
                remainingTime = remainingTime,
                remainingCooldown = remainingCooldown,
                isActive = isActive
            };

            m_buffs.Add(instance);

            if (isActive)
            {
                ApplyModifiers(buff, true);
                onBuffAdded?.Invoke(instance);
            }
        }

        public virtual void RemoveBuff(BuffInstance instance)
        {
            if (instance == null || !instance.isActive)
                return;

            instance.isActive = false;
            ApplyModifiers(instance.buff, false);
            instance.remainingCooldown = instance.buff.cooldown;
            onBuffRemoved?.Invoke(instance);
        }

        /// <summary>
        /// Returns the contribution of each active buff to the given stat.
        /// </summary>
        /// <param name="statName">The name of the stat in <see cref="Buff"/> and <see cref="EntityStatsManager"/>.</param>
        /// <returns>A dictionary mapping the buff name to the amount it modifies the stat.</returns>
        public virtual Dictionary<string, int> GetStatModifiers(string statName)
        {
            var result = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(statName))
                return result;

            if (statName == nameof(EntityStatsManager.attackSpeed))
            {
                int totalPercent = 0;
                int totalValue = 0;

                foreach (var  instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    totalPercent += instance.buff.increaseAttackSpeedPercent;
                    totalValue += instance.buff.increaseAttackSpeedValue;
                }

                int current = m_stats.attackSpeed;
                int baseValue = Mathf.RoundToInt((current - totalValue) / (1f + totalPercent / 100f));

                foreach (var instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    var b = instance.buff;
                    int value = Mathf.RoundToInt(baseValue * b.increaseAttackSpeedPercent / 100f) + b.increaseAttackSpeedValue;
                    if (value != 0)
                        result[b.name] = value;
                }

                return result;
            }

            if (statName == nameof(EntityStatsManager.minDamage) || statName == nameof(EntityStatsManager.maxDamage))
            {
                int totalPercent = 0;
                int totalValue = 0;

                foreach (var instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    totalPercent += instance.buff.increaseDamagePercent;
                    totalValue += instance.buff.increaseDamageValue;
                }

                int current = statName == nameof(EntityStatsManager.minDamage) ? m_stats.minDamage : m_stats.maxDamage;
                int baseValue = Mathf.RoundToInt((current - totalValue) / (1f + totalPercent / 100f));

                foreach (var instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    var b = instance.buff;
                    int value = Mathf.RoundToInt(baseValue * b.increaseDamagePercent / 100f) + b.increaseDamageValue;
                    if (value != 0)
                        result[b.name] = value;
                }

                return result;
            }

            if (statName == nameof(EntityStatsManager.minMagicDamage) || statName == nameof(EntityStatsManager.maxMagicDamage))
            {
                int totalPercent = 0;
                int totalValue = 0;

                foreach (var instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    totalPercent += instance.buff.increaseMagicalDamagePercent;
                    totalValue += instance.buff.increaseMagicalDamageValue;
                }

                int current = statName == nameof(EntityStatsManager.minMagicDamage) ? m_stats.minMagicDamage : m_stats.maxMagicDamage;
                int baseValue = Mathf.RoundToInt((current - totalValue) / (1f + totalPercent / 100f));

                foreach (var instance in m_buffs)
                {
                    if (!instance.isActive || instance.buff == null)
                        continue;

                    var b = instance.buff;
                    int value = Mathf.RoundToInt(baseValue * b.increaseMagicalDamagePercent / 100f) + b.increaseMagicalDamageValue;
                    if (value != 0)
                        result[b.name] = value;
                }

                return result;
            }

            var field = typeof(Buff).GetField(statName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return result;

            foreach (var instance in m_buffs)
            {
                if (!instance.isActive || instance.buff == null)
                    continue;

                int value = (int)field.GetValue(instance.buff);
                if (value != 0)
                    result[instance.buff.name] = value;
            }

            return result;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            for (int i = m_buffs.Count - 1; i >= 0; i--)
            {
                var instance = m_buffs[i];
                var allowed = instance.buff.allowedScenes;
                if (allowed != null && allowed.Length > 0 && System.Array.IndexOf(allowed, scene.name) < 0)
                {
                    RemoveBuff(instance);
                    m_buffs.RemoveAt(i);
                }
            }
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;
            for (int i = m_buffs.Count - 1; i >= 0; i--)
            {
                var instance = m_buffs[i];
                if (instance.isActive)
                {
                    instance.remainingTime -= dt;
                    if (instance.remainingTime <= 0f)
                        RemoveBuff(instance);
                }
                else
                {
                    instance.remainingCooldown -= dt;
                    if (instance.remainingCooldown <= 0f)
                        m_buffs.RemoveAt(i);
                }
            }
        }

        protected virtual void ActivateBuff(BuffInstance instance)
        {
            instance.isActive = true;
            instance.remainingTime = instance.buff.removeOnLogout ? float.PositiveInfinity : instance.buff.duration;
            ApplyModifiers(instance.buff, true);
            onBuffAdded?.Invoke(instance);
        }

        protected virtual void ApplyModifiers(Buff buff, bool add)
        {
            int mult = add ? 1 : -1;

            int previousMaxHealth = m_stats.maxHealth;
            int previousMaxMana = m_stats.maxMana;
            int previousHealth = m_stats.health;
            int previousMana = m_stats.mana;

            m_stats.strength += buff.strength * mult;
            m_stats.dexterity += buff.dexterity * mult;
            m_stats.vitality += buff.vitality * mult;
            m_stats.energy += buff.energy * mult;

            m_stats.Recalculate();
            ApplySecondaryModifiers(previousMaxHealth, previousMaxMana, previousHealth, previousMana);
            }

        protected virtual void ApplySecondaryModifiers(int previousMaxHealth, int previousMaxMana, int previousHealth, int previousMana)
        {
            int defense = 0;
            int magicResistance = 0;
            int fireResistance = 0;
            int waterResistance = 0;
            int iceResistance = 0;
            int earthResistance = 0;
            int airResistance = 0;
            int lightningResistance = 0;
            int shadowResistance = 0;
            int lightResistance = 0;
            int arcaneResistance = 0;

            int baseMaxMana = m_stats.maxMana;
            int baseMaxHealth = m_stats.maxHealth;
            int additionalMana = 0;
            int additionalManaPercent = 0;
            int additionalHealth = 0;
            int additionalHealthPercent = 0;
            int attackSpeedPercent = 0;
            int attackSpeedValue = 0;
            int damagePercent = 0;
            int damageValue = 0;
            int magicDamagePercent = 0;
            int magicDamageValue = 0;
            int experienceRewardPercent = 0;
            int moneyRewardPercent = 0;
            int manaRegenPerSecond = 0;
            int manaRegenPer5Seconds = 0;
            int manaRegenPer30Seconds = 0;
            int healthRegenPerSecond = 0;
            int healthRegenPer5Seconds = 0;
            int healthRegenPer30Seconds = 0;
            int experiencePerSecondPercent = 0;
            int experiencePer5SecondsPercent = 0;
            int experiencePer30SecondsPercent = 0;
            bool magicImmunity = false;
            bool fireImmunity = false;
            bool waterImmunity = false;
            bool iceImmunity = false;
            bool earthImmunity = false;
            bool airImmunity = false;
            bool lightningImmunity = false;
            bool shadowImmunity = false;
            bool lightImmunity = false;
            bool arcaneImmunity = false;
            int amberlingsPerMinute = 0;
            int lunarisPerMinute = 0;
            int solmiresPerMinute = 0;
            int itemPricePercent = 0;

            foreach (var instance in m_buffs)
            {
                if (!instance.isActive)
                    continue;

                var b = instance.buff;
                defense += b.defense;
                magicResistance += b.magicResistance;
                fireResistance += b.fireResistance;
                waterResistance += b.waterResistance;
                iceResistance += b.iceResistance;
                earthResistance += b.earthResistance;
                airResistance += b.airResistance;
                lightningResistance += b.lightningResistance;
                shadowResistance += b.shadowResistance;
                lightResistance += b.lightResistance;
                arcaneResistance += b.arcaneResistance;

                additionalMana += b.additionalMana;
                additionalManaPercent += b.additionalManaPercent;
                additionalHealth += b.additionalHealth;
                additionalHealthPercent += b.additionalHealthPercent;
                attackSpeedPercent += b.increaseAttackSpeedPercent;
                attackSpeedValue += b.increaseAttackSpeedValue;
                damagePercent += b.increaseDamagePercent;
                damageValue += b.increaseDamageValue;
                magicDamagePercent += b.increaseMagicalDamagePercent;
                magicDamageValue += b.increaseMagicalDamageValue;
                experienceRewardPercent += b.additionalExperienceRewardPercent;
                moneyRewardPercent += b.additionalMoneyRewardPercent;
                manaRegenPerSecond += b.manaRegenPerSecond;
                manaRegenPer5Seconds += b.manaRegenPer5Seconds;
                manaRegenPer30Seconds += b.manaRegenPer30Seconds;
                healthRegenPerSecond += b.healthRegenPerSecond;
                healthRegenPer5Seconds += b.healthRegenPer5Seconds;
                healthRegenPer30Seconds += b.healthRegenPer30Seconds;
                experiencePerSecondPercent += b.experiencePerSecondPercent;
                experiencePer5SecondsPercent += b.experiencePer5SecondsPercent;
                experiencePer30SecondsPercent += b.experiencePer30SecondsPercent;
                magicImmunity |= b.magicImmunity;
                fireImmunity |= b.fireImmunity;
                waterImmunity |= b.waterImmunity;
                iceImmunity |= b.iceImmunity;
                earthImmunity |= b.earthImmunity;
                airImmunity |= b.airImmunity;
                lightningImmunity |= b.lightningImmunity;
                shadowImmunity |= b.shadowImmunity;
                lightImmunity |= b.lightImmunity;
                arcaneImmunity |= b.arcaneImmunity;
                amberlingsPerMinute += b.additionalAmberlingsPerMinute;
                lunarisPerMinute += b.additionalLunarisPerMinute;
                solmiresPerMinute += b.additionalSolmiresPerMinute;
                itemPricePercent += b.itemPricePercent;
            }

            ModifyStatProperty("defense", defense);
            if (magicImmunity) SetStatProperty("magicResistance", int.MaxValue); else ModifyStatProperty("magicResistance", magicResistance);
            if (fireImmunity) SetStatProperty("fireResistance", int.MaxValue); else ModifyStatProperty("fireResistance", fireResistance);
            if (waterImmunity) SetStatProperty("waterResistance", int.MaxValue); else ModifyStatProperty("waterResistance", waterResistance);
            if (iceImmunity) SetStatProperty("iceResistance", int.MaxValue); else ModifyStatProperty("iceResistance", iceResistance);
            if (earthImmunity) SetStatProperty("earthResistance", int.MaxValue); else ModifyStatProperty("earthResistance", earthResistance);
            if (airImmunity) SetStatProperty("airResistance", int.MaxValue); else ModifyStatProperty("airResistance", airResistance);
            if (lightningImmunity) SetStatProperty("lightningResistance", int.MaxValue); else ModifyStatProperty("lightningResistance", lightningResistance);
            if (shadowImmunity) SetStatProperty("shadowResistance", int.MaxValue); else ModifyStatProperty("shadowResistance", shadowResistance);
            if (lightImmunity) SetStatProperty("lightResistance", int.MaxValue); else ModifyStatProperty("lightResistance", lightResistance);
            if (arcaneImmunity) SetStatProperty("arcaneResistance", int.MaxValue); else ModifyStatProperty("arcaneResistance", arcaneResistance);

            int extraMana = Mathf.RoundToInt(baseMaxMana * additionalManaPercent / 100f);
            int extraHealth = Mathf.RoundToInt(baseMaxHealth * additionalHealthPercent / 100f);
            SetStatProperty("maxMana", baseMaxMana + additionalMana + extraMana);
            SetStatProperty("maxHealth", baseMaxHealth + additionalHealth + extraHealth);
            int maxHealthDelta = Mathf.Max(0, m_stats.maxHealth - previousMaxHealth);
            int maxManaDelta = Mathf.Max(0, m_stats.maxMana - previousMaxMana);
            m_stats.health = Mathf.Min(previousHealth + maxHealthDelta, m_stats.maxHealth);
            m_stats.mana = Mathf.Min(previousMana + maxManaDelta, m_stats.maxMana);

            int atkSpeed = m_stats.attackSpeed;
            atkSpeed = Mathf.RoundToInt(atkSpeed * (1f + attackSpeedPercent / 100f)) + attackSpeedValue;
            SetStatProperty("attackSpeed", atkSpeed);

            int minDmg = m_stats.minDamage;
            int maxDmg = m_stats.maxDamage;
            float dmgMul = 1f + damagePercent / 100f;
            minDmg = Mathf.RoundToInt(minDmg * dmgMul) + damageValue;
            maxDmg = Mathf.RoundToInt(maxDmg * dmgMul) + damageValue;
            SetStatProperty("minDamage", minDmg);
            SetStatProperty("maxDamage", maxDmg);

            int minMagic = m_stats.minMagicDamage;
            int maxMagic = m_stats.maxMagicDamage;
            float magicMul = 1f + magicDamagePercent / 100f;
            minMagic = Mathf.RoundToInt(minMagic * magicMul) + magicDamageValue;
            maxMagic = Mathf.RoundToInt(maxMagic * magicMul) + magicDamageValue;
            SetStatProperty("minMagicDamage", minMagic);
            SetStatProperty("maxMagicDamage", maxMagic);

            SetStatProperty("manaRegenPerSecond", manaRegenPerSecond);
            SetStatProperty("manaRegenPer5Seconds", manaRegenPer5Seconds);
            SetStatProperty("manaRegenPer30Seconds", manaRegenPer30Seconds);
            SetStatProperty("healthRegenPerSecond", healthRegenPerSecond);
            SetStatProperty("healthRegenPer5Seconds", healthRegenPer5Seconds);
            SetStatProperty("healthRegenPer30Seconds", healthRegenPer30Seconds);
            SetStatProperty("experiencePerSecondPercent", experiencePerSecondPercent);
            SetStatProperty("experiencePer5SecondsPercent", experiencePer5SecondsPercent);
            SetStatProperty("experiencePer30SecondsPercent", experiencePer30SecondsPercent);
            SetStatProperty("additionalAmberlingsPerMinute", amberlingsPerMinute);
            SetStatProperty("additionalLunarisPerMinute", lunarisPerMinute);
            SetStatProperty("additionalSolmiresPerMinute", solmiresPerMinute);
            SetStatProperty("itemPricePercent", itemPricePercent);
            SetStatProperty("additionalExperienceRewardPercent", experienceRewardPercent);
            SetStatProperty("additionalMoneyRewardPercent", moneyRewardPercent);
        }
        
        protected virtual void SetStatProperty(string name, int value)
        {
            var prop = typeof(EntityStatsManager).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                prop.SetValue(m_stats, value);
            }
        }

        protected virtual void ModifyStatProperty(string name, int amount)
        {
            var prop = typeof(EntityStatsManager).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                int current = (int)prop.GetValue(m_stats);
                prop.SetValue(m_stats, current + amount);
            }
        }
    }
}
