using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance;

        public float CurrentDexterityMultiplier = 1.0f;
        public float CurrentStrengthMultiplier = 1.0f;
        public float CurrentVitalityMultiplier = 1.0f;
        public float CurrentEnergyMultiplier = 1.0f;

        [Header("Multipliers for Difficulty Scaling")]
        [SerializeField] private float strengthMultiplier  = 0.8f;
        [SerializeField] private float dexterityMultiplier = 1.0f;
        [SerializeField] private float vitalityMultiplier  = 1.0f;
        [SerializeField] private float energyMultiplier    = 0.8f;
        
        private float lastUpdateTime = 0f;
        private float minInterval = 2f;
        private float oldGlobalDifficulty = 5f;


        public float StrengthMultiplier => strengthMultiplier;
        public float DexterityMultiplier => dexterityMultiplier;
        public float VitalityMultiplier => vitalityMultiplier;
        public float EnergyMultiplier => energyMultiplier;

        private Dictionary<Entity, float> lastDifficultyTextTimes = new();
        private float textCooldown = 5f;
        private Dictionary<Entity, DifficultyText.MessageType> lastTextTypes = new();


        public PlayerBehaviorLogger playerLogger;

        private void Start()
        {
            StartCoroutine(WaitForPlayerLogger());

            if (!isDifficultyLoaded)
            {
                oldGlobalDifficulty = 5.0f;

                CurrentDexterityMultiplier = 1.0f + (oldGlobalDifficulty * dexterityMultiplier);
                CurrentStrengthMultiplier  = 1.0f + (oldGlobalDifficulty * strengthMultiplier);
                CurrentVitalityMultiplier  = 1.0f + (oldGlobalDifficulty * vitalityMultiplier);
                CurrentEnergyMultiplier    = 1.0f + (oldGlobalDifficulty * energyMultiplier);

                // Debug.Log("[Diff-Manager] Default difficulty set to 5.0 in Start().");
            }
        }

        private IEnumerator WaitForPlayerLogger()
        {
            while (playerLogger == null)
            {
                var newLogger = FindFirstObjectByType<PlayerBehaviorLogger>();
                if (newLogger != null)
                {
                    ReassignPlayerLogger(newLogger);
                    // Debug.Log("PlayerBehaviorLogger found and assigned.");
                }
                yield return null; 
            }
        }

        public bool useAIDDA = true;

        public void SetDifficultyFromAI(float predictedDifficulty)
        {
            if (!useAIDDA) return;

            if (playerLogger != null && (playerLogger.totalCombatTime < 30f || playerLogger.enemiesDefeated < 3))
            {
                // Debug.Log("[Diff-Manager] Too little data, skipping AI difficulty update.");
                return;
            }

            float smoothedDifficulty = Mathf.Lerp(oldGlobalDifficulty, predictedDifficulty, 0.2f);
            // Debug.Log($"[Diff-Manager] Adjusting difficulty with LERP: Old={oldGlobalDifficulty:F2}, New={predictedDifficulty:F2}, Smoothed={smoothedDifficulty:F2}");

            oldGlobalDifficulty = smoothedDifficulty;
            float delta = smoothedDifficulty - 5.0f;

            CurrentStrengthMultiplier  = 1.0f + (delta * strengthMultiplier);
            CurrentDexterityMultiplier = 1.0f + (delta * dexterityMultiplier);
            CurrentVitalityMultiplier  = 1.0f + (delta * vitalityMultiplier);
            CurrentEnergyMultiplier    = 1.0f + (delta * energyMultiplier);

            UpdateAllEnemyStats(oldGlobalDifficulty, smoothedDifficulty);

            // (optional) Level.instance?.ApplyDifficultyToEntities();
        }

        public void UpdateAllEnemyStats(float oldDiff, float newDiff)
        {
            if (Time.time - lastUpdateTime < minInterval)
            {
                // Debug.Log("[Diff-Manager] Skipping UpdateAllEnemyStats due to cooldown");
                return;
            }

            lastUpdateTime = Time.time;

            bool difficultyChanged = Mathf.Abs(newDiff - oldDiff) > 0.001f;
            bool difficultyIncreased = newDiff > oldDiff;
            bool difficultyDecreased = newDiff < oldDiff;

            var enemies = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(e => e.CompareTag("Entity/Enemy") && e.gameObject.layer == LayerMask.NameToLayer("Entities"))
                .ToList();

            if (enemies.Count == 0) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.isDead || enemy.stats == null)
                    continue;

                enemy.stats.Initialize();

                if (enemy.stats.isNewlySpawned)
                {
                    enemy.stats.isNewlySpawned = false;
                    continue;
                }

                int oldStrength = enemy.stats.strength;
                int oldDexterity = enemy.stats.dexterity;
                int oldVitality = enemy.stats.vitality;
                int oldEnergy = enemy.stats.energy;
                int oldMinDamage = enemy.stats.minDamage;
                int oldMaxDamage = enemy.stats.maxDamage;


                if (!enemy.stats.wasBoosted)
                {
                    Debug.LogWarning($"[Diff-Manager] {enemy.name} (ID={enemy.GetInstanceID()}) nie był boostowany – pomijam UpdateAllEnemyStats dla bezpieczeństwa.");
                    continue;
                }

                float difficulty = GetRawDifficulty();

                enemy.stats.strength  = GetCurvedStat(enemy.stats.GetBaseStrength(),  difficulty, 0.25f, 5.0f);
                enemy.stats.dexterity = GetCurvedStat(enemy.stats.GetBaseDexterity(), difficulty, 0.25f, 6.0f);
                enemy.stats.vitality  = GetCurvedStat(enemy.stats.GetBaseVitality(),  difficulty, 0.25f, 6.0f);
                enemy.stats.energy    = GetCurvedStat(enemy.stats.GetBaseEnergy(),    difficulty, 0.25f, 5.0f);

                enemy.stats.Recalculate();

                int newMinDamage = enemy.stats.minDamage;
                int newMaxDamage = enemy.stats.maxDamage;

                bool damageIncreased = newMinDamage > oldMinDamage && newMaxDamage > oldMaxDamage;
                bool damageDecreased = newMinDamage < oldMinDamage && newMaxDamage < oldMaxDamage;

                bool statsIncreased = enemy.stats.strength > oldStrength
                                || enemy.stats.dexterity > oldDexterity
                                || enemy.stats.vitality > oldVitality
                                || enemy.stats.energy   > oldEnergy;

                bool statsDecreased = enemy.stats.strength < oldStrength
                                || enemy.stats.dexterity < oldDexterity
                                || enemy.stats.vitality < oldVitality
                                || enemy.stats.energy   < oldEnergy;

                if (difficultyChanged && (statsIncreased || damageIncreased))
                {
                    TryShowText(enemy, DifficultyText.MessageType.DifficultyIncreased);
                }
                else if (difficultyChanged && (statsDecreased || damageDecreased))
                {
                    TryShowText(enemy, DifficultyText.MessageType.DifficultyDecreased);
                }
                else if (!difficultyChanged)
                {
                    if (statsIncreased)
                        TryShowText(enemy, DifficultyText.MessageType.BuffUp);
                    else if (statsDecreased)
                        TryShowText(enemy, DifficultyText.MessageType.WeakenUp);
                }

                // Debug.Log($"[DEBUG] DiffChanged: {difficultyChanged}, Stats↑: {statsIncreased}, Stats↓: {statsDecreased}, DMG↑: {damageIncreased}, DMG↓: {damageDecreased}");
                // Debug.Log($"[DEBUG] STR: {oldStrength} → {enemy.stats.strength}, DEX: {oldDexterity} → {enemy.stats.dexterity}, VIT: {oldVitality} → {enemy.stats.vitality}, ENE: {oldEnergy} → {enemy.stats.energy}");
                // Debug.Log($"[DEBUG] DMG: {oldMinDamage}-{oldMaxDamage} → {enemy.stats.minDamage}-{enemy.stats.maxDamage}");
                // Debug.Log($"[DEBUG] Difficulty: Old={oldDiff}, New={newDiff}");

                // Debug.Log($"[Diff-Manager] {enemy.name} stats updated. STR={enemy.stats.strength}, DEX={enemy.stats.dexterity}, VIT={enemy.stats.vitality}, ENE={enemy.stats.energy}, DMG={enemy.stats.minDamage}-{enemy.stats.maxDamage}");
            }
        }

        public void ApplyCurrentDifficultyToNewEnemy(Entity enemy)
        {
            if (enemy == null || enemy.stats == null) return;

            enemy.stats.Initialize();

            if (enemy.stats.baseStrength <= 0)
            {
                Debug.LogWarning($"[Diff-Manager] Skipping {enemy.name} — baseStrength not initialized (currently: {enemy.stats.baseStrength})");
                return;
            }

            if (enemy.stats.wasBoosted)
            {
                // Debug.Log($"[Diff-Manager] Skipping boost: {enemy.name} was already boosted.");
                return;
            }

            float difficulty = GetRawDifficulty();

            enemy.stats.strength  = GetCurvedStat(enemy.stats.GetBaseStrength(),  difficulty, 0.25f, 5.0f);
            enemy.stats.dexterity = GetCurvedStat(enemy.stats.GetBaseDexterity(), difficulty, 0.25f, 6.0f);
            enemy.stats.vitality  = GetCurvedStat(enemy.stats.GetBaseVitality(),  difficulty, 0.25f, 6.0f);
            enemy.stats.energy    = GetCurvedStat(enemy.stats.GetBaseEnergy(),    difficulty, 0.25f, 5.0f);

            enemy.stats.Recalculate();
            enemy.stats.wasBoosted = true;

            // Debug.Log($"[Diff-Manager] New enemy {enemy.name} got base stats with multipliers.");
        }

        private void TryShowText(Entity enemy, DifficultyText.MessageType type)
        {
            if (!lastDifficultyTextTimes.ContainsKey(enemy))
            {
                lastDifficultyTextTimes[enemy] = 0f;
                lastTextTypes[enemy] = type;
            }

            float lastTime = lastDifficultyTextTimes[enemy];
            DifficultyText.MessageType lastType = lastTextTypes[enemy];

            if (Time.time - lastTime < textCooldown && lastType == type)
                return;

            lastDifficultyTextTimes[enemy] = Time.time;
            lastTextTypes[enemy] = type;

            enemy.GetComponent<EntityFeedback>()?.ShowDifficultyChange(type);
        }

        public float GetAdjustedDifficulty()
        {
            return RLModel.Instance != null ? RLModel.Instance.GetCurrentDifficulty() : 5f;
        }

        private bool isDifficultyLoaded = false;

        public void LoadDifficultyForCharacter(CharacterInstance character)
        {
            if (isDifficultyLoaded) return;
            if (character == null)
            {
                Debug.LogWarning("No character provided for loading difficulty.");
                return;
            }

            var logger = PlayerBehaviorLogger.Instance;
            if (logger == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger is null, cannot predict difficulty.");
                return;
            }

            if (logger.totalCombatTime < 30f || logger.enemiesDefeated < 3)
            {
                // Debug.Log("[Diff-Manager] Not enough data on load – default difficulty set to 5.0");
                oldGlobalDifficulty = 5.0f;
            }
            else
            {
                float predicted = AIModel.Instance.PredictDifficulty(
                    logger.playerDeaths,
                    logger.enemiesDefeated,
                    logger.totalCombatTime,
                    logger.potionsUsed
                );

               // Debug.Log($"[Diff-Manager] Loaded difficulty prediction for character: {predicted}");
                oldGlobalDifficulty = Mathf.Clamp(predicted, 3.0f, 10.0f);
            }

            CurrentDexterityMultiplier = 1.0f + (oldGlobalDifficulty * dexterityMultiplier);
            CurrentStrengthMultiplier  = 1.0f + (oldGlobalDifficulty * strengthMultiplier);
            CurrentVitalityMultiplier  = 1.0f + (oldGlobalDifficulty * vitalityMultiplier);
            CurrentEnergyMultiplier    = 1.0f + (oldGlobalDifficulty * energyMultiplier);

            UpdateAllEnemyStats(oldGlobalDifficulty, oldGlobalDifficulty);

            isDifficultyLoaded = true;
        }

        /*
        private void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("[Diff-Manager] Manual difficulty adjustment test.");
            }
        }
        */

        public void ReassignPlayerLogger(PlayerBehaviorLogger newLogger)
        {
            playerLogger = newLogger;

            if (playerLogger != null)
            {
                // Debug.Log($"PlayerBehaviorLogger reassigned to: {playerLogger.gameObject.name}");
            }
            else
            {
                Debug.LogError("Failed to reassign PlayerBehaviorLogger!");
            }
        }

        public void AssignCharacter(PlayerBehaviorLogger newLogger)
        {
            playerLogger = newLogger;
            ResetDifficulty();
           // Debug.Log($"Character assigned: {newLogger.gameObject.name}. Difficulty reset.");
        }

        public void ResetDifficulty()
        {
            CurrentDexterityMultiplier = 1.0f;
            CurrentStrengthMultiplier = 1.0f;
            CurrentVitalityMultiplier = 1.0f;
            CurrentEnergyMultiplier = 1.0f;

            if (playerLogger != null)
            {
                playerLogger.ResetLogs();
            }

           // Debug.Log("Difficulty reset to default values.");
        }

        public int GetCurrentDifficulty()
        {
            return Mathf.RoundToInt((CurrentDexterityMultiplier + CurrentStrengthMultiplier + CurrentVitalityMultiplier + CurrentEnergyMultiplier) / 4.0f * 10);
        }

        public float GetRawDifficulty() => oldGlobalDifficulty;

        public int GetCurvedStat(int baseValue, float difficulty, float minFactor, float maxFactor)
        {
            float t = (difficulty - 5f) / 4f; // [-1, +1] gdzie 5 = środek
            float factor;

            if (t < 0f)
            {
                factor = Mathf.Lerp(minFactor, 1.0f, 1.0f + t); 
            }
            else
            {
                factor = Mathf.Lerp(1.0f, maxFactor, t);
            }

            return Mathf.RoundToInt(baseValue * factor);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}