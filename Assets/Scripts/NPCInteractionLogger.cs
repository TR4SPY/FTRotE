using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Logging/Npc Interaction Logger")]
    public class NpcInteractionLogger : MonoBehaviour
    {
        [Tooltip("Opcjonalny opis NPC dla logów.")]
        public string npcName;

        private void Start()
        {
            if (string.IsNullOrEmpty(npcName))
            {
                npcName = gameObject.name; // Ustaw nazwę na nazwę obiektu, jeśli nie została określona
            }
        }

        /// <summary>
        /// Loguje interakcję z tym NPC.
        /// </summary>
        public void LogInteraction()
        {
            PlayerBehaviorLogger.Instance?.LogNpcInteraction(); // Logowanie w PBL
            Debug.Log($"Interacted with NPC: {npcName}");
        }
    }
}
