using UnityEngine;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Zone Trigger")]
    public class ZoneTrigger : MonoBehaviour
    {
        public string zoneStatus = "New Area Discovered";

        [Tooltip("The name of the zone.")]
        public string zoneName;

        [Tooltip("The description of the zone.")]
        public string zoneDescription;

        private GUIZoneHUD guiZoneHUD;
        protected Collider m_collider;

        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }

        private void Start()
        {
            // Inicjalizacja Collidera
            InitializeCollider();

#if UNITY_2023_1_OR_NEWER
            guiZoneHUD = Object.FindFirstObjectByType<GUIZoneHUD>();
#else
            guiZoneHUD = Object.FindObjectOfType<GUIZoneHUD>();
#endif

            if (guiZoneHUD == null)
            {
                Debug.LogError("GUIZoneHUD not found in the scene.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isPlayer = other.CompareTag(GameTags.Player);
            bool isAI = other.GetComponent<AgentController>()?.isAI == true;

            if (!isPlayer && !isAI) return; // Ignoruj, jeśli to nie gracz ani AI Agent

            if (other.CompareTag("Entity/AI_Agent"))
            {
                var agentController = other.GetComponent<AgentController>();
                if (agentController != null)
                {
                    if (agentController.HasDiscoveredZone(zoneName))
                    {
                        Debug.Log($"[ZoneTrigger] AI Agent already discovered '{zoneName}', ignoring duplicate.");
                        return;
                    }

                    // Sprawdzenie, czy agent rzeczywiście się ruszał w stronę tej strefy
                    if (agentController.GetTargetName() == zoneName)
                    {
                        Debug.Log($"[ZoneTrigger] AI Agent reached and confirmed discovery of '{zoneName}'.");
                        agentController.DiscoverZone(zoneName, true); // Ostateczna weryfikacja odkrycia
                    }
                }
            }

            if (string.IsNullOrEmpty(zoneName))
            {
                Debug.LogError("Zone name is null or empty. Cannot log zone discovery.");
                return;
            }

            // Jeśli obiekt to gracz
            if (isPlayer)
            {
                var currentCharacter = Game.instance.currentCharacter;

                if (currentCharacter == null)
                {
                    Debug.LogError("Current character is null. Cannot log zone discovery.");
                    return;
                }

                if (currentCharacter.visitedZones == null)
                {
                    Debug.LogError("Visited zones list is null. Initializing it.");
                    currentCharacter.visitedZones = new HashSet<string>();
                }

                if (currentCharacter.visitedZones.Contains(zoneName))
                {
                    Debug.Log($"Zone '{zoneName}' has already been visited by the player.");
                    return;
                }

                // Dodaj strefę do odwiedzonych
                currentCharacter.visitedZones.Add(zoneName);

                // Zapis stanu gry
                GameSave.instance.Save();

                // Logowanie odkrycia strefy
                var entity = other.GetComponent<Entity>();
                if (entity != null)
                {
                    PlayerBehaviorLogger.Instance?.LogAreaDiscovered(entity, zoneName);
                }
                //PlayerBehaviorLogger.Instance?.LogAreaDiscovered(zoneName);

                // Wyświetlenie informacji o strefie w GUI
                guiZoneHUD?.ShowZone(zoneStatus, zoneName, zoneDescription);

                Debug.Log($"Zone '{zoneName}' discovered and saved for character '{currentCharacter.name}'.");
            }

            // Jeśli obiekt to AI Agent
            if (isAI)
            {
                Debug.Log($"AI Agent discovered zone '{zoneName}'.");
                PlayerBehaviorLogger.Instance?.LogAgentZoneDiscovery(zoneName); // Logowanie dla AI
            }
        }
    }
}
