//  - DODANO 29 GRUDNIA 2024 - 0001

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using PLAYERTWO.ARPGProject;

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

                   /* if (_instance == null)
                    {
                        Debug.LogError("PlayerBehaviorLogger.Instance was accessed, but no instance exists in the scene.");
                    } */
                }
                return _instance;
            }
        }

        private void Awake()
        {
            lastUpdateTime = Time.time;

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
    public int difficultyMultiplier = 0;  // Mnożnik trudności (domyślnie bazuje na wzorze enemiesDefeated % 10)
    public int enemiesDefeated = 0;  // Licznik pokonanych wrogów
    public float totalCombatTime = 0f;  // Łączny czas walki
    public int potionsUsed = 0;  // Licznik użytych mikstur
    public int zonesDiscovered = 0;
    public int npcInteractions = 0;
    public int questsCompleted = 0;
    public int waypointsDiscovered = 0;
    public string currentDynamicPlayerType = "Unknown";
    public float lastUpdateTime;
    private bool difficultyAdjusted = false; // Flaga dla wielokrotności 10
    private HashSet<string> discoveredZones = new HashSet<string>();
    private HashSet<int> discoveredWaypoints = new HashSet<int>();
    public bool isLoggingEnabled { get; set; } = true;
    public int achievementsUnlocked = 0; // Licznik zdobytych osiągnięć
    public List<string> unlockedAchievements = new List<string>(); // Lista nazw osiągnięć
    private AchievementManager achievementManager;

    private void Start()
        {
            UpdatePlayerType();
            achievementManager = UnityEngine.Object.FindFirstObjectByType<AchievementManager>();
            if (achievementManager == null)
            {
                Debug.LogError("AchievementManager not found in the scene!");
            }
        }

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
            UpdatePlayerType(); // Recalculate player type
        }

            /// <summary>
    /// Zaloguj pokonanie wroga.
    /// </summary>
    public void LogDifficultyMultiplier()
    {
        difficultyMultiplier++;

        // Reset flagi po osiągnięciu kolejnego progu 10
        if (difficultyMultiplier % 10 != 0)
        {
            difficultyAdjusted = false;
        }

        // Wywołaj AdjustDifficulty tylko przy wielokrotności 10 i gdy trudność jeszcze nie została dostosowana
        if (difficultyMultiplier % 10 == 0 && !difficultyAdjusted)
        {
            DifficultyManager.Instance.AdjustDifficulty(this);
            difficultyAdjusted = true; // Flaga oznaczająca, że trudność została dostosowana
            Debug.Log($"Difficulty Multiplier is now: {difficultyMultiplier}");
        }
    }
    public void LogEnemiesDefeated()
    {
        enemiesDefeated++;
        Debug.Log($"Enemies defeated: {enemiesDefeated}");
        UpdatePlayerType(); // Recalculate player type
        achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
    }

        /// <summary>
        /// Zaloguj śmierć gracza.
        /// </summary>
        public void LogPlayerDeath()
        {
            playerDeaths++;
            Debug.Log($"Player deaths: {playerDeaths}");
            DifficultyManager.Instance.AdjustDifficulty(this);
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
        }

        public void LogAreaDiscovered(string zoneName)
        {
            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                Debug.Log($"Discovered new area: {zoneName}");
                UpdatePlayerType(); // Recalculate player type
                achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
            }
        }

        public void LogWaypointDiscovery(int waypointID)
        {
            if (!discoveredWaypoints.Contains(waypointID))
            {
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                Debug.Log($"Discovered new waypoint: {waypointID}");
                UpdatePlayerType(); // Recalculate player type
                achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
            }
        }

        public void LogNpcInteraction()
        {
            npcInteractions++;
            Debug.Log($"NPC interaction! Total: {npcInteractions}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
        }
        public void LogQuestCompleted()
        {
            questsCompleted++;
            Debug.Log($"Quest completed! Total: {questsCompleted}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
        }

        public void LogPotionsUsed()
        {
            potionsUsed++;
            Debug.Log($"Potion used! Total: {potionsUsed}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
        }

        public void LogCurrentDynamicPlayerType()
        {
            Debug.Log($"Current Dynamic Player Type: {currentDynamicPlayerType}");
        }

        public void LoadLogs(CharacterInstance characterInstance)
        {
            if (characterInstance == null)
            {
                Debug.LogError("CharacterInstance is null. Cannot load logs.");
                return;
            }

            playerDeaths = characterInstance.playerDeaths;
            enemiesDefeated = characterInstance.enemiesDefeated;
            totalCombatTime = characterInstance.totalCombatTime;
            npcInteractions = characterInstance.npcInteractions;
            waypointsDiscovered = characterInstance.waypointsDiscovered;
            questsCompleted = characterInstance.questsCompleted;
            potionsUsed = characterInstance.potionsUsed;
            zonesDiscovered = characterInstance.zonesDiscovered;
            currentDynamicPlayerType = characterInstance.currentDynamicPlayerType;

            Debug.Log($"Loaded Player Behavior Logs for {characterInstance.name}: " +
                    $"Deaths={playerDeaths}, Defeated={enemiesDefeated}, " +
                    $"CombatTime={totalCombatTime}, NPCInteractions={npcInteractions}, " +
                    $"WaypointsDiscovered={waypointsDiscovered}, QuestsCompleted={questsCompleted}, " +
                    $"PotionsUsed={potionsUsed}, ZonesDiscovered={zonesDiscovered}, " +
                    $"currentDynamicPlayerType={currentDynamicPlayerType}");
        }

        /// <summary>
        /// Updates the player's type based on current metrics.
        /// </summary>
        public void UpdatePlayerType()
        {
            // Scoring logic
            int achieverScore = questsCompleted + waypointsDiscovered;
            int explorerScore = zonesDiscovered + waypointsDiscovered;
            int socializerScore = npcInteractions;
            int killerScore = enemiesDefeated + (int)(totalCombatTime / 60);

            // Determine dominant type
            string newPlayerType = DetermineDominantType(achieverScore, explorerScore, socializerScore, killerScore);

            // Update if changed
            if (newPlayerType != currentDynamicPlayerType)
            {
                Debug.Log($"Player type updated: {currentDynamicPlayerType} -> {newPlayerType}");
                currentDynamicPlayerType = newPlayerType;
            }
        }

        /// <summary>
        /// Determines the dominant player type.
        /// </summary>
        private string DetermineDominantType(int achiever, int explorer, int socializer, int killer)
        {
            int maxScore = Mathf.Max(achiever, explorer, socializer, killer);

            if (maxScore == achiever) return "Achiever";
            if (maxScore == explorer) return "Explorer";
            if (maxScore == socializer) return "Socializer";
            if (maxScore == killer) return "Killer";

            return "Undefined";
        }

        public void SaveLogsToFile()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var characterInstance = Game.instance?.currentCharacter;
            if (characterInstance == null)
            {
                Debug.LogWarning("No character instance found. Skipping log save.");
                return;
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Research_{characterInstance.name}_{timestamp}.log";
            string filePath = Path.Combine(directoryPath, fileName);

            // Tworzenie struktury danych logu
            var logData = new
            {
                timestamp = timestamp,
                characterName = characterInstance.name,
                totalPlayTime = FormatPlayTime(characterInstance.totalPlayTime),
                playerDeaths = playerDeaths,
                enemiesDefeated = enemiesDefeated,
                totalCombatTime = totalCombatTime,
                npcInteractions = npcInteractions,
                potionsUsed = potionsUsed,
                zonesDiscovered = zonesDiscovered,
                questsCompleted = questsCompleted,
                waypointsDiscovered = waypointsDiscovered,
                achievementsUnlocked = achievementsUnlocked,

                currentDifficultyMultiplier = difficultyMultiplier,
                playerTypeQuestionnaire = characterInstance.playerType,
                dynamicPlayerType = currentDynamicPlayerType
            };

            // Serializacja do JSON
            string jsonContent = JsonUtility.ToJson(logData, true);

            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"Logs saved to: {filePath}");
        }
        public void LogAchievement(string achievementName)
        {
            if (!unlockedAchievements.Contains(achievementName))
            {
                unlockedAchievements.Add(achievementName);
                achievementsUnlocked++;
                Debug.Log($"Achievement unlocked: {achievementName}");
            }
        }

        public void ApplySettingsFromGameSettings()
        {
            if (GameSettings.instance != null)
            {
                isLoggingEnabled = GameSettings.instance.GetSaveLogs();
                Debug.Log($"PlayerBehaviorLogger: Logging is {(isLoggingEnabled ? "Enabled" : "Disabled")} from settings.");
            }
        }

        private void Update()
        {
            if (Game.instance.currentCharacter != null)
            {
                float deltaTime = Time.time - lastUpdateTime;
                Game.instance.currentCharacter.totalPlayTime += deltaTime;
                lastUpdateTime = Time.time;
            }

            if (achievementManager == null)
            {
                achievementManager = UnityEngine.Object.FindFirstObjectByType<AchievementManager>();
            }

            achievementManager?.CheckAchievements(this);
        }

        public static string FormatPlayTime(float totalSeconds)
        {
            int days = (int)(totalSeconds / 86400); // 1 dzień = 86400 sekund
            int hours = (int)((totalSeconds % 86400) / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            if (days > 0)
                return $"{days}d {hours}h {minutes}m {seconds}s";
            else if (hours > 0)
                return $"{hours}h {minutes}m {seconds}s";
            else if (minutes > 0)
                return $"{minutes}m {seconds}s";
            else
                return $"{seconds}s";
        }

        public void LogAgentZoneDiscovery(string zoneName)
        {
            Debug.Log($"AI Agent discovered zone: {zoneName}");
        }

        /// <summary>
        /// Resetowanie logów (opcjonalne).
        /// </summary>
        public void ResetLogs()
        {
            Debug.Log("Resetting logs...");
            //playerDeaths = 0;
            difficultyMultiplier = 0;
            //enemiesDefeated = 0;
            //totalCombatTime = 0f;
            //potionsUsed = 0;
            difficultyAdjusted = false;
        }
    }
}