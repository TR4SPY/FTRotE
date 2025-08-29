using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PLAYERTWO.ARPGProject;

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

        protected Collider m_collider;

        private static readonly List<ZoneTrigger> allZones = new();
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
            if (!allZones.Contains(this))
                allZones.Add(this);
        }

        private void OnDisable()
        {
            allZones.Remove(this);
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

            bool isPlayer = other.CompareTag(GameTags.Player);
            bool isAI = other.GetComponent<AgentController>()?.isAI == true;

            if (!isPlayer && !isAI) return;

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

            var character = Game.instance.currentCharacter;
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
            ZoneTrigger exact = allZones
                .Where(z =>
                {
                    Bounds bounds = z.m_collider.bounds;
                    bounds.Expand(1f);
                    return z.requireInside && bounds.Contains(position);
                })
                .OrderByDescending(z => z.priority)
                .ThenBy(z => Vector3.Distance(position, z.transform.position))
                .FirstOrDefault();

            if (exact != null) return exact;

            ZoneTrigger nearby = allZones
                .Where(z => !z.requireInside)
                .Where(z => Vector3.Distance(position, z.transform.position) <= z.influenceRadius)
                .OrderByDescending(z => z.priority)
                .ThenBy(z => Vector3.Distance(position, z.transform.position))
                .FirstOrDefault();

            return nearby;
        }
    }
}
