//  ZMODYFIKOWANO 31 GRUDNIA 2024
using UnityEngine;
using UnityEngine.Events;
using System;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Waypoint")]
    public class Waypoint : MonoBehaviour
    {
        public UnityEvent onActive;

        [Header("Waypoint Settings")]
        public int waypointID = -1; // Unikalne ID Waypoint
       public bool autoActive = true;
        public float activationRadius = 10f;
        public string title = "New Waypoint";

        [Header("Restrictions")]
        public CharacterClassRestrictions allowedClasses = CharacterClassRestrictions.None;
        public PlayerType allowedPlayerTypes = PlayerType.None;

        [Header("Waypoint Discovery Rewards")]
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

        protected float m_startTime;
        protected bool m_active = false;
        protected Collider m_collider;

        protected const float k_triggerActivationDelay = 0.1f;
        protected static int globalWaypointID = 0;

        protected LevelRespawner m_respawner => LevelRespawner.instance;
        protected LevelWaypoints m_waypoints => LevelWaypoints.instance;

        public bool active
        {
            get { return m_active; }
            set
            {
                if (!m_active && value)
                {
                    onActive.Invoke();
                    LogWaypointDiscovery();
                }
                m_active = value;
            }
        }

        protected Entity m_player => Level.instance.player;

        public virtual SpacePoint GetSpacePoint()
        {
            var position = transform.position;
            position += Vector3.up * m_player.controller.height * 0.5f;
            return new(position, transform.rotation);
        }

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
            m_startTime = Time.time;

            if (waypointID == -1)
            {
                waypointID = globalWaypointID++;
                Debug.Log($"Assigned unique ID {waypointID} to Waypoint '{name}'");
            }
        }

        protected virtual void Update()
        {
            var distance = Vector3.Distance(m_player.position, transform.position);

            if (distance <= activationRadius)
            {
                m_waypoints.currentWaypoint = this;

                if (!active && autoActive)
                    active = true;
            }
        }

        protected virtual bool CanTrigger() => active && !m_waypoints.traveling &&
            !m_respawner.isRespawning && Time.time > k_triggerActivationDelay;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!CanTrigger() || !other.CompareTag(GameTags.Player))
                return;

            m_player.StandStill();
            m_player.inputs.LockMoveDirection();
            m_waypoints.currentWaypoint = this;
            GUIWindowsManager.instance.waypointsWindow.Show();
        }

        private void GiveDiscoveryReward(Entity player)
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

            if (player.stats != null)
                finalGold = Mathf.RoundToInt(finalGold * (1f + player.stats.additionalMoneyRewardPercent / 100f));
            player.inventory.instance.money += finalGold;

            Debug.Log($"[Waypoint] {player.name} discovered waypoint '{title}' (ID: {waypointID}) and received {finalExp} EXP, {finalGold} Gold.");
        }

        protected void LogWaypointDiscovery(Collider other = null)
        {
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

            Entity interactor = m_player;

            if (other != null)
            {
                bool isPlayer = other.CompareTag(GameTags.Player);
                bool isAI = other.GetComponent<AgentController>()?.isAI == true;

                if (!isPlayer && !isAI)
                {
                    Debug.LogWarning($"Waypoint discovery ignored. {other.name} is neither Player nor AI Agent.");
                    return;
                }

                interactor = other.GetComponent<Entity>();
                if (interactor == null)
                {
                    Debug.LogWarning($"Entity is null for {other.name}. Cannot log waypoint discovery.");
                    return;
                }
            }

            if (AgentBehaviorLogger.Instance != null)
            {
                AgentBehaviorLogger.Instance.LogWaypointDiscovery(waypointID); 
                Debug.Log($"[Waypoint] AI discovered '{title}' (ID: {waypointID})");
            }

            if (PlayerBehaviorLogger.Instance != null && interactor != null)
            {
                if (Game.instance.currentCharacter.HasActivatedWaypoint(waypointID))
                {
                    Debug.Log($"[Waypoint] '{title}' (ID: {waypointID}) already discovered. No reward given.");
                    return;
                }

                Game.instance.currentCharacter.MarkWaypointAsActivated(waypointID);
                PlayerBehaviorLogger.Instance.LogWaypointDiscovery(interactor, waypointID);
                GiveDiscoveryReward(interactor);
                
                Debug.Log($"[Waypoint] '{title}' (ID: {waypointID}) discovered and logged by {interactor.name}.");
            }
            else if (interactor == null)
            {
                Debug.LogWarning("[Waypoint] Interactor entity is null. Cannot log waypoint discovery.");
            }
            else
            {
                Debug.LogWarning("[Waypoint] PlayerBehaviorLogger.Instance is null. Cannot log waypoint discovery.");
            }
        }

        public static void SetPlayer(Entity entity)
        {
            Level.instance?.SetPlayer(entity);
        }
    }
}
