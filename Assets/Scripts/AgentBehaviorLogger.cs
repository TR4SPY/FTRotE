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

        public int zonesDiscovered = 0;
        public int npcInteractions = 0;
        public int enemiesAvoided = 0;
        public int skillsUsed = 0;
        public int enemiesKilled = 0;
        public int agentDeaths = 0; // Licznik śmierci agenta AI

        private HashSet<string> discoveredZones = new HashSet<string>();
        public int waypointsDiscovered = 0;

        // Słownik (lub HashSet) przechowujący odkryte waypointy (po ID, żeby nie powtarzać)
        private HashSet<int> discoveredWaypoints = new HashSet<int>();

        public void LogAgentZoneDiscovery(string zoneName)
        {
            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                Debug.Log($"Agent AI discovered zone: {zoneName}. Total zones discovered: {zonesDiscovered}");
            }
        }

        /// <summary>
        /// Loguje odkrycie waypointu przez AI Agenta, o ile nie zostało już zaliczone.
        /// </summary>
        /// <param name="waypointID">Unikalny ID waypointu.</param>
        public void LogWaypointDiscovery(int waypointID)
        {
            // string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            // Jeśli agent jeszcze nie odkrył tego waypointu
            if (!discoveredWaypoints.Contains(waypointID))
            {
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                Debug.Log($"AI Agent discovered a new waypoint: {waypointID}. " +
                          $"Total discovered: {waypointsDiscovered}");
            }
        }

        public void LogNpcInteraction()
        {
            npcInteractions++;
            Debug.Log($"Agent AI interacted with an NPC! Total interactions: {npcInteractions}");
        }

        private HashSet<string> loggedKills = new HashSet<string>();

        public void LogEnemyKilled(string enemyName)
        {
            if (!loggedKills.Contains(enemyName))
            {
                loggedKills.Add(enemyName);
                enemiesKilled++;
                Debug.Log($"Agent AI killed an enemy: {enemyName}. Total kills: {enemiesKilled}");
            }
        }

        private bool agentDeathLogged = false;

        public void LogAgentDeath(Entity entity)
        {
            if (!agentDeathLogged)
            {
                agentDeathLogged = true;
                agentDeaths++;
                Debug.Log($"{GetActor(entity)} died! Total agent deaths: {agentDeaths}");

                // Reset flagi po odrodzeniu
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

        public void SaveAgentLogsToFile()
        {
            var characterInstance = Game.instance?.currentCharacter;
            if (characterInstance == null)
            {
                Debug.LogWarning("No character instance found. Skipping log save.");
                return;
            }

            string directoryPath = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"AI_BehaviorLogs_{characterInstance.name}.log";
            string filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"=== AI Agent Behavior Logs for {characterInstance.name} as of {timestamp} ===");
                writer.WriteLine($"Zones Discovered: {zonesDiscovered}");
                writer.WriteLine($"NPC Interactions: {npcInteractions}");
                writer.WriteLine($"Avoided Enemies: {enemiesAvoided}");
            }
            Debug.Log($"Agent AI Logs saved to {filePath}");
        }
    }
}
