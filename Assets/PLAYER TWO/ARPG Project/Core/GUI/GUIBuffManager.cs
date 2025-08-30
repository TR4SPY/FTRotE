using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Buff Manager")]
    public class GUIBuffManager : MonoBehaviour
    {
        [Tooltip("The prefab used to represent a buff slot.")]
        public GUIBuffSlot slotPrefab;

        protected EntityBuffManager m_buffManager;
        protected Dictionary<BuffInstance, GUIBuffSlot> m_slots = new();

        protected virtual void Start()
        {
            var player = Level.instance?.player;
            if (player)
            {
                m_buffManager = player.GetComponent<EntityBuffManager>();
                if (m_buffManager != null)
                {
                    m_buffManager.onBuffAdded.AddListener(HandleBuffAdded);
                    m_buffManager.onBuffRemoved.AddListener(HandleBuffRemoved);

                    foreach (var instance in m_buffManager.buffs)
                    {
                        if (instance.isActive)
                        {
                            HandleBuffAdded(instance);
                        }
                    }
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_buffManager != null)
            {
                m_buffManager.onBuffAdded.RemoveListener(HandleBuffAdded);
                m_buffManager.onBuffRemoved.RemoveListener(HandleBuffRemoved);
            }
        }

        protected virtual void Update()
        {
            foreach (var pair in m_slots)
            {
                var instance = pair.Key;
                var slot = pair.Value;

                if (instance.buff.duration > 0)
                {
                    slot.coolDownImage.fillAmount = instance.remainingTime / instance.buff.duration;
                }
                else
                {
                    slot.coolDownImage.fillAmount = 0f;
                }

                if (slot.keyText)
                {
                    if (float.IsInfinity(instance.remainingTime) || instance.buff.duration <= 0f)
                    {
                        slot.keyText.text = "\u221E";
                    }
                    else
                    {
                        slot.keyText.text = TooltipFormatter.FormatTime(instance.remainingTime);
                    }
                }
                
                if (instance.remainingTime <= 10f)
                {
                    slot.BeginExpiryFade(instance.remainingTime);
                }
                else
                {
                    slot.StopExpiryFade();
                }
            }

            SortSlots();
        }

        protected virtual void HandleBuffAdded(BuffInstance instance)
        {
            if (slotPrefab == null) return;
            var slot = Instantiate(slotPrefab, transform);
            slot.SetBuff(instance);
            if (m_buffManager != null)
            {
                int index = m_buffManager.buffs.IndexOf(instance);
                slot.transform.SetSiblingIndex(index);
            }
            m_slots[instance] = slot;
            SortSlots();
        }

        protected virtual void HandleBuffRemoved(BuffInstance instance)
        {
            if (m_slots.TryGetValue(instance, out var slot))
            {
                if (slot.keyText)
                {
                    slot.keyText.text = string.Empty;
                    slot.keyText.gameObject.SetActive(false);
                }

                slot.StopExpiryFade();

                Destroy(slot.gameObject);
                m_slots.Remove(instance);
            }
            
            SortSlots();
        }

        protected void SortSlots()
        {
            if (m_slots.Count == 0)
            {
                return;
            }

            var buffInstances = m_slots.Keys.ToList();
            var buffs = new List<BuffInstance>();
            var debuffs = new List<BuffInstance>();

            foreach (var inst in buffInstances)
            {
                if (inst.isDebuff)
                {
                    debuffs.Add(inst);
                }
                else
                {
                    buffs.Add(inst);
                }
            }

            buffs.Sort((a, b) => b.remainingTime.CompareTo(a.remainingTime));
            debuffs.Sort((a, b) => b.remainingTime.CompareTo(a.remainingTime));

            int index = 0;
            foreach (var inst in buffs)
            {
                if (m_slots.TryGetValue(inst, out var slot))
                {
                    slot.transform.SetSiblingIndex(index++);
                }
            }

            foreach (var inst in debuffs)
            {
                if (m_slots.TryGetValue(inst, out var slot))
                {
                    slot.transform.SetSiblingIndex(index++);
                }
            }
        }
    }
}
