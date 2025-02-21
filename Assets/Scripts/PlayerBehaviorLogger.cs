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

    private bool difficultyAdjusted = false; // Flaga dla wielokrotności 10
    private HashSet<string> discoveredZones = new HashSet<string>();
    private HashSet<int> discoveredWaypoints = new HashSet<int>();
    public AchievementManager achievementManager { get; private set; }

    // == GRACZ ==
    [Header("Player Stats")]
    [Tooltip("Liczba śmierci gracza.")]
    public int playerDeaths = 0;

    [Tooltip("Liczba ukończonych misji.")]
    public int questsCompleted = 0;

    [Tooltip("Lista zdobytych osiągnięć.")]
    public List<string> unlockedAchievements = new List<string>();
    // public int achievementsUnlocked = 0;

    // == WALKA ==
    [Space(10)]
    [Header("Combat Stats")]
    [Tooltip("Łączny czas walki w sekundach.")]
    public float totalCombatTime = 0f;

    [Tooltip("Liczba pokonanych wrogów.")]
    public int enemiesDefeated = 0;

    [Tooltip("Liczba użytych mikstur.")]
    public int potionsUsed = 0;

    // == EKSPLORACJA ==
    [Space(10)]
    [Header("Exploration Stats")]
    [Tooltip("Liczba odkrytych stref.")]
    public int zonesDiscovered = 0;

    [Tooltip("Liczba interakcji z NPC.")]
    public int npcInteractions = 0;

    [Tooltip("Liczba odkrytych waypointów.")]
    public int waypointsDiscovered = 0;

    // == SYSTEM ==
    [Space(10)]
    [Header("System Stats")]
    [Tooltip("Mnożnik trudności, domyślnie bazuje na enemiesDefeated % 10.")]
    public int difficultyMultiplier = 0;

    [Tooltip("Czy logowanie jest włączone.")]
    public bool isLoggingEnabled { get; set; } = true;

    [Tooltip("Aktualny dynamiczny typ gracza.")]
    public string currentDynamicPlayerType = "Unknown";

    [Tooltip("Czas ostatniej aktualizacji wartości.")]
    public float lastUpdateTime;

    private void Start()
        {
            UpdatePlayerType();
            achievementManager = UnityEngine.Object.FindFirstObjectByType<AchievementManager>();
            if (achievementManager == null)
            {
                Debug.LogError("AchievementManager not found in the scene!");
            }
        }

    public string GetActor(Entity entity)
        {
            if (entity == null) return "Unknown";

            if (entity.isPlayer)
                return "Player";
            else if (entity.isAgent)
                return "AI Agent";
            else
                return "Unknown";
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
            // Debug.Log($"Combat Time: {totalCombatTime}");
            UpdatePlayerType(); // Recalculate player type
        }

        public void PredictAndApplyDifficulty()
        {
            if (AIModel.Instance == null || DifficultyManager.Instance == null)
            {
                Debug.LogError("AIModel or DifficultyManager is missing!");
                return;
            }

            Debug.Log($"[AI-DDA] Player Stats -> Deaths: {playerDeaths}, Enemies: {enemiesDefeated}, Combat Time: {totalCombatTime}, Potions: {potionsUsed}");

            float predictedDifficulty = AIModel.Instance.PredictDifficulty(
                playerDeaths,
                enemiesDefeated,
                totalCombatTime,
                potionsUsed
            );

            DifficultyManager.Instance.SetDifficultyFromAI(predictedDifficulty);
            Debug.Log($"[AI-DDA] Updated Difficulty based on AI Prediction: {predictedDifficulty}");
        }

        public void LogDifficultyMultiplier()
        {
            difficultyMultiplier++;

            /*
            if (difficultyMultiplier % 10 != 0)
            {
                difficultyAdjusted = false;
            }

            if (difficultyMultiplier % 10 == 0 && !difficultyAdjusted)
            {
                PredictAndApplyDifficulty();  // 🔹 Teraz używamy metody AI-DDA
                difficultyAdjusted = true;
            }
            */

            PredictAndApplyDifficulty();
        }

        public void LogEnemiesDefeated(Entity entity)
        {
            string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            enemiesDefeated++;
            Debug.Log($"{actor} defeated an enemy! Total: {enemiesDefeated}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Check achievements in real-time
            PredictAndApplyDifficulty(); 
        }

        /// <summary>
        /// Zaloguj śmierć gracza.
        /// </summary>
        public void LogPlayerDeath(Entity entity)
        {
            string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            playerDeaths++;
            Debug.Log($"{actor} died! Total deaths: {playerDeaths}");

            // Machine Learning decyduje, czy zmniejszyć trudność po śmierci gracza
            PredictAndApplyDifficulty(); 

            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Check achievements in real-time
        }

        public void LogAreaDiscovered(Entity entity, string zoneName)
        {
            string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                Debug.Log($"{actor} discovered a new area: {zoneName}");
                UpdatePlayerType(); // Recalculate player type
                achievementManager?.CheckAchievements(this); // Check achievements in real-time
            }
        }

        public void LogWaypointDiscovery(Entity entity, int waypointID)
        {
            string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            if (!discoveredWaypoints.Contains(waypointID))
            {
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                Debug.Log($"{actor} discovered a new waypoint: {waypointID}");
                UpdatePlayerType(); // Recalculate player type
                achievementManager?.CheckAchievements(this); // Check achievements in real-time
            }
        }

        public void LogNpcInteraction(Entity entity)
        {
            string actor = GetActor(entity); // Rozpoznaj, czy to gracz, czy AI Agent
            npcInteractions++;
            Debug.Log($"{actor} interacted with an NPC! Total: {npcInteractions}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
        }

        public void LogQuestCompleted()
        {
            questsCompleted++;
            Debug.Log($"Quest completed! Total: {questsCompleted}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Sprawdź osiągnięcia w czasie rzeczywistym
        }

        public void LogPotionsUsed(Entity entity)
        {
            string actor = GetActor(entity); // Automatyczne rozpoznanie aktora

            potionsUsed++;
            Debug.Log($"{actor} used a potion! Total: {potionsUsed}");
            UpdatePlayerType(); // Recalculate player type
            achievementManager?.CheckAchievements(this); // Check achievements in real-time
            PredictAndApplyDifficulty(); 
        }

        public void LogCurrentDynamicPlayerType()
        {
            Debug.Log($"Current Dynamic Player Type: {currentDynamicPlayerType}");
        }

        public void LogAgentAction(string action, string target)
        {
            Debug.Log($"AI Agent performed action: {action} on {target}");
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

            unlockedAchievements = new List<string>(characterInstance.unlockedAchievements); 
            int achievementCount = unlockedAchievements.Count;

            Debug.Log($"Loaded Player Behavior Logs for {characterInstance.name}: " +
                    $"Deaths={playerDeaths}, Defeated={enemiesDefeated}, " +
                    $"CombatTime={totalCombatTime}, NPCInteractions={npcInteractions}, " +
                    $"WaypointsDiscovered={waypointsDiscovered}, QuestsCompleted={questsCompleted}, " +
                    $"PotionsUsed={potionsUsed}, ZonesDiscovered={zonesDiscovered}, " +
                    $"AchievementsUnlocked={achievementCount}, " + 
                    $"currentDynamicPlayerType={currentDynamicPlayerType}");
        }

        /// <summary>
        /// Updates the player's type based on current metrics.
        /// </summary>
        public void UpdatePlayerType()
        {
            // Scoring logic
            int achieverScore = (questsCompleted * 2) + (unlockedAchievements.Count * 2);
            int explorerScore = (zonesDiscovered * 2) + waypointsDiscovered;
            int socializerScore = (npcInteractions * 3) + questsCompleted;
            int killerScore = (int)(enemiesDefeated * 0.5) + (int)(totalCombatTime / 120); 

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

            // Pobierz aktualną instancję postaci z Game.instance
            var characterInstance = Game.instance?.currentCharacter;
            if (characterInstance == null)
            {
                Debug.LogWarning("No character instance found. Skipping log save.");
                return;
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Research_{characterInstance.name}.log";
            string filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"=== Player Behavior Logs as of {timestamp} ===");
                writer.WriteLine($"Character Name: {characterInstance.name}");
                writer.WriteLine($"Total Play Time: {FormatPlayTime(Game.instance.currentCharacter.totalPlayTime)}");
                writer.WriteLine($"Player Deaths: {playerDeaths}");
                writer.WriteLine($"Enemies Defeated: {enemiesDefeated}");
                writer.WriteLine($"Total Combat Time: {totalCombatTime} seconds");
                writer.WriteLine($"NPC Interactions: {npcInteractions}");
                writer.WriteLine($"Potions Used: {potionsUsed}");
                writer.WriteLine($"Zones Discovered: {zonesDiscovered}");
                writer.WriteLine($"Quests Completed: {questsCompleted}");
                writer.WriteLine($"Waypoints Discovered: {waypointsDiscovered}");
                writer.WriteLine($"Unlocked Achievements: {unlockedAchievements.Count}");
                writer.WriteLine($"Current Difficulty Multiplier: {difficultyMultiplier}");
                writer.WriteLine($"Player Type based on questionnaire: {characterInstance.playerType}");
                writer.WriteLine($"Current Dynamic Player Type: {currentDynamicPlayerType}");
            }
            Debug.Log($"Logs saved to: {filePath}");
        }

        public void LogAchievement(string achievementName)
        {
            if (!unlockedAchievements.Contains(achievementName))
            {
                unlockedAchievements.Add(achievementName);
                // achievementsUnlocked++;

                Debug.Log($"Achievement unlocked: {achievementName}");

                // Pobranie aktualnej postaci gracza
                var characterInstance = Game.instance?.currentCharacter;

                if (characterInstance != null)
                {
                    characterInstance.unlockedAchievements = new List<string>(unlockedAchievements);

                    // Zapisanie osiągnięcia do systemu zapisów gry
                    Game.instance.GetComponent<GameSave>().SaveLogsForCharacter(characterInstance);

                    Debug.Log($"Achievement '{achievementName}' saved for {characterInstance.name}.");
                }
                else
                {
                    Debug.LogError("Cannot save achievement: Character instance is NULL.");
                }
            }
        }

        public void ExportPlayerData()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "player_logs.csv");

            // Jeśli plik nie istnieje, dodajemy nagłówek
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "playerDeaths,enemiesDefeated,totalCombatTime,enemiesAvoided,potionsUsed,zonesDiscovered,npcInteractions,waypointsDiscovered,achievementsUnlocked,currentDynamicPlayerType,difficultyMultiplier,difficultyLevel\n");
            }

            // Pobranie aktualnego poziomu trudności
            int currentDifficulty = DifficultyManager.Instance.GetCurrentDifficulty();

            // Tworzenie wpisu do CSV
            string logEntry = $"{playerDeaths},{enemiesDefeated},{totalCombatTime},{potionsUsed},{zonesDiscovered},{npcInteractions},{waypointsDiscovered},{unlockedAchievements.Count},{currentDynamicPlayerType},{difficultyMultiplier},{currentDifficulty}\n";

            // Zapis do pliku
            File.AppendAllText(filePath, logEntry);

            Debug.Log("Player data exported to CSV.");
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

            // achievementManager?.CheckAchievements(this);
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