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

                   /*   UNCOMMENT IN CASE OF INSTANCE ISSUES

                   if (_instance == null)
                    {
                        Debug.LogError("PlayerBehaviorLogger.Instance was accessed, but no instance exists in the scene.");
                    } 
                    
                    */

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
                // DontDestroyOnLoad(gameObject);   //  IN CASE OF INSTANCE ISSUES
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }

            statsManager = Game.instance?.GetComponentInChildren<GUIStatsManager>();

            if (statsManager == null)
            {
              //  Debug.LogError("GUIStatsManager not found in Game.instance!");
            }
        }

    private bool difficultyAdjusted = false;
    private HashSet<string> discoveredZones = new HashSet<string>();
    private HashSet<int> discoveredWaypoints = new HashSet<int>();
    public AchievementManager achievementManager { get; private set; }

    private GUIStatsManager statsManager;
    
    // == PLAYER DATA ==
    [Header("Player Stats")]
    [Tooltip("Amount of Player deaths.")]
    public int playerDeaths = 0;

    [Tooltip("Amount of Quests completed.")]
    public int questsCompleted = 0;

    [Tooltip("List of Unlocked achievements.")]
    public List<string> unlockedAchievements = new List<string>();
    // public int achievementsUnlocked = 0; // OLD - NO LONGER USED

    // == COMBAT DATA ==
    [Space(10)]
    [Header("Combat Stats")]
    [Tooltip("Total combat time in seconds.")]
    public float totalCombatTime = 0f;

    [Tooltip("Amount of defeated enemies.")]
    public int enemiesDefeated = 0;

    [Tooltip("Amount of potions used.")]
    public int potionsUsed = 0;

    // == EXPLORATION DATA ==
    [Space(10)]
    [Header("Exploration Stats")]
    [Tooltip("Amount of discovered zones.")]
    public int zonesDiscovered = 0;

    [Tooltip("Amount of NPC interactions.")]
    public int npcInteractions = 0;

    [Tooltip("Amount of discovered waypoints.")]
    public int waypointsDiscovered = 0;

    // == SYSTEM DATA ==
    [Space(10)]
    [Header("System Stats")]
    [Tooltip("Difficulty multiplier for AI models.")]
    public int difficultyMultiplier = 0;

    [Tooltip("Is logging of data enabled?")]
    public bool isLoggingEnabled { get; set; } = true;

    [Tooltip("Current dynamic Bartle's player type.")]
    public string currentDynamicPlayerType = "Unknown";

    [Tooltip("Time of last update.")]
    public float lastUpdateTime;
    
    private float lastCombatEventTime = 0f;
    private const float combatWindow  = 5f;

    private float lastPredictionTime = 0f;
    private float predictionCooldown = 2f;

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

        public void StartCombat()
        {
            combatStartTime = Time.time;
        }

        public void EndCombat()
        {
            totalCombatTime += Time.time - combatStartTime;
            // Debug.Log($"Combat Time: {totalCombatTime}");    //  DEBUG - Total Combat Time
            UpdatePlayerType();
        }

        public void PredictAndApplyDifficulty()
        {
            if (AIModel.Instance == null || DifficultyManager.Instance == null || RLModel.Instance == null)
            {
                Debug.LogError("MLP model, RL model, or DifficultyManager is missing!");
                return;
            }

            // Debug.Log($"[AI-DDA] Player Stats -> Deaths: {playerDeaths}, Enemies: {enemiesDefeated}, Combat Time: {totalCombatTime}, Potions: {potionsUsed}");   //  DEBUG

            if (Time.time - lastPredictionTime < predictionCooldown)
            {
                // Debug.Log("[AI-DDA] Skipping duplicate difficulty prediction.");
                return;
            }
            
            lastPredictionTime = Time.time;

            // Multilayer Perceptron ONNX - 1st Model Prediction
            float predictedDifficulty = AIModel.Instance.PredictDifficulty(
                playerDeaths,
                enemiesDefeated,
                totalCombatTime,
                potionsUsed
            );

            // Reinforcement Learning ONNX - 2nd Model Correction
            RLModel.Instance.AdjustDifficulty(predictedDifficulty);
        }

        public void LogDifficultyMultiplier()
        {
            difficultyMultiplier++;

            /*  OLD - NO LONGER USED

            if (difficultyMultiplier % 10 != 0)
            {
                difficultyAdjusted = false;
            }

            if (difficultyMultiplier % 10 == 0 && !difficultyAdjusted)
            {
                PredictAndApplyDifficulty();
                difficultyAdjusted = true;
            }

            */

            PredictAndApplyDifficulty();
        }

        public void LogEnemiesDefeated(Entity entity)
        {
            string actor = GetActor(entity);
            lastCombatEventTime = Time.time;

            enemiesDefeated++;
            BestiaryManager.Instance?.RegisterKill(entity);

            var faction = entity != null ? entity.GetComponent<FactionMember>()?.faction ?? Faction.None : Faction.None;
            ReputationManager.Instance?.Adjust(faction, -5);

            // Debug.Log($"{actor} defeated an enemy! Total: {enemiesDefeated}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
            PredictAndApplyDifficulty(); 
            // DifficultyManager.Instance?.UpdateAllEnemyStats();
        }

        public void LogPlayerDeath(Entity entity)
        {
            string actor = GetActor(entity);
            lastCombatEventTime = Time.time;

            playerDeaths++;
            // Debug.Log($"{actor} died! Total deaths: {playerDeaths}");

            PredictAndApplyDifficulty(); 

            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);    
            // DifficultyManager.Instance?.UpdateAllEnemyStats();
        }

        public void LogAreaDiscovered(Entity entity, string zoneName)
        {
            string actor = GetActor(entity);

            if (!discoveredZones.Contains(zoneName))
            {
                discoveredZones.Add(zoneName);
                zonesDiscovered++;
                // Debug.Log($"{actor} discovered a new area: {zoneName}");
                UpdatePlayerType();
                achievementManager?.CheckAchievements(this);
            }
        }

        public void LogWaypointDiscovery(Entity entity, int waypointID)
        {
            string actor = GetActor(entity);

            if (!discoveredWaypoints.Contains(waypointID))
            {
                discoveredWaypoints.Add(waypointID);
                waypointsDiscovered++;
                // Debug.Log($"{actor} discovered a new waypoint: {waypointID}");
                UpdatePlayerType();
                achievementManager?.CheckAchievements(this);
            }
        }

        public void LogNpcInteraction(Entity entity, Faction faction = Faction.None)
        {
            string actor = GetActor(entity);
            npcInteractions++;
            ReputationManager.Instance?.Adjust(faction, 1);
            // Debug.Log($"{actor} interacted with an NPC! Total: {npcInteractions}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
        }

        public void LogQuestCompleted(Faction faction = Faction.None)
        {
            questsCompleted++;
            ReputationManager.Instance?.Adjust(faction, 5);
            // Debug.Log($"Quest completed! Total: {questsCompleted}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
        }

        public void LogPotionsUsed(Entity entity)
        {
            string actor = GetActor(entity);
            lastCombatEventTime = Time.time;

            potionsUsed++;
            // Debug.Log($"{actor} used a potion! Total: {potionsUsed}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
            PredictAndApplyDifficulty(); 
        }

        public void LogCurrentDynamicPlayerType()
        {
            // Debug.Log($"Current Dynamic Player Type: {currentDynamicPlayerType}");
        }

        public void LogAgentAction(string action, string target)
        {
            // Debug.Log($"AI Agent performed action: {action} on {target}");
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

            if (characterInstance.reputation != null && ReputationManager.Instance != null)
            {
                var dict = new Dictionary<Faction, int>();
                foreach (var kvp in characterInstance.reputation)
                {
                    if (System.Enum.TryParse(kvp.Key, out Faction faction))
                        dict[faction] = kvp.Value;
                }
                ReputationManager.Instance.Load(dict);
            }

            /* 
                Debug.Log($"Loaded Player Behavior Logs for {characterInstance.name}: " +
                    $"Deaths={playerDeaths}, Defeated={enemiesDefeated}, " +
                    $"CombatTime={totalCombatTime}, NPCInteractions={npcInteractions}, " +
                    $"WaypointsDiscovered={waypointsDiscovered}, QuestsCompleted={questsCompleted}, " +
                    $"PotionsUsed={potionsUsed}, ZonesDiscovered={zonesDiscovered}, " +
                    $"AchievementsUnlocked={achievementCount}, " + 
                    $"currentDynamicPlayerType={currentDynamicPlayerType}");
            */
        }

        public void UpdatePlayerType()
        {
            int achieverScore = (questsCompleted * 2) + (unlockedAchievements.Count * 2);
            int explorerScore = (zonesDiscovered * 2) + waypointsDiscovered;
            int socializerScore = (npcInteractions * 3) + questsCompleted;
            int killerScore = (int)(enemiesDefeated * 0.5) + (int)(totalCombatTime / 120); 

            string newPlayerType = DetermineDominantType(achieverScore, explorerScore, socializerScore, killerScore);

            if (newPlayerType != currentDynamicPlayerType)
            {
                // Debug.Log($"Player type updated: {currentDynamicPlayerType} -> {newPlayerType}");
                currentDynamicPlayerType = newPlayerType;
            }

            if (statsManager != null)
            {
                statsManager.Refresh();
            }

            if (LevelNPCs.Instance != null)
            LevelNPCs.Instance.RefreshQuestGivers();
        }

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

            // Getting current instance of character from Game.instance
            var characterInstance = Game.instance?.currentCharacter;
            if (characterInstance == null)
            {
                Debug.LogWarning("No character instance found. Skipping log save.");
                return;
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"DDA_{characterInstance.name}.log";
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

                var reps = ReputationManager.Instance?.GetAllReputations();
                if (reps != null)
                {
                    foreach (var kvp in reps)
                    {
                        writer.WriteLine($"{kvp.Key} Reputation: {kvp.Value}");
                    }
                }
            }
            // Debug.Log($"Logs saved to: {filePath}");
        }

        public void LogAchievement(string achievementName)
        {
            if (!unlockedAchievements.Contains(achievementName))
            {
                unlockedAchievements.Add(achievementName);
                // achievementsUnlocked++;  //  OLD - NO LONGER USED

                // Debug.Log($"Achievement unlocked: {achievementName}");

                //  Getting current character
                var characterInstance = Game.instance?.currentCharacter;

                if (characterInstance != null)
                {
                    characterInstance.unlockedAchievements = new List<string>(unlockedAchievements);

                    // Saving achievements to the Game Save system
                    Game.instance.GetComponent<GameSave>().SaveLogsForCharacter(characterInstance);

                    // Debug.Log($"Achievement '{achievementName}' saved for {characterInstance.name}.");
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

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "playerDeaths,enemiesDefeated,totalCombatTime,potionsUsed,zonesDiscovered,npcInteractions,waypointsDiscovered,achievementsUnlocked,currentDynamicPlayerType,difficultyMultiplier,difficultyLevel\n");
            }

            int currentDifficulty = DifficultyManager.Instance.GetCurrentDifficulty();

            string logEntry = $"{playerDeaths},{enemiesDefeated},{totalCombatTime},{potionsUsed},{zonesDiscovered},{npcInteractions},{waypointsDiscovered},{unlockedAchievements.Count},{currentDynamicPlayerType},{difficultyMultiplier},{currentDifficulty}\n";

            File.AppendAllText(filePath, logEntry);

            // Debug.Log("Player data exported to CSV.");  //  DEBUG - Player data saved to CSV
        }

        public void ApplySettingsFromGameSettings()
        {
            if (GameSettings.instance != null)
            {
                isLoggingEnabled = GameSettings.instance.GetSaveLogs();
                // Debug.Log($"PlayerBehaviorLogger: Logging is {(isLoggingEnabled ? "Enabled" : "Disabled")} from settings.");    //  DEBUG - 1:0 of PBL logging
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

            if (Time.time - lastCombatEventTime < combatWindow)
                totalCombatTime += Time.deltaTime;
        }

        public static string FormatPlayTime(float totalSeconds)
        {
            int days = (int)(totalSeconds / 86400); // 1 day = 86400 seconds
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
            // Debug.Log($"AI Agent discovered zone: {zoneName}");
        }

        public void ResetLogs()
        {
            // Debug.Log("Resetting logs...");
            difficultyMultiplier = 0;
            difficultyAdjusted = false;
            // playerDeaths = 0;
            // enemiesDefeated = 0;
            // totalCombatTime = 0f;
            // potionsUsed = 0;
        }

        public void ResetData()
        {
            // Debug.Log("Resetting player behavior data...");

            playerDeaths = 0;
            enemiesDefeated = 0;
            totalCombatTime = 0f;
            potionsUsed = 0;
            questsCompleted = 0;
            zonesDiscovered = 0;
            npcInteractions = 0;
            waypointsDiscovered = 0;
            difficultyMultiplier = 0;

            discoveredZones.Clear();
            discoveredWaypoints.Clear();
            unlockedAchievements.Clear();

            currentDynamicPlayerType = "Unknown";

            lastUpdateTime = Time.time;
            lastPredictionTime = 0f;

            statsManager?.Refresh();

            // Debug.Log("All behavior tracking data has been reset.");
        }
    }
}