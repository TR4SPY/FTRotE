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

        [Header("Zone Discovery Rewards")]
        [Header("Achiever Rewards")]
        [Tooltip("EXP reward for Achiever")]
        public int achieverExpReward = 100;
        [Tooltip("Gold reward for Achiever")]
        public int achieverGoldReward = 100;

        [Header("Killer Rewards")]
        [Tooltip("EXP reward for Killer")]
        public int killerExpReward = 100;
        [Tooltip("Gold reward for Killer")]
        public int killerGoldReward = 100;

        [Header("Socializer Rewards")]
        [Tooltip("EXP reward for Socializer")]
        public int socializerExpReward = 100;
        [Tooltip("Gold reward for Socializer")]
        public int socializerGoldReward = 100;

        [Header("Explorer Rewards")]
        [Tooltip("EXP reward for Explorer")]
        public int explorerExpReward = 500;
        [Tooltip("Gold reward for Explorer")]
        public int explorerGoldReward = 500;

        private GUIZoneHUD guiZoneHUD;
        protected Collider m_collider;

        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }

        private void Start()
        {
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

        private void GiveZoneReward(Entity player)
        {
            if (player == null || player.stats == null || player.inventory == null)
                return;

            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;
            int finalExp = 0;
            int finalGold = 0;

            switch (playerType)
            {
                case "Achiever":
                    finalExp = achieverExpReward;
                    finalGold = achieverGoldReward;
                    break;
                case "Killer":
                    finalExp = killerExpReward;
                    finalGold = killerGoldReward;
                    break;
                case "Socializer":
                    finalExp = socializerExpReward;
                    finalGold = socializerGoldReward;
                    break;
                case "Explorer":
                    finalExp = explorerExpReward;
                    finalGold = explorerGoldReward;
                    break;
            }

            player.stats.AddExperience(finalExp);
            player.inventory.instance.money += finalGold;

            Debug.Log($"[Zone] {player.name} discovered zone '{zoneName}' and received {finalExp} EXP, {finalGold} Gold.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            bool isPlayer = other.CompareTag(GameTags.Player);
            bool isAI = other.GetComponent<AgentController>()?.isAI == true;

            if (!isPlayer && !isAI)
                return;

            if (string.IsNullOrEmpty(zoneName))
            {
                Debug.LogError("[ZoneTrigger] Zone name is null or empty. Cannot log zone discovery.");
                return;
            }

            if (isAI)
            {
                var agentController = other.GetComponent<AgentController>();
                if (agentController != null)
                {
                    if (agentController.HasDiscoveredZone(zoneName))
                    {
                        Debug.Log($"[ZoneTrigger] AI Agent already discovered '{zoneName}', ignoring duplicate.");
                        return;
                    }

                    if (agentController.GetTargetName() == zoneName)
                    {
                        Debug.Log($"[ZoneTrigger] AI Agent reached and confirmed discovery of '{zoneName}'.");
                        agentController.DiscoverZone(zoneName, true);
                    }
                }
                Debug.Log($"[ZoneTrigger] AI Agent discovered zone '{zoneName}'.");
                PlayerBehaviorLogger.Instance?.LogAgentZoneDiscovery(zoneName);
                return;
            }

            var currentCharacter = Game.instance.currentCharacter;
            if (currentCharacter == null)
            {
                Debug.LogError("[ZoneTrigger] Current character is null. Cannot log zone discovery.");
                return;
            }

            if (currentCharacter.HasVisitedZone(zoneName))
            {
                Debug.Log($"[ZoneTrigger] Zone '{zoneName}' has already been visited by the player. No reward given.");
                return;
            }

            currentCharacter.MarkZoneAsVisited(zoneName);
            GameSave.instance.Save();

            var entity = other.GetComponent<Entity>();
            if (entity != null)
            {
                PlayerBehaviorLogger.Instance?.LogAreaDiscovered(entity, zoneName);
                GiveZoneReward(entity);
            }

            guiZoneHUD?.ShowZone(zoneStatus, zoneName, zoneDescription);
            Debug.Log($"[ZoneTrigger] Zone '{zoneName}' discovered and saved for character '{currentCharacter.name}'.");
        }
    }
}
