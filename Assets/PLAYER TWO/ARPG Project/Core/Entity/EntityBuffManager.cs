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
            m_buffs.Add(instance);
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

            m_stats.strength += buff.strength * mult;
            m_stats.dexterity += buff.dexterity * mult;
            m_stats.vitality += buff.vitality * mult;
            m_stats.energy += buff.energy * mult;

            m_stats.Recalculate();
            ApplySecondaryModifiers();
        }

        protected virtual void ApplySecondaryModifiers()
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
            }

            ModifyStatProperty("defense", defense);
            ModifyStatProperty("magicResistance", magicResistance);
            ModifyStatProperty("fireResistance", fireResistance);
            ModifyStatProperty("waterResistance", waterResistance);
            ModifyStatProperty("iceResistance", iceResistance);
            ModifyStatProperty("earthResistance", earthResistance);
            ModifyStatProperty("airResistance", airResistance);
            ModifyStatProperty("lightningResistance", lightningResistance);
            ModifyStatProperty("shadowResistance", shadowResistance);
            ModifyStatProperty("lightResistance", lightResistance);
            ModifyStatProperty("arcaneResistance", arcaneResistance);
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
