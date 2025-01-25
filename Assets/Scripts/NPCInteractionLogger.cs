using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Logging/Npc Interaction Logger")]
    public class NpcInteractionLogger : MonoBehaviour
    {
        [Tooltip("Opcjonalny opis NPC dla logów.")]
        public string npcName;
        protected Entity m_player => Level.instance.player;

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
        public void LogInteraction(Collider other)
        {
            // Sprawdź, czy interakcja pochodzi od gracza lub Agenta AI
            bool isPlayer = other.CompareTag(GameTags.Player);
            bool isAI = other.GetComponent<AgentController>()?.isAI == true;

            if (!isPlayer && !isAI)
            {
                Debug.LogWarning($"Interaction ignored. {other.name} is neither Player nor AI Agent.");
                return;
            }

            if (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Cannot log NPC interaction.");
                return;
            }

            // Jeśli interakcja pochodzi od gracza
            if (isPlayer)
            {
                var entity = other.GetComponent<Entity>();
                if (entity != null)
                {
                    PlayerBehaviorLogger.Instance.LogNpcInteraction(entity);
                    Debug.Log($"Player interacted with NPC: {npcName}");
                }
                else
                {
                    Debug.LogWarning("Player Entity is null. Cannot log NPC interaction for Player.");
                }
            }

            // Jeśli interakcja pochodzi od Agenta AI
            if (isAI)
            {
                var entity = other.GetComponent<Entity>();
                if (entity != null)
                {
                    PlayerBehaviorLogger.Instance.LogNpcInteraction(entity);
                    Debug.Log($"AI Agent interacted with NPC: {npcName}");
                }
                else
                {
                    Debug.LogWarning("AI Agent Entity is null. Cannot log NPC interaction for AI Agent.");
                }
            }
        }
    }
}
