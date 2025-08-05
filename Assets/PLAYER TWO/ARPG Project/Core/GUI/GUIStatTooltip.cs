using System.Collections;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stat Tooltip")]
    public class GUIStatTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Name of the stat to display when hovering.")]
        public string statName;

        private EntityStatsManager m_stats;
        private EntityBuffManager m_buffs;

        private bool m_initializationFailed;
        private bool m_waiting;

        protected virtual void Start()
        {
            InitializeReferences();
        }

        protected virtual void InitializeReferences()
        {
            if (m_initializationFailed || m_stats != null && m_buffs != null || m_waiting)
                return;
            
            StartCoroutine(WaitForPlayer());
        }

        private IEnumerator WaitForPlayer()
        {
            m_waiting = true;
            float elapsed = 0f;
            const float timeout = 5f;
            while (Level.instance?.player == null && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            m_waiting = false;

            if (Level.instance?.player == null)
            {
                Debug.LogError("GUIStatTooltip could not find player after 5 seconds.", this);
                m_initializationFailed = true;
                yield break;
            }

            m_stats = Level.instance.player.stats;
            m_buffs = Level.instance.player.GetComponent<EntityBuffManager>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GUITooltip.instance == null || m_initializationFailed)
                return;

            if (m_stats == null || m_buffs == null)
            {
                InitializeReferences();
                return;
            }

            if (string.IsNullOrEmpty(statName))
                return;

            var modifiers = m_buffs.GetStatModifiers(statName);
            int buffTotal = 0;
            foreach (var value in modifiers.Values)
                buffTotal += value;

            var prop = typeof(EntityStatsManager).GetProperty(statName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
                return;

            int current = (int)prop.GetValue(m_stats);
            int baseValue = current - buffTotal;

            var builder = new StringBuilder();
            builder.AppendLine($"Base: {baseValue}");
            foreach (var pair in modifiers)
            {
                string sign = pair.Value >= 0 ? "+" : "";
                builder.AppendLine($"{pair.Key}: {sign}{pair.Value}");
            }
            builder.Append($"Total: {current}");

            string displayName = StringUtils.ConvertToTitleCase(statName);
            GUITooltip.instance.ShowTooltip(displayName, builder.ToString(), gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GUITooltip.instance?.HideTooltip();
        }
    }
}
