using UnityEngine;
using System.Collections.Generic;

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
                    slot.keyText.text = Mathf.CeilToInt(instance.remainingTime).ToString();
                }
            }
        }

        protected virtual void HandleBuffAdded(BuffInstance instance)
        {
            if (slotPrefab == null) return;
            var slot = Instantiate(slotPrefab, transform);
            slot.SetBuff(instance);
            m_slots[instance] = slot;
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

                Destroy(slot.gameObject);
                m_slots.Remove(instance);
            }
        }
    }
}
