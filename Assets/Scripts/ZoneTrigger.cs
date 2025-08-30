using UnityEngine;
using System.Collections.Generic;
// using System.Linq;
using PLAYERTWO.ARPGProject;
using System;

namespace AI_DDA.Assets.Scripts
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Zone Trigger")]
    public class ZoneTrigger : MonoBehaviour
    {

        [Header("Zone Trigger Options")]
        public bool showInHUD = true;
        public bool giveReward = true;

        public string zoneStatus = "New Area Discovered";

        [Tooltip("The name of the zone.")]
        public string zoneName;

        [Tooltip("The description of the zone.")]
        public string zoneDescription;

        [Header("Restrictions")]
        public CharacterClassRestrictions allowedClasses = CharacterClassRestrictions.None;
        public PlayerType allowedPlayerTypes = PlayerType.None;


        [Header("Region Tracking")]
        public bool requireInside = true;
        public float influenceRadius = 60f;
        public int priority = 0;

        [Header("Zone Discovery Rewards")]
        [Header("Achiever Rewards")]
        public int achieverExpReward = 100;
        public int achieverGoldReward = 100;

        [Header("Killer Rewards")]
        public int killerExpReward = 100;
        public int killerGoldReward = 100;

        [Header("Socializer Rewards")]
        public int socializerExpReward = 100;
        public int socializerGoldReward = 100;

        [Header("Explorer Rewards")]
        public int explorerExpReward = 500;
        public int explorerGoldReward = 500;

        private GUIZoneHUD guiZoneHUD;
        private bool wasTriggeredOnce = false;

        // Prevents zone discovery immediately on scene load when the player
        // starts inside one or more ZoneTrigger colliders. This avoids
        // counting the spawn region as "discovered" before the player moves.
        private const float InitialTriggerGracePeriod = 0.5f;

        protected Collider m_collider;

        private static readonly HashSet<string> triggeredZoneNames = new();

        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();

            if (!m_collider.isTrigger)
            {
                m_collider.isTrigger = true;
                Debug.LogWarning($"[ZoneTrigger] Collider on '{gameObject.name}' was not set as Trigger. Automatically corrected.");
            }
        }

        private void OnEnable()
        {
            LevelZones.instance?.RegisterZone(this);
        }

        private void OnDisable()
        {
            LevelZones.instance?.UnregisterZone(this);
        }

        private void Start()
        {
            InitializeCollider();

#if UNITY_2023_1_OR_NEWER
            guiZoneHUD = UnityEngine.Object.FindFirstObjectByType<GUIZoneHUD>();
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

            // string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;
            
            var character = Game.instance.currentCharacter;
            if (character == null)
                return;

            string playerType = character.currentDynamicPlayerType;

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

            if (player.stats != null)
                finalGold = Mathf.RoundToInt(finalGold * (1f + player.stats.additionalMoneyRewardPercent / 100f));

            player.inventory.instance.money += finalGold;

            Debug.Log($"[Zone] {player.name} discovered zone '{zoneName}' and received {finalExp} EXP, {finalGold} Gold.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            // Ignore trigger events fired during the initial frames after the
            // level loads. This happens when the player spawns already inside
            // a zone collider which would otherwise mark the zone as visited
            // without any actual exploration.
            if (Time.timeSinceLevelLoad < InitialTriggerGracePeriod)
                return;

            bool isPlayer = other.CompareTag(GameTags.Player);
            bool isAI = other.GetComponent<AgentController>()?.isAI == true;

            if (!isPlayer && !isAI) return;

            var character = Game.instance.currentCharacter;
            CharacterClassRestrictions playerClass = CharacterClassRestrictions.None;
            PlayerType playerTypeEnum = PlayerType.None;
            if (character != null)
            {
                if (character.data?.classPrefab != null)
                    playerClass = CharacterInstance.GetClassBitFromName(character.data.classPrefab.name);
                else
                    playerClass = CharacterInstance.GetClassBitFromName(character.name);
                Enum.TryParse(character.currentDynamicPlayerType, true, out playerTypeEnum);
            }

            if (allowedClasses != CharacterClassRestrictions.None && (allowedClasses & playerClass) == 0)
                return;
            if (allowedPlayerTypes != PlayerType.None && (allowedPlayerTypes & playerTypeEnum) == 0)
                return;

            if (string.IsNullOrEmpty(zoneName))
            {
                Debug.LogError("[ZoneTrigger] Zone name is null or empty.");
                return;
            }

            if (triggeredZoneNames.Contains(zoneName))
                return;

            if (isAI)
            {
                var agent = other.GetComponent<AgentController>();
                if (agent != null)
                {
                    if (agent.HasDiscoveredZone(zoneName))
                    {
                       // Debug.Log($"[ZoneTrigger] AI Agent already discovered '{zoneName}', skipping.");
                        return;
                    }

                    if (agent.GetTargetName() == zoneName)
                    {
                       // Debug.Log($"[ZoneTrigger] AI Agent reached and confirmed discovery of '{zoneName}'.");
                        agent.DiscoverZone(zoneName, true);
                    }

                    PlayerBehaviorLogger.Instance?.LogAgentZoneDiscovery(zoneName);
                }

                return;
            }

            if (character == null)
            {
                Debug.LogError("[ZoneTrigger] Current character is null. Cannot log zone discovery.");
                return;
            }

            if (character.HasVisitedZone(zoneName))
            {
               // Debug.Log($"[ZoneTrigger] Zone '{zoneName}' already visited by player.");
                return;
            }

            character.MarkZoneAsVisited(zoneName);
            GameSave.instance.Save();
            triggeredZoneNames.Add(zoneName);

            var entity = other.GetComponent<Entity>();
            if (entity != null)
            {
                PlayerBehaviorLogger.Instance?.LogAreaDiscovered(entity, zoneName);
                if (giveReward)
                    GiveZoneReward(entity);
            }

            if (showInHUD)
                guiZoneHUD?.ShowZone(zoneStatus, zoneName, zoneDescription);

           // Debug.Log($"[ZoneTrigger] Zone '{zoneName}' discovered and saved for '{character.name}'.");
        }

        /// <summary>
        /// Static method for determining the most appropriate region for a given position.
        /// </summary>
        public static ZoneTrigger GetCurrentRegion(Vector3 position)
        {
            return LevelZones.instance?.GetCurrentRegion(position);
        }
    }
}
