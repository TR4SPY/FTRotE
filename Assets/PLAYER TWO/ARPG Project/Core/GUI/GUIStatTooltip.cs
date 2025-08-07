using System.Collections;
using System.Text;
using System.Linq;
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
            bool isPercentage = statName.Contains("Percent");

            string FormatValue(int value) => isPercentage ? $"{value}%" : value.ToString();
            var builder = new StringBuilder();
            builder.AppendLine($"Base: {FormatValue(baseValue)}");

            var increased = modifiers.Where(m => m.Value > 0);
            var decreased = modifiers.Where(m => m.Value < 0);

            if (increased.Any())
            {
                builder.AppendLine("Increased by:");
                foreach (var pair in increased)
                {
                    string value = FormatValue(pair.Value);
                    value = StringUtils.StringWithColor($"+{value}", GameColors.Green);
                    builder.AppendLine($" {pair.Key}: {value}");
                }
            }
            if (decreased.Any())
            {
                builder.AppendLine("Decreased by:");
                foreach (var pair in decreased)
                {
                    int absValue = Mathf.Abs(pair.Value);
                    string value = FormatValue(absValue);
                    value = StringUtils.StringWithColor($"-{value}", GameColors.LightRed);
                    builder.AppendLine($" {pair.Key}: {value}");
                }
            }

            Color totalColor = buffTotal > 0 ? GameColors.Green : buffTotal < 0 ? GameColors.LightRed : GameColors.White;
            string totalValue = StringUtils.StringWithColor(FormatValue(current), totalColor);
            builder.Append($"Total: {totalValue}");
            
            string displayName = StringUtils.ConvertToTitleCase(statName);
            GUITooltip.instance.ShowTooltip(displayName, builder.ToString(), gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GUITooltip.instance?.HideTooltip();
        }
    }
}
