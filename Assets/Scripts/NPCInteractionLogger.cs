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
                npcName = gameObject.name;
            }
        }

        public void LogInteraction(Collider other)
        {
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

            var faction = GetComponent<FactionMember>()?.faction ?? Faction.None;

            if (isPlayer)
            {
                var entity = other.GetComponent<Entity>();
                if (entity != null)
                {
                    PlayerBehaviorLogger.Instance.LogNpcInteraction(entity, faction);
                    Debug.Log($"Player interacted with NPC: {npcName}");
                }
                else
                {
                    Debug.LogWarning("Player Entity is null. Cannot log NPC interaction for Player.");
                }
            }

            if (isAI)
            {
                var entity = other.GetComponent<Entity>();
                if (entity != null)
                {
                    PlayerBehaviorLogger.Instance.LogNpcInteraction(entity, faction);
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
