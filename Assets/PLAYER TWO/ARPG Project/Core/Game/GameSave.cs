using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using AI_DDA.Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;

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
        public int lastSavedNPCID = 0;

        protected override void Initialize()
        { 
            base.Initialize();
            DontDestroyOnLoad(gameObject);
        }

        public virtual void Save()
        {
            lastSavedNPCID = Game.instance.lastNPCID;

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
                if (m_currentCharacter.specializations == null)
                    Debug.LogWarning("[GameSave] currentCharacter.specializations is null.");

                SaveDifficultyForCharacter(m_currentCharacter);
                SaveLogsForCharacter(m_currentCharacter);
                SaveLogsIfEnabled();
            }

            foreach (var craftman in GUICraftman.OpenCraftman.ToList())
            {
                craftman.ReturnItemsToPlayerOrDrop();
            }

            CaptureVitalStats();
            if (m_currentCharacter != null)
            {
                var tiers = m_currentCharacter.specializations != null
                    ? string.Join(",", m_currentCharacter.specializations.GetUnlockedTiersInstance())
                    : "None";
                var questCount = m_currentCharacter.quests?.currentQuests?.Length ?? 0;
                Debug.Log($"[GameSave] Saving character '{m_currentCharacter.name}' unlockedSpecializationTiers=[{tiers}] quests={questCount}");
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
            Game.instance.lastNPCID = lastSavedNPCID;

            GameSerializer data = null;
            switch (mode)
            {
                default:
#if !UNITY_WEBGL
                case Mode.Binary:
                    data = LoadBinary();
                    break;
                case Mode.JSON:
                    data = LoadJSON();
                    break;
#endif
                case Mode.PlayerPrefs:
                    data = LoadPlayerPrefs();
                    break;
            }

            if (data != null && m_currentCharacter != null)
            {
                var currentId = GameDatabase.instance.GetElementId<Character>(m_currentCharacter.data);
                var charData = data.characters.FirstOrDefault(c => c.characterId == currentId);
                if (charData != null)
                {
                    var tiers = charData.unlockedSpecializationTiers != null
                        ? string.Join(",", charData.unlockedSpecializationTiers)
                        : "None";
                    var questCount = charData.quests?.quests?.Count ?? 0;
                    Debug.Log($"[GameSave] Loaded character '{m_currentCharacter.name}' unlockedSpecializationTiers=[{tiers}] quests={questCount}");

                    if (charData.bestiaryEntries != null)
                    {
                        m_currentCharacter.bestiary = charData.bestiaryEntries
                            .GroupBy(e => e.enemyId)
                            .ToDictionary(g => g.Key, g => ConvertToBestiaryEntry(g.First()));
                        BestiaryManager.Instance?.SetEntries(m_currentCharacter.bestiary);
                    }

                    if (m_currentCharacter.specializations != null && charData.unlockedSpecializationTiers != null)
                    {
                        m_currentCharacter.specializations.SetUnlockedTiers(charData.unlockedSpecializationTiers);
                        Debug.Log($"[GameSave] Applied specialization tiers to character '{m_currentCharacter.name}'");
                    }
                }
            }

            if (m_currentCharacter?.Entity != null)
                m_currentCharacter.Entity.items.RevalidateEquippedItems();

            return data;
        }
        
        private static BestiaryEntry ConvertToBestiaryEntry(BestiaryEntrySaveData data)
        {
            var prefabList = GameDatabase.instance?.enemies;
            Sprite icon = null;
            string enemyName = $"Enemy_{data.enemyId}";

            if (prefabList != null && data.enemyId >= 0 && data.enemyId < prefabList.Count)
            {
                var prefab = prefabList[data.enemyId];
                if (prefab != null)
                {
                    enemyName = prefab.name;
                    icon = prefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
                }
            }

            return new BestiaryEntry
            {
                enemyId = data.enemyId,
                name = new LocalizedString { TableEntryReference = enemyName },
                description = new LocalizedString { TableEntryReference = $"{enemyName}_Description" },
                icon = icon,
                encounters = data.encounters,
                kills = data.kills
            };
        }

        protected virtual void SaveBinary()
        {
            var path = GetFilePath();
            var data = new GameSerializer(m_game);
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);

            // Debug.Log($"Saved DifficultyManager: Dexterity={data.dexterityMultiplier}, Strength={data.strengthMultiplier}, Vitality={data.vitalityMultiplier}, Energy={data.energyMultiplier}");
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

                        // Debug.Log($"Loaded DifficultyManager: Dexterity={data.dexterityMultiplier}, Strength={data.strengthMultiplier}, Vitality={data.vitalityMultiplier}, Energy={data.energyMultiplier}");
                    }

                    return data;
                }
                catch (SerializationException ex)
                {
                    Debug.LogError($"[GameSave] Serialization error loading game data from '{path}': {ex}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GameSave] Error loading game data from '{path}': {ex}");
                }
            }

            return null;
        }

        public void CaptureVitalStats()
        {
            foreach (var character in Game.instance.characters)
            {
                if (character.Entity != null)
                {
                    character.savedHealth = character.Entity.stats.health;
                    character.savedMana   = character.Entity.stats.mana;
                    var buffManager = character.Entity.GetComponent<EntityBuffManager>();
                    if (buffManager != null)
                        character.buffs = new BuffsSerializer(buffManager);
                }
                else
                {
                    Debug.LogWarning($"[SAVE WARNING] {character.name}: Entity == null → No HP/MP overwrite");
                }
            }
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

            DifficultyManager.Instance.ForceSetRawDifficulty(character.savedDifficulty);

            // Ensure the RL model uses the loaded difficulty as its baseline
            RLModel.Instance?.SetCurrentDifficulty(character.savedDifficulty);
                        
            /*
            Debug.Log($"Loaded Difficulty for {character.name}: " +
                      $"Dexterity={DifficultyManager.Instance.CurrentDexterityMultiplier}, " +
                      $"Strength={DifficultyManager.Instance.CurrentStrengthMultiplier}, " +
                      $"Vitality={DifficultyManager.Instance.CurrentVitalityMultiplier}, " +
                      $"Energy={DifficultyManager.Instance.CurrentEnergyMultiplier}");
            */

            DifficultyManager.Instance.UpdateAllEnemyStats(character.savedDifficulty, character.savedDifficulty);
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

            if (Game.instance.currentCharacter != null)
            {
                Game.instance.currentCharacter.totalPlayTime = character.totalPlayTime;
                Game.instance.currentCharacter.storylineCompleted = character.storylineCompleted;
            }

            PlayerBehaviorLogger.Instance.unlockedAchievements = new List<string>(character.unlockedAchievements);
            int achievementsUnlocked = PlayerBehaviorLogger.Instance.unlockedAchievements.Count;

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
            PlayerBehaviorLogger.Instance.currentDynamicPlayerType = character.currentDynamicPlayerType;

            PlayerBehaviorLogger.Instance.lastUpdateTime = Time.time;

            /*
            Debug.Log($"Loaded Player Behavior Logs for {character.name}: " +
                    $"Deaths={PlayerBehaviorLogger.Instance.playerDeaths}, " +
                    $"Defeated={PlayerBehaviorLogger.Instance.enemiesDefeated}, " +
                    $"CombatTime={PlayerBehaviorLogger.Instance.totalCombatTime}, " +
                    $"NPCInteractions={PlayerBehaviorLogger.Instance.npcInteractions}, " +
                    $"WaypointsDiscovered={PlayerBehaviorLogger.Instance.waypointsDiscovered}, " +
                    $"QuestsCompleted={PlayerBehaviorLogger.Instance.questsCompleted}, " +
                    $"PotionsUsed={PlayerBehaviorLogger.Instance.potionsUsed}, " +
                    $"ZonesDiscovered={PlayerBehaviorLogger.Instance.zonesDiscovered}, " +
                    $"AchievementsUnlocked={achievementsUnlocked}, " +
                    $"PlayerType={QuestionnaireManager.Instance.playerType}, " +
                    $"CurrentDynamicPlayerType={PlayerBehaviorLogger.Instance.currentDynamicPlayerType}, " +
                    $"PlayTime={character.totalPlayTime}");
            */
        
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

            character.savedDifficulty = DifficultyManager.Instance.GetRawDifficulty();

            /* 
            Debug.Log($"Saved Difficulty for {character.name}: " +
                    $"Dexterity={character.GetMultiplier("Dexterity")}, " +
                    $"Strength={character.GetMultiplier("Strength")}, " +
                    $"Vitality={character.GetMultiplier("Vitality")}, " +
                    $"Energy={character.GetMultiplier("Energy")}");
            */
        }

        public void SaveLogsForCharacter(CharacterInstance character)
        {
            if (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger.Instance is null. Skipping player behavior logs save.");
                return;
            }

            character.totalPlayTime += Time.time - PlayerBehaviorLogger.Instance.lastUpdateTime;

            character.playerDeaths = PlayerBehaviorLogger.Instance.playerDeaths;
            character.enemiesDefeated = PlayerBehaviorLogger.Instance.enemiesDefeated;
            character.totalCombatTime = PlayerBehaviorLogger.Instance.totalCombatTime;
            character.npcInteractions = PlayerBehaviorLogger.Instance.npcInteractions;
            character.waypointsDiscovered = PlayerBehaviorLogger.Instance.waypointsDiscovered;
            character.questsCompleted = PlayerBehaviorLogger.Instance.questsCompleted;
            character.potionsUsed = PlayerBehaviorLogger.Instance.potionsUsed;
            character.zonesDiscovered = PlayerBehaviorLogger.Instance.zonesDiscovered;
            character.storylineCompleted = Game.instance.currentCharacter.storylineCompleted;
            
            character.unlockedAchievements = new List<string>(PlayerBehaviorLogger.Instance.unlockedAchievements);
            int achievementsUnlocked = character.unlockedAchievements.Count;

            character.playerType = QuestionnaireManager.Instance?.playerType ?? "Undefined";
            character.currentDynamicPlayerType = PlayerBehaviorLogger.Instance?.currentDynamicPlayerType ?? "Unknown";
            
            if (ReputationManager.Instance != null)
            {
                character.reputation = new Dictionary<Faction, int>(ReputationManager.Instance.GetAllReputations());
            }

            // Debug.Log($"Saved Player Behavior Logs for {character.name} | AchievementsUnlocked={achievementsUnlocked}");
            if (BestiaryManager.Instance != null)
                character.bestiary = new Dictionary<int, BestiaryEntry>(BestiaryManager.Instance.GetEntries());        
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