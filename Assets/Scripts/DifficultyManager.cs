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
        private float lastRLUpdateTime = 0f;
        private float rlUpdateInterval = 30f; 

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

        /// <summary>
        /// Machine Learning dostarcza przewidywaną trudność.
        /// Reinforcement Learning dynamicznie dostosowuje trudność na podstawie bieżącej rozgrywki.
        /// </summary>
        public void SetDifficultyFromAI(float rlDifficultyAdjustment)
        {
            // Pobranie trudności przewidzianej przez XGBoost
            float predictedDifficulty = AIModel.Instance.PredictDifficulty(PlayerBehaviorLogger.Instance);

            // RL może zmienić trudność maksymalnie o ±1
            float newDifficulty = Mathf.Clamp(predictedDifficulty + rlDifficultyAdjustment, predictedDifficulty - 1f, predictedDifficulty + 1f);

            Debug.Log($"[AI-DDA] Predicted Difficulty (XGBoost): {predictedDifficulty}, RL Adjustment: {rlDifficultyAdjustment}, Final Difficulty: {newDifficulty}");

            // Przekształcenie wartości trudności na mnożniki statystyk
            CurrentDexterityMultiplier = 1.0f + (newDifficulty * 0.1f);
            CurrentStrengthMultiplier = 1.0f + (newDifficulty * 0.15f);
            CurrentVitalityMultiplier = 1.0f + (newDifficulty * 0.1f);
            CurrentEnergyMultiplier = 1.0f + (newDifficulty * 0.05f);

            Debug.Log($"[AI-DDA] Adjusted multipliers -> " +
                    $"Dexterity: {CurrentDexterityMultiplier}, " +
                    $"Strength: {CurrentStrengthMultiplier}, " +
                    $"Vitality: {CurrentVitalityMultiplier}, " +
                    $"Energy: {CurrentEnergyMultiplier}");

            ApplyDifficultyToExistingEnemies();
        }


        /// <summary>
        /// Dynamiczna aktualizacja statystyk przeciwników w oparciu o poziom trudności (ML + RL).
        /// </summary>
        public void ApplyDifficultyToExistingEnemies()
        {
            var entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                .Where(e => e.CompareTag("Entity/Enemy") && e.gameObject.layer == LayerMask.NameToLayer("Entities"))
                                .ToList();

            if (entities.Count == 0)
            {
                Debug.LogWarning("No enemies found to apply difficulty.");
                return;
            }

            foreach (var entity in entities)
            {
                if (entity.stats != null)
                {
                    entity.stats.strength = Mathf.Max(1, (int)(entity.stats.GetBaseStrength() * CurrentStrengthMultiplier));
                    entity.stats.dexterity = Mathf.Max(1, (int)(entity.stats.GetBaseDexterity() * CurrentDexterityMultiplier));
                    entity.stats.vitality = Mathf.Max(1, (int)(entity.stats.GetBaseVitality() * CurrentVitalityMultiplier));
                    entity.stats.energy = Mathf.Max(1, (int)(entity.stats.GetBaseEnergy() * CurrentEnergyMultiplier));

                    entity.stats.Recalculate();

                    Debug.Log($"[AI-DDA] Updated enemy {entity.name}: Strength={entity.stats.strength}, Dexterity={entity.stats.dexterity}, Vitality={entity.stats.vitality}, Energy={entity.stats.energy}");
                }
                else
                {
                    Debug.LogWarning($"Entity {entity.name} has no stats. Skipping difficulty adjustment.");
                }
            }
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

            float predictedDifficulty = AIModel.Instance.PredictDifficulty(PlayerBehaviorLogger.Instance);
            SetDifficultyFromAI(predictedDifficulty);

            isDifficultyLoaded = true;
            Debug.Log($"[AI-DDA] Loaded Difficulty for {character.name}, AI Predicted: {predictedDifficulty}");
            ApplyDifficultyToExistingEnemies();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("[AI-DDA] Manual difficulty adjustment test.");
                AIModel.Instance.SendStatsToRL(PlayerBehaviorLogger.Instance);
            }

            // Automatyczne wysyłanie statystyk do RL co 30 sekund
            if (Time.time - lastRLUpdateTime >= rlUpdateInterval)
            {
                Debug.Log("[AI-DDA] Automatically sending stats to RL for difficulty adjustment.");
                AIModel.Instance.SendStatsToRL(PlayerBehaviorLogger.Instance);
                lastRLUpdateTime = Time.time;
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

            Debug.Log("[AI-DDA] Difficulty reset to default values.");
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
                Debug.Log("[AI-DDA] DifficultyManager instance assigned.");
            }
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("Duplicate DifficultyManager destroyed.");
            }
        }
    }
}
