using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class AgentBehaviorLogger : MonoBehaviour
    {
        private static AgentBehaviorLogger _instance;
        public static AgentBehaviorLogger Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<AgentBehaviorLogger>();
#else
                    _instance = Object.FindObjectOfType<AgentBehaviorLogger>();
#endif
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private bool agentDeathLogged = false;
        private HashSet<string> discoveredZones = new HashSet<string>();
        public HashSet<int> discoveredWaypoints = new HashSet<int>();

        [Header("Exploration Stats")]
        [Tooltip("Liczba odkrytych stref.")]
        public int zonesDiscovered = 0;

        [Tooltip("Liczba odkrytych waypointów.")]
        public int waypointsDiscovered = 0;

        [Tooltip("Liczba interakcji z NPC.")]
        public int npcInteractions = 0;

        [Space(10)]

        [Header("Combat Stats")]
        [Tooltip("Liczba pokonanych wrogów.")]
        public int enemiesDefeated = 0;

        [Tooltip("Liczba śmierci Agenta AI.")]
        public int agentDeaths = 0;

        [Tooltip("Łączny czas walki Agenta przez całą sesję gry.")]
        public float totalCombatTime = 0f;

        [Tooltip("Czas walki Agenta w bieżącym epizodzie.")]
        public float combatTimeInEpisode = 0f;

        [Space(10)]

        [Header("Training Stats")]
        [Tooltip("Liczba zakończonych epizodów.")]
        public int episodeCount = 0;

        // public int enemiesAvoided = 0;
        // public int skillsUsed = 0;

        public void LogAgentZoneDiscovery(string zoneName)
        {
            if (!discoveredZones.Contains(zoneName))
            {
                int oldValue = zonesDiscovered;
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                SaveLogs(oldValue, zonesDiscovered);
                Debug.Log($"Agent AI discovered zone: {zoneName}. Total zones discovered: {zonesDiscovered}");
            }
            else
            {
               // Debug.Log($"[AgentBehaviorLogger] Zone {zoneName} already logged. Skipping.");
            }
        }

        public void LogWaypointDiscovery(int waypointID)
        {
            if (!discoveredWaypoints.Contains(waypointID))
            {
                int oldValue = waypointsDiscovered;
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                SaveLogs(oldValue, waypointsDiscovered);
                Debug.Log($"AI Agent discovered a new waypoint: {waypointID}. " +
                        $"Total discovered: {waypointsDiscovered}");
            }
        }

        public void LogNpcInteraction()
        {
            int oldValue = npcInteractions;
            npcInteractions++;
            SaveLogs(oldValue, npcInteractions);
            Debug.Log($"Agent AI interacted with an NPC! Total interactions: {npcInteractions}");
        }

        private HashSet<string> loggedKills = new HashSet<string>();

        public void LogEnemyKilled(string enemyName)
        {
            if (!loggedKills.Contains(enemyName))
            {
                int oldValue = enemiesDefeated;
                loggedKills.Add(enemyName);
                enemiesDefeated++;
                SaveLogs(oldValue, enemiesDefeated);
                Debug.Log($"Agent AI killed an enemy: {enemyName}. Total kills: {enemiesDefeated}");
            }
        }
        public void StartCombat()
        {
            Debug.Log($"[AgentBehaviorLogger] Combat started!");
        }

        public void EndCombat(float combatDuration)
        {
            totalCombatTime += combatDuration;
            combatTimeInEpisode += combatDuration;
            Debug.Log($"[AgentBehaviorLogger] Combat ended! Duration: {combatDuration}s | Total Combat Time: {totalCombatTime}s | Combat Time in Episode: {combatTimeInEpisode}s");
        }

        public void LogCombatTime(float time)
        {
            totalCombatTime = time; // Aktualizacja w Inspektorze
            // Debug.Log($"[AgentBehaviorLogger] Total Combat Time: {totalCombatTime} seconds");
        }

        public void LogCombatTimePerEpisode(float time)
        {
            combatTimeInEpisode = time; // Aktualizacja w Inspektorze
            // Debug.Log($"[AgentBehaviorLogger] Combat Time in Episode: {combatTimeInEpisode} seconds");
        }

        public void LogAgentDeath(Entity entity)
        {
            if (!agentDeathLogged)
            {
                int oldValue = agentDeaths;
                agentDeathLogged = true;
                agentDeaths++;
                SaveLogs(oldValue, agentDeaths);
                Debug.Log($"{GetActor(entity)} died! Total agent deaths: {agentDeaths}");

                entity.Invoke(nameof(ResetAgentDeathLog), 1f);
            }
        }

        private void ResetAgentDeathLog()
        {
            agentDeathLogged = false;
        }


        private string GetActor(Entity entity)
        {
            if (entity.isPlayer)
                return "Player";
            if (entity.isAgent)
                return "Agent AI";
            return entity.name;
        }

        public void SaveLogs(int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                SaveAgentLogsToFile();
            }
        }

        public void SaveAgentLogsToFile()
        {
            try
            {
                Debug.Log("[AgentBehaviorLogger] Attempting to save logs...");

                var characterInstance = Game.instance?.currentCharacter;
                if (characterInstance == null)
                {
                    Debug.LogWarning("[AgentBehaviorLogger] No character instance found. Skipping log save.");
                    return;
                }

                string directoryPath = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"Agent_BehaviorLogs_{characterInstance.name}.log";
                string filePath = Path.Combine(directoryPath, fileName);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"=== AI Agent Behavior Logs for {characterInstance.name} as of {timestamp} ===");
                    writer.WriteLine($"Zones Discovered: {zonesDiscovered}");
                    writer.WriteLine($"Waypoints Discovered: {waypointsDiscovered}");
                    writer.WriteLine($"NPC Interactions: {npcInteractions}");
                    writer.WriteLine($"Enemies Defeated: {enemiesDefeated}");
                    writer.WriteLine($"Agent Deaths: {agentDeaths}");
                    writer.WriteLine($"Total Combat Time: {totalCombatTime} seconds");
                    writer.WriteLine($"Combat Time in Last Episode: {combatTimeInEpisode} seconds");
                    writer.WriteLine($"Total Episodes Completed: {episodeCount}");
                }

                Debug.Log($"[AgentBehaviorLogger] Logs successfully saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AgentBehaviorLogger] Failed to save logs: {ex.Message}");
            }
        }
    }
}