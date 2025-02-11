using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AI_DDA.Assets.Scripts;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game Save")]
    public class GameSave : Singleton<GameSave>
    {
        public enum Mode { Binary, JSON, PlayerPrefs }

        [Header("Saving Settings")]
        [Tooltip("The version of the save files. If you change this value, old saves become incompatible.")]
        public int saveVersion = 1;

        [Tooltip("The mode in which you want to save the Game data. WebGL builds will fallback to PlayerPrefs.")]
        public Mode mode = Mode.Binary;

        [Tooltip("The name of the file used to store the Game data.")]
        public string fileName = "save";

        protected const string k_jsonExtension = "json";
        protected const string k_binaryExtension = "bin";

        protected Game m_game => Game.instance;
        protected CharacterInstance m_currentCharacter => m_game.currentCharacter;

        public virtual void Save()
        {
            if (Level.instance == null)
            {
                Debug.LogWarning("Level instance is null. Skipping scene data update.");
            }
            else
            {
                UpdateSceneData();
            }

            if (m_currentCharacter != null)
            {
                SaveDifficultyForCharacter(m_currentCharacter);
                SaveLogsForCharacter(m_currentCharacter);
                SaveLogsIfEnabled();
            }

            switch (mode)
            {
                default:
#if !UNITY_WEBGL
                case Mode.Binary:
                    SaveBinary();
                    break;
                case Mode.JSON:
                    SaveJSON();
                    break;
#endif
                case Mode.PlayerPrefs:
                    SavePlayerPrefs();
                    break;
            }
        }

        public virtual GameSerializer Load()
        {
            switch (mode)
            {
                default:
#if !UNITY_WEBGL
                case Mode.Binary:
                    return LoadBinary();
                case Mode.JSON:
                    return LoadJSON();
#endif
                case Mode.PlayerPrefs:
                    return LoadPlayerPrefs();
            }
        }

        protected virtual void SaveBinary()
        {
            var path = GetFilePath();
            var data = new GameSerializer(m_game);
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);

            Debug.Log($"Saved DifficultyManager: Dexterity={data.dexterityMultiplier}, Strength={data.strengthMultiplier}, Vitality={data.vitalityMultiplier}, Energy={data.energyMultiplier}");
        }

        protected virtual GameSerializer LoadBinary()
        {
            var path = GetFilePath();

            if (File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                using var stream = new FileStream(path, FileMode.Open);

                try
                {
                    var data = formatter.Deserialize(stream) as GameSerializer;

                    if (data != null && DifficultyManager.Instance != null)
                    {
                        DifficultyManager.Instance.CurrentDexterityMultiplier = data.dexterityMultiplier;
                        DifficultyManager.Instance.CurrentStrengthMultiplier = data.strengthMultiplier;
                        DifficultyManager.Instance.CurrentVitalityMultiplier = data.vitalityMultiplier;
                        DifficultyManager.Instance.CurrentEnergyMultiplier = data.energyMultiplier;

                        Debug.Log($"Loaded DifficultyManager: Dexterity={data.dexterityMultiplier}, Strength={data.strengthMultiplier}, Vitality={data.vitalityMultiplier}, Energy={data.energyMultiplier}");
                    }

                    return data;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error loading game data: {ex.Message}");
                }
            }

            return null;
        }

        public void LoadDifficultyForCharacter(CharacterInstance character)
        {
            if (DifficultyManager.Instance == null)
            {
                Debug.LogWarning("DifficultyManager.Instance is null. Skipping difficulty load.");
                return;
            }

            DifficultyManager.Instance.CurrentDexterityMultiplier = character.GetMultiplier("Dexterity");
            DifficultyManager.Instance.CurrentStrengthMultiplier = character.GetMultiplier("Strength");
            DifficultyManager.Instance.CurrentVitalityMultiplier = character.GetMultiplier("Vitality");
            DifficultyManager.Instance.CurrentEnergyMultiplier = character.GetMultiplier("Energy");
            Debug.Log($"Loaded Difficulty for {character.name}: " +
                      $"Dexterity={DifficultyManager.Instance.CurrentDexterityMultiplier}, " +
                      $"Strength={DifficultyManager.Instance.CurrentStrengthMultiplier}, " +
                      $"Vitality={DifficultyManager.Instance.CurrentVitalityMultiplier}, " +
                      $"Energy={DifficultyManager.Instance.CurrentEnergyMultiplier}");

            DifficultyManager.Instance.ApplyDifficultyToExistingEnemies();
        }

        public void LoadLogsForCharacter(CharacterInstance character)
        {
            if (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Skipping logs load.");
                return;
            }

            if (character == null)
            {
                Debug.LogWarning("Character is null. Cannot load logs.");
                return;
            }

            if (QuestionnaireManager.Instance != null)
            {
                QuestionnaireManager.Instance.playerType = character.playerType;
                if (QuestionnaireManager.Instance.playerTypeAttributeText != null)
                {
                    QuestionnaireManager.Instance.playerTypeAttributeText.text = character.playerType;
                }
            }

            // Wczytanie całkowitego czasu gry
            if (Game.instance.currentCharacter != null)
            {
                Game.instance.currentCharacter.totalPlayTime = character.totalPlayTime;
            }

            PlayerBehaviorLogger.Instance.unlockedAchievements = new List<string>(character.unlockedAchievements);
            #if UNITY_2023_1_OR_NEWER
            AchievementManager achievementManager = Object.FindFirstObjectByType<AchievementManager>();
            #else
            AchievementManager achievementManager = Object.FindObjectOfType<AchievementManager>();
            #endif

            if (achievementManager != null)
            {
                achievementManager.CheckAchievements(PlayerBehaviorLogger.Instance);
            }


            PlayerBehaviorLogger.Instance.playerDeaths = character.playerDeaths;
            PlayerBehaviorLogger.Instance.enemiesDefeated = character.enemiesDefeated;
            PlayerBehaviorLogger.Instance.totalCombatTime = character.totalCombatTime;
            PlayerBehaviorLogger.Instance.npcInteractions = character.npcInteractions;

            PlayerBehaviorLogger.Instance.waypointsDiscovered = character.waypointsDiscovered;
            PlayerBehaviorLogger.Instance.questsCompleted = character.questsCompleted;
            PlayerBehaviorLogger.Instance.potionsUsed = character.potionsUsed;
            PlayerBehaviorLogger.Instance.zonesDiscovered = character.zonesDiscovered;
            // PlayerBehaviorLogger.Instance.unlockedAchievements = character.unlockedAchievements;
            QuestionnaireManager.Instance.playerType = character.playerType;
            PlayerBehaviorLogger.Instance.currentDynamicPlayerType = character.currentDynamicPlayerType;
            // PlayerBehaviorLogger.Instance.unlockedAchievements = new List<string>(character.unlockedAchievements);

             // Wczytaj łączny czas gry
            PlayerBehaviorLogger.Instance.lastUpdateTime = Time.time; // Inicjalizacja czasu

            Debug.Log($"Loaded Player Behavior Logs for {character.name}: " +
                    $"Deaths={PlayerBehaviorLogger.Instance.playerDeaths}, " +
                    $"Defeated={PlayerBehaviorLogger.Instance.enemiesDefeated}, " +
                    $"CombatTime={PlayerBehaviorLogger.Instance.totalCombatTime}, " +
                    $"NPCInteractions={PlayerBehaviorLogger.Instance.npcInteractions}" +
                    $"WaypointsDiscovered={PlayerBehaviorLogger.Instance.waypointsDiscovered}" +
                    $"QuestsCompleted={PlayerBehaviorLogger.Instance.questsCompleted}" +
                    $"PotionsUsed={PlayerBehaviorLogger.Instance.potionsUsed}" +
                    $"ZonesDiscovered={PlayerBehaviorLogger.Instance.zonesDiscovered}" +
                    // $"AchievementsUnlocked={PlayerBehaviorLogger.Instance.achievementsUnlocked}" +
                    $"PlayerType={QuestionnaireManager.Instance.playerType}" +
                    $"CurrentDynamicPlayerType={PlayerBehaviorLogger.Instance.currentDynamicPlayerType}" +
                    $"PlayTime={character.totalPlayTime}");
        }

        public void SaveDifficultyForCharacter(CharacterInstance character)
        {
            if (DifficultyManager.Instance == null)
            {
                Debug.LogWarning("DifficultyManager.Instance is null. Skipping difficulty save.");
                return;
            }

            character.SetMultiplier("Dexterity", DifficultyManager.Instance.CurrentDexterityMultiplier);
            character.SetMultiplier("Strength", DifficultyManager.Instance.CurrentStrengthMultiplier);
            character.SetMultiplier("Vitality", DifficultyManager.Instance.CurrentVitalityMultiplier);
            character.SetMultiplier("Energy", DifficultyManager.Instance.CurrentEnergyMultiplier);

            Debug.Log($"Saved Difficulty for {character.name}: " +
                    $"Dexterity={character.GetMultiplier("Dexterity")}, " +
                    $"Strength={character.GetMultiplier("Strength")}, " +
                    $"Vitality={character.GetMultiplier("Vitality")}, " +
                    $"Energy={character.GetMultiplier("Energy")}");
        }

        public void SaveLogsForCharacter(CharacterInstance character)
        {
                if (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Skipping player behavior logs save.");
                return;
            }

            // Zapisz całkowity czas gry
            //character.totalPlayTime = Game.instance.currentCharacter.totalPlayTime;
            // Zapisz łączny czas gry
            character.totalPlayTime += Time.time - PlayerBehaviorLogger.Instance.lastUpdateTime;


            character.playerDeaths = PlayerBehaviorLogger.Instance.playerDeaths;
            character.enemiesDefeated = PlayerBehaviorLogger.Instance.enemiesDefeated;
            character.totalCombatTime = PlayerBehaviorLogger.Instance.totalCombatTime;
            character.npcInteractions = PlayerBehaviorLogger.Instance.npcInteractions;
            character.waypointsDiscovered = PlayerBehaviorLogger.Instance.waypointsDiscovered;
            character.questsCompleted = PlayerBehaviorLogger.Instance.questsCompleted;
            character.potionsUsed = PlayerBehaviorLogger.Instance.potionsUsed;
            character.zonesDiscovered = PlayerBehaviorLogger.Instance.zonesDiscovered;
            // character.unlockedAchievements = PlayerBehaviorLogger.Instance.unlockedAchievements;
            // character.playerType = QuestionnaireManager.Instance.playerType;
            character.playerType = QuestionnaireManager.Instance?.playerType ?? "Undefined";
            character.currentDynamicPlayerType = PlayerBehaviorLogger.Instance?.currentDynamicPlayerType ?? "Unknown";
            character.unlockedAchievements = new List<string>(PlayerBehaviorLogger.Instance.unlockedAchievements);

            Debug.Log($"Saved Player Behavior Logs for {character.name}");
        }

        protected virtual void SaveJSON()
        {
            var path = GetFilePath();
            var data = new GameSerializer(m_game);

            File.WriteAllText(path, data.ToJson());
        }

        protected virtual GameSerializer LoadJSON()
        {
            var path = GetFilePath();

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return GameSerializer.FromJson(json);
            }

            return null;
        }

        protected virtual void SavePlayerPrefs()
        {
            var saveKey = GetSaveKey();
            var data = new GameSerializer(m_game).ToJson();
            PlayerPrefs.SetString(saveKey, data);
            PlayerPrefs.Save();
        }

        protected virtual GameSerializer LoadPlayerPrefs()
        {
            var saveKey = GetSaveKey();

            if (PlayerPrefs.HasKey(saveKey))
            {
                var data = PlayerPrefs.GetString(saveKey);
                return GameSerializer.FromJson(data);
            }

            return null;
        }

        protected virtual string GetSaveKey()
        {
            var prefix = Application.isEditor ? "dev_" : "";
            return $"{prefix}{fileName}_{saveVersion}";
        }

        protected virtual string GetFilePath()
        {
            var saveKey = GetSaveKey();
            var extension = mode == Mode.JSON ? k_jsonExtension : k_binaryExtension;
            return Path.Combine(Application.persistentDataPath, $"{saveKey}.{extension}");
        }

        public void SaveLogsIfEnabled()
        {
            if (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Skipping log save.");
                return;
            }

            if (PlayerBehaviorLogger.Instance.isLoggingEnabled)
            {
                PlayerBehaviorLogger.Instance.SaveLogsToFile();
            }
        }

        protected virtual void UpdateSceneData()
        {
            if (Level.instance == null)
            {
                Debug.LogWarning("Level instance is null. Scene data update skipped.");
                return;
            }

            if (m_currentCharacter != null)
            {
                m_currentCharacter.scenes.UpdateScene(Level.instance);
            }
        }
    }
}