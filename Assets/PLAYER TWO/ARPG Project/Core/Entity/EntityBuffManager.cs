using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

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

        public virtual bool AddBuff(Buff buff)
        {
            return AddBuff(buff, false);
        }

        public virtual bool AddBuff(Buff buff, bool isDebuff)
        {
            if (!buff || !m_stats)
                return false;

            var instance = m_buffs.Find(b => b.buff == buff && b.isDebuff == isDebuff);

            if (instance != null)
            {
                if (instance.isActive)
                {
                    instance.remainingTime = buff.duration;
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
            instance.remainingTime = instance.buff.duration;
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
