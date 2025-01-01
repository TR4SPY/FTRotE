//  - DODANO 29 GRUDNIA 2024 - 0001

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    public class PlayerBehaviorLogger : MonoBehaviour
    {

        private static PlayerBehaviorLogger _instance;
        public static PlayerBehaviorLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    #if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<PlayerBehaviorLogger>();
                    #else
                    _instance = Object.FindObjectOfType<PlayerBehaviorLogger>();
                    #endif

                    if (_instance == null)
                    {
                        Debug.LogError("PlayerBehaviorLogger.Instance was accessed, but no instance exists in the scene.");
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

    public int playerDeaths = 0;  // Licznik śmierci gracza
    public int enemiesDefeated = 0;  // Licznik pokonanych wrogów
    public float totalCombatTime = 0f;  // Łączny czas walki
    public int potionsUsed = 0;  // Licznik użytych mikstur
    public int zonesDiscovered = 0;
    public int npcInteractions = 0;
    public int questsCompleted = 0;
    public int waypointsDiscovered = 0;
    private bool difficultyAdjusted = false; // Flaga dla wielokrotności 10
    private HashSet<string> discoveredZones = new HashSet<string>();
    private HashSet<int> discoveredWaypoints = new HashSet<int>();

    private float combatStartTime;

        /// <summary>
        /// Wywołaj na początku walki.
        /// </summary>
        public void StartCombat()
        {
            combatStartTime = Time.time;
        }

        /// <summary>
        /// Wywołaj po zakończeniu walki.
        /// </summary>
        public void EndCombat()
        {
            totalCombatTime += Time.time - combatStartTime;
            Debug.Log($"Combat Time: {totalCombatTime}");
        }

            /// <summary>
    /// Zaloguj pokonanie wroga.
    /// </summary>
    public void LogEnemyDefeated()
    {
        enemiesDefeated++;
        Debug.Log($"Enemies defeated: {enemiesDefeated}");

        // Reset flagi po osiągnięciu kolejnego progu 10
        if (enemiesDefeated % 10 != 0)
        {
            difficultyAdjusted = false;
        }

        // Wywołaj AdjustDifficulty tylko przy wielokrotności 10 i gdy trudność jeszcze nie została dostosowana
        if (enemiesDefeated % 10 == 0 && !difficultyAdjusted)
        {
            DifficultyManager.Instance.AdjustDifficulty(this);
            difficultyAdjusted = true; // Flaga oznaczająca, że trudność została dostosowana
        }
    }

        /// <summary>
        /// Zaloguj śmierć gracza.
        /// </summary>
        public void LogPlayerDeath()
        {
            playerDeaths++;
            Debug.Log($"Player deaths: {playerDeaths}");
            DifficultyManager.Instance.AdjustDifficulty(this);
        }

        public void LogAreaDiscovered(string zoneName)
        {
            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                Debug.Log($"Discovered new area: {zoneName}");
            }
        }

        public void LogWaypointDiscovery(int waypointID)
        {
            if (!discoveredWaypoints.Contains(waypointID))
            {
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                Debug.Log($"Discovered new waypoint: {waypointID}");
            }
        }

        public void LogNpcInteraction()
        {
            npcInteractions++;
            Debug.Log($"NPC interaction! Total: {npcInteractions}");
        }
        public void LogQuestCompleted()
        {
            questsCompleted++;
            Debug.Log($"Quest completed! Total: {questsCompleted}");
        }

        /// <summary>
        /// Resetowanie logów (opcjonalne).
        /// </summary>
        public void ResetLogs()
        {
            Debug.Log("Resetting logs...");
            playerDeaths = 0;
            enemiesDefeated = 0;
            totalCombatTime = 0f;
            potionsUsed = 0;
            difficultyAdjusted = false;
        }
    }
}