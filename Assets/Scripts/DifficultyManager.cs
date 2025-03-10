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
        [SerializeField] private float strengthMultiplier = 0.30f;
        [SerializeField] private float dexterityMultiplier = 0.2f;
        [SerializeField] private float vitalityMultiplier = 0.2f;
        [SerializeField] private float energyMultiplier = 0.10f;
        private float lastUpdateTime = 0f;
        private float minInterval = 2f;
        private float oldGlobalDifficulty = 5f;

        public PlayerBehaviorLogger playerLogger;

        private void Start()
        {
            StartCoroutine(WaitForPlayerLogger());
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

        public void SetDifficultyFromAI(float difficultyLevel)
        {
            if (!useAIDDA) return;
            Debug.Log($"[AI-DDA] Setting difficulty based on AI Prediction: {difficultyLevel}");

            float oldDiff = oldGlobalDifficulty;   // Poprzednia wartość trudności
            float newDiff = difficultyLevel;       // Nowa wartość od RL

            oldGlobalDifficulty = newDiff;

            CurrentDexterityMultiplier = 1.0f + (newDiff * dexterityMultiplier);
            CurrentStrengthMultiplier  = 1.0f + (newDiff * strengthMultiplier);
            CurrentVitalityMultiplier  = 1.0f + (newDiff * vitalityMultiplier);
            CurrentEnergyMultiplier    = 1.0f + (newDiff * energyMultiplier);

            UpdateAllEnemyStats(oldDiff, newDiff);

            // (optional) Level.instance?.ApplyDifficultyToEntities();
        }

        public void UpdateAllEnemyStats(float oldDiff, float newDiff)
        {
            if (Time.time - lastUpdateTime < minInterval)
            {
                Debug.Log("[AI-DDA] Skipping UpdateAllEnemyStats due to cooldown");
                return;
            }
            lastUpdateTime = Time.time;

            bool difficultyChanged    = Mathf.Abs(newDiff - oldDiff) > 0.01f;
            bool difficultyIncreased  = newDiff > oldDiff;
            bool difficultyDecreased  = newDiff < oldDiff;

            var enemies = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                .Where(e => e.CompareTag("Entity/Enemy")
                                        && e.gameObject.layer == LayerMask.NameToLayer("Entities"))
                                .ToList();

            if (enemies.Count == 0)
                return;

            foreach (var enemy in enemies)
            {
                if (enemy.isDead || enemy.stats == null)
                    continue;

                if (enemy.stats.isNewlySpawned)
                {
                    enemy.stats.isNewlySpawned = false;
                    continue;
                }

                int oldStrength  = enemy.stats.strength;
                int oldDexterity = enemy.stats.dexterity;
                int oldVitality  = enemy.stats.vitality;
                int oldEnergy    = enemy.stats.energy;

                enemy.stats.strength  = Mathf.Max(1, (int)(enemy.stats.GetBaseStrength()  * CurrentStrengthMultiplier));
                enemy.stats.dexterity = Mathf.Max(1, (int)(enemy.stats.GetBaseDexterity() * CurrentDexterityMultiplier));
                enemy.stats.vitality  = Mathf.Max(1, (int)(enemy.stats.GetBaseVitality()  * CurrentVitalityMultiplier));
                enemy.stats.energy    = Mathf.Max(1, (int)(enemy.stats.GetBaseEnergy()    * CurrentEnergyMultiplier));

                enemy.stats.Recalculate();

                bool statsIncreased = (enemy.stats.strength  > oldStrength
                                    || enemy.stats.dexterity > oldDexterity
                                    || enemy.stats.vitality > oldVitality
                                    || enemy.stats.energy   > oldEnergy);

                bool statsDecreased = (enemy.stats.strength  < oldStrength
                                    || enemy.stats.dexterity < oldDexterity
                                    || enemy.stats.vitality < oldVitality
                                    || enemy.stats.energy   < oldEnergy);

                if (statsIncreased && statsDecreased)
                {
                    statsIncreased = false;
                    statsDecreased = false;
                }

                if (!statsIncreased && !statsDecreased)
                {
                    continue;
                }

                var entityFeedback = enemy.GetComponent<EntityFeedback>();
                if (entityFeedback != null)
                {
                    // There are four variants for Difficulty Text
                    // a) difficultyChanged + statsIncreased + difficultyIncreased => "Difficulty Increased"
                    // b) difficultyChanged + statsDecreased + difficultyDecreased => "Difficulty Decreased"
                    // c) !difficultyChanged + statsIncreased => "Buff Up"
                    // d) !difficultyChanged + statsDecreased => "Weaken Up"

                    if (difficultyChanged)
                    {
                        // a) DifficultyIncreased
                        if (difficultyIncreased && statsIncreased)
                            entityFeedback.ShowDifficultyChange(DifficultyText.MessageType.DifficultyIncreased);
                        // b) DifficultyDecreased
                        else if (difficultyDecreased && statsDecreased)
                            entityFeedback.ShowDifficultyChange(DifficultyText.MessageType.DifficultyDecreased);
                    }
                    else
                    {
                        // c) BuffUp
                        if (statsIncreased)
                            entityFeedback.ShowDifficultyChange(DifficultyText.MessageType.BuffUp);
                        // d) WeakenUp
                        else if (statsDecreased)
                            entityFeedback.ShowDifficultyChange(DifficultyText.MessageType.WeakenUp);
                    }
                }

                Debug.Log($"[AI-DDA] Updated stats for {enemy.name} => Strength={enemy.stats.strength}, " +
                        $"Dex={enemy.stats.dexterity}, Vitality={enemy.stats.vitality}, Energy={enemy.stats.energy}");
            }
        }

        public void ApplyCurrentDifficultyToNewEnemy(Entity enemy)
        {
            if (enemy == null || enemy.stats == null) return;

            enemy.stats.strength  = Mathf.Max(1, (int)(enemy.stats.GetBaseStrength()  * CurrentStrengthMultiplier));
            enemy.stats.dexterity = Mathf.Max(1, (int)(enemy.stats.GetBaseDexterity() * CurrentDexterityMultiplier));
            enemy.stats.vitality  = Mathf.Max(1, (int)(enemy.stats.GetBaseVitality()  * CurrentVitalityMultiplier));
            enemy.stats.energy    = Mathf.Max(1, (int)(enemy.stats.GetBaseEnergy()    * CurrentEnergyMultiplier));

            enemy.stats.Recalculate();

            Debug.Log($"[AI-DDA] New enemy {enemy.name} got base stats with multipliers (no difficulty text).");
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

            CurrentDexterityMultiplier = character.GetMultiplier("Dexterity");
            CurrentStrengthMultiplier  = character.GetMultiplier("Strength");
            CurrentVitalityMultiplier  = character.GetMultiplier("Vitality");
            CurrentEnergyMultiplier    = character.GetMultiplier("Energy");

            var logger = PlayerBehaviorLogger.Instance;
            if (logger == null)
            {
                Debug.LogWarning("PlayerBehaviorLogger is null, cannot predict difficulty.");
                return;
            }

            float predictedDifficulty = AIModel.Instance.PredictDifficulty(
                logger.playerDeaths,
                logger.enemiesDefeated,
                logger.totalCombatTime,
                logger.potionsUsed
            );

            float oldDiff = oldGlobalDifficulty;
            float newDiff = predictedDifficulty;

            oldGlobalDifficulty = newDiff;

            CurrentDexterityMultiplier = 1.0f + (newDiff * 0.1f);
            CurrentStrengthMultiplier  = 1.0f + (newDiff * 0.15f);
            CurrentVitalityMultiplier  = 1.0f + (newDiff * 0.1f);
            CurrentEnergyMultiplier    = 1.0f + (newDiff * 0.05f);

            // isDifficultyLoaded = true;
            Debug.Log($"Loaded Difficulty for character: {character.name}, AI Predicted: {predictedDifficulty}");
            UpdateAllEnemyStats(oldDiff, newDiff);

            isDifficultyLoaded = true;
        }

        /*
        private void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("[AI-DDA] Manual difficulty adjustment test.");
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
            Debug.Log($"Character assigned: {newLogger.gameObject.name}. Difficulty reset.");
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

            Debug.Log("Difficulty reset to default values.");
        }

        public int GetCurrentDifficulty()
        {
            return Mathf.RoundToInt((CurrentDexterityMultiplier + CurrentStrengthMultiplier + CurrentVitalityMultiplier + CurrentEnergyMultiplier) / 4.0f * 10);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Debug.Log("DifficultyManager instance assigned.");
            }
            else
            {
                Destroy(gameObject);
                // Debug.LogWarning("Duplicate DifficultyManager destroyed.");
            }
        }
    }
}