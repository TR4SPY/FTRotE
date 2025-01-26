using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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

        private HashSet<string> discoveredZones = new HashSet<string>();

        public void LogAgentZoneDiscovery(string zoneName)
        {
            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                Debug.Log($"Agent AI discovered zone: {zoneName}. Total zones discovered: {zonesDiscovered}");
            }
        }

        public void LogNpcInteraction()
        {
            npcInteractions++;
            Debug.Log($"Agent AI interacted with an NPC! Total interactions: {npcInteractions}");
        }

        public void LogEnemyAvoided(string enemyName)
        {
            enemiesAvoided++;
            Debug.Log($"Agent AI avoided enemy: {enemyName}. Total avoided: {enemiesAvoided}");
        }

        public void LogSkillUsed(string skillName)
        {
            skillsUsed++;
            Debug.Log($"Agent AI used skill: {skillName}. Total skills used: {skillsUsed}");
        }

        public void SaveLogsToFile()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"AI_Agent_BehaviorLogs_{timestamp}.log";
            string filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"=== AI Agent Behavior Logs as of {timestamp} ===");
                writer.WriteLine($"Zones Discovered: {zonesDiscovered}");
                writer.WriteLine($"NPC Interactions: {npcInteractions}");
                writer.WriteLine($"Enemies Avoided: {enemiesAvoided}");
                writer.WriteLine($"Skills Used: {skillsUsed}");
            }

            Debug.Log($"Agent AI Logs saved to {filePath}");
        }
    }
}
