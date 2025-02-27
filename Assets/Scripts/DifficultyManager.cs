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
                    Debug.Log("PlayerBehaviorLogger found and assigned.");
                }
                yield return null; 
            }
        }

        public void SetDifficultyFromAI(float difficultyLevel)
        {
            Debug.Log($"[AI-DDA] Setting difficulty based on AI Prediction: {difficultyLevel}");

            CurrentDexterityMultiplier = 1.0f + (difficultyLevel * 0.1f);
            CurrentStrengthMultiplier = 1.0f + (difficultyLevel * 0.15f);
            CurrentVitalityMultiplier = 1.0f + (difficultyLevel * 0.1f);
            CurrentEnergyMultiplier = 1.0f + (difficultyLevel * 0.05f);

            Debug.Log($"[AI-DDA] Current multipliers -> " +
                    $"Dexterity: {CurrentDexterityMultiplier}, " +
                    $"Strength: {CurrentStrengthMultiplier}, " +
                    $"Vitality: {CurrentVitalityMultiplier}, " +
                    $"Energy: {CurrentEnergyMultiplier}");

            UpdateAllEnemyStats();
        }

        public void UpdateAllEnemyStats()
        {
            Debug.Log("[AI-DDA] Updating difficulty for all existing enemies...");

            var enemies = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                .Where(e => e.CompareTag("Entity/Enemy") && e.gameObject.layer == LayerMask.NameToLayer("Entities"))
                                .ToList();

            if (enemies.Count == 0)
            {
                Debug.LogWarning("[AI-DDA] No enemies found to update.");
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy.isDead) 
                {
                    Debug.Log($"[AI-DDA] Skipping difficulty update for {enemy.name} (dead).");
                    continue;
                }

                if (enemy.stats != null)
                {
                    int oldStrength = enemy.stats.strength;
                    int oldDexterity = enemy.stats.dexterity;
                    int oldVitality = enemy.stats.vitality;
                    int oldEnergy = enemy.stats.energy;

                    enemy.stats.strength = Mathf.Max(1, (int)(enemy.stats.GetBaseStrength() * CurrentStrengthMultiplier));
                    enemy.stats.dexterity = Mathf.Max(1, (int)(enemy.stats.GetBaseDexterity() * CurrentDexterityMultiplier));
                    enemy.stats.vitality = Mathf.Max(1, (int)(enemy.stats.GetBaseVitality() * CurrentVitalityMultiplier));
                    enemy.stats.energy = Mathf.Max(1, (int)(enemy.stats.GetBaseEnergy() * CurrentEnergyMultiplier));

                    enemy.stats.Recalculate();

                    bool increased = (enemy.stats.strength > oldStrength || enemy.stats.dexterity > oldDexterity ||
                                    enemy.stats.vitality > oldVitality || enemy.stats.energy > oldEnergy);
                    bool decreased = (enemy.stats.strength < oldStrength || enemy.stats.dexterity < oldDexterity ||
                                    enemy.stats.vitality < oldVitality || enemy.stats.energy < oldEnergy);

                    if (increased && decreased)
                    {
                        increased = false;
                        decreased = false;
                    }

                    EntityFeedback entityFeedback = enemy.GetComponent<EntityFeedback>();
                    if (entityFeedback != null && (increased || decreased))
                    {
                        entityFeedback.ShowDifficultyChange(increased);
                    }

                    Debug.Log($"[AI-DDA] Updated stats for {enemy.name}: Strength={enemy.stats.strength}, Dexterity={enemy.stats.dexterity}, Vitality={enemy.stats.vitality}, Energy={enemy.stats.energy}");
                }
                else
                {
                    Debug.LogWarning($"[AI-DDA] Entity {enemy.name} has no stats. Skipping update.");
                }
            }
        }
        
        public float GetAdjustedDifficulty()
        {
            return RLModel.Instance != null ? RLModel.Instance.AdjustDifficulty(CurrentStrengthMultiplier) : 5f;
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
            CurrentStrengthMultiplier = character.GetMultiplier("Strength");
            CurrentVitalityMultiplier = character.GetMultiplier("Vitality");
            CurrentEnergyMultiplier = character.GetMultiplier("Energy");

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

            SetDifficultyFromAI(predictedDifficulty);

            isDifficultyLoaded = true;
            Debug.Log($"Loaded Difficulty for character: {character.name}, AI Predicted: {predictedDifficulty}");

            UpdateAllEnemyStats();
        }

        private void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("[AI-DDA] Manual difficulty adjustment test.");
            }
        }

        public void ReassignPlayerLogger(PlayerBehaviorLogger newLogger)
        {
            playerLogger = newLogger;

            if (playerLogger != null)
            {
                Debug.Log($"PlayerBehaviorLogger reassigned to: {playerLogger.gameObject.name}");
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
                Debug.Log("DifficultyManager instance assigned.");
            }
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("Duplicate DifficultyManager destroyed.");
            }
        }
    }
}