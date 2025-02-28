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
        }

    private bool difficultyAdjusted = false;
    private HashSet<string> discoveredZones = new HashSet<string>();
    private HashSet<int> discoveredWaypoints = new HashSet<int>();
    public AchievementManager achievementManager { get; private set; }

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

            enemiesDefeated++;
            Debug.Log($"{actor} defeated an enemy! Total: {enemiesDefeated}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
            PredictAndApplyDifficulty(); 
            // DifficultyManager.Instance?.UpdateAllEnemyStats();
        }

        public void LogPlayerDeath(Entity entity)
        {
            string actor = GetActor(entity);

            playerDeaths++;
            Debug.Log($"{actor} died! Total deaths: {playerDeaths}");

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
                Debug.Log($"{actor} discovered a new area: {zoneName}");
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
                Debug.Log($"{actor} discovered a new waypoint: {waypointID}");
                UpdatePlayerType();
                achievementManager?.CheckAchievements(this);
            }
        }

        public void LogNpcInteraction(Entity entity)
        {
            string actor = GetActor(entity);
            npcInteractions++;
            Debug.Log($"{actor} interacted with an NPC! Total: {npcInteractions}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
        }

        public void LogQuestCompleted()
        {
            questsCompleted++;
            Debug.Log($"Quest completed! Total: {questsCompleted}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
        }

        public void LogPotionsUsed(Entity entity)
        {
            string actor = GetActor(entity);

            potionsUsed++;
            Debug.Log($"{actor} used a potion! Total: {potionsUsed}");
            UpdatePlayerType();
            achievementManager?.CheckAchievements(this);
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

        public void UpdatePlayerType()
        {
            //  Logic of score system
            int achieverScore = (questsCompleted * 2) + (unlockedAchievements.Count * 2);
            int explorerScore = (zonesDiscovered * 2) + waypointsDiscovered;
            int socializerScore = (npcInteractions * 3) + questsCompleted;
            int killerScore = (int)(enemiesDefeated * 0.5) + (int)(totalCombatTime / 120); 

            //  Determining of Bartle's Player Type
            string newPlayerType = DetermineDominantType(achieverScore, explorerScore, socializerScore, killerScore);

            //  Updating Bartle's Player Type if changed (Dynamically)
            if (newPlayerType != currentDynamicPlayerType)
            {
                Debug.Log($"Player type updated: {currentDynamicPlayerType} -> {newPlayerType}");
                currentDynamicPlayerType = newPlayerType;
            }
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
                // achievementsUnlocked++;  //  OLD - NO LONGER USED

                Debug.Log($"Achievement unlocked: {achievementName}");

                //  Getting current character
                var characterInstance = Game.instance?.currentCharacter;

                if (characterInstance != null)
                {
                    characterInstance.unlockedAchievements = new List<string>(unlockedAchievements);

                    // Saving achievements to the Game Save system
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

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "playerDeaths,enemiesDefeated,totalCombatTime,enemiesAvoided,potionsUsed,zonesDiscovered,npcInteractions,waypointsDiscovered,achievementsUnlocked,currentDynamicPlayerType,difficultyMultiplier,difficultyLevel\n");
            }

            int currentDifficulty = DifficultyManager.Instance.GetCurrentDifficulty();

            string logEntry = $"{playerDeaths},{enemiesDefeated},{totalCombatTime},{potionsUsed},{zonesDiscovered},{npcInteractions},{waypointsDiscovered},{unlockedAchievements.Count},{currentDynamicPlayerType},{difficultyMultiplier},{currentDifficulty}\n";

            File.AppendAllText(filePath, logEntry);

            Debug.Log("Player data exported to CSV.");  //  DEBUG - Player data saved to CSV
        }

        public void ApplySettingsFromGameSettings()
        {
            if (GameSettings.instance != null)
            {
                isLoggingEnabled = GameSettings.instance.GetSaveLogs();
                Debug.Log($"PlayerBehaviorLogger: Logging is {(isLoggingEnabled ? "Enabled" : "Disabled")} from settings.");    //  DEBUG - 1:0 of PBL logging
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

            // achievementManager?.CheckAchievements(this); //  OLD - NO LONGER USED
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
            Debug.Log($"AI Agent discovered zone: {zoneName}");
        }

        public void ResetLogs()
        {
            Debug.Log("Resetting logs...");
            difficultyMultiplier = 0;
            difficultyAdjusted = false;
            // playerDeaths = 0;
            // enemiesDefeated = 0;
            // totalCombatTime = 0f;
            // potionsUsed = 0;
        }
    }
}