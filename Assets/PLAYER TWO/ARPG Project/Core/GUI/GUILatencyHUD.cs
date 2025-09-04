using UnityEngine;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    public class GUILatencyHUD : MonoBehaviour
    {
        public TMP_Text latencyText;

        void Update()
        {
            bool show = GameSettings.instance != null && GameSettings.instance.GetShowLatency();
            if (latencyText != null)
            {
                latencyText.gameObject.SetActive(show);
            }

            if (!show || latencyText == null)
                return;

            int ping = GetSimulatedPing();
            latencyText.text = $"Latency: {ping} ms";
        }

        private int GetSimulatedPing()
        {
            // Placeholder ping calculation. Replace with real network ping in production.
            return Mathf.RoundToInt(Time.deltaTime * 1000f);
        }
    }
}