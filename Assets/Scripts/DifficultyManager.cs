using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance;

        public float CurrentDexterityMultiplier = 1.0f; // Domyślnie 100%
        public float CurrentStrengthMultiplier = 1.0f; // Domyślnie 100%
        public float CurrentVitalityMultiplier = 1.0f; // Domyślnie 100%
        public float CurrentEnergyMultiplier = 1.0f; // Domyślnie 100%

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
                yield return null; // Czekaj jedną klatkę
            }
        }

        public void AdjustDifficulty(PlayerBehaviorLogger logger)
        {
            if (logger == null)
            {
                Debug.LogError("Logger is null in AdjustDifficulty.");
                return;
            }

            if (logger.playerDeaths >= 3 && logger.playerDeaths % 3 == 0)
            {
                CurrentDexterityMultiplier *= 0.9f;
                CurrentStrengthMultiplier *= 0.8f;
                CurrentVitalityMultiplier *= 0.95f;
                CurrentEnergyMultiplier *= 0.95f; 

                logger.ResetLogs();
                Debug.Log($"Difficulty decreased: Dexterity={CurrentDexterityMultiplier}, Strength={CurrentStrengthMultiplier}, Vitality={CurrentVitalityMultiplier}, Energy={CurrentEnergyMultiplier}");                return;
            }

            if (logger.difficultyMultiplier >= 10 && logger.difficultyMultiplier % 10 == 0)
            {
                CurrentDexterityMultiplier *= 1.1f;
                CurrentStrengthMultiplier *= 1.2f;
                CurrentVitalityMultiplier *= 1.05f;
                CurrentEnergyMultiplier *= 1.05f;

                logger.ResetLogs();
                Debug.Log($"Difficulty increased: Dexterity={CurrentDexterityMultiplier}, Strength={CurrentStrengthMultiplier}, Vitality={CurrentVitalityMultiplier}, Energy={CurrentEnergyMultiplier}");
            }
            else
            {
                Debug.Log("No changes to difficulty.");
            }

            ApplyDifficultyToExistingEnemies();
        }

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
                    entity.stats.strength = Mathf.Max(1, (int)(entity.stats.strength * CurrentStrengthMultiplier));
                    entity.stats.dexterity = Mathf.Max(1, (int)(entity.stats.dexterity * CurrentDexterityMultiplier));
                    entity.stats.vitality = Mathf.Max(1, (int)(entity.stats.vitality * CurrentDexterityMultiplier));
                    entity.stats.energy = Mathf.Max(1, (int)(entity.stats.energy * CurrentDexterityMultiplier));

                    entity.stats.Recalculate(); // Przeliczenie statystyk po zmianie mnożników

                    Debug.Log($"Adjusted stats for {entity.name}: Strength={entity.stats.strength}, Dexterity={entity.stats.dexterity}, Vitality={entity.stats.vitality}, Energy={entity.stats.energy}");
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
            if (isDifficultyLoaded) return; // Unika wielokrotnego ładowania

            if (character == null)
            {
                Debug.LogWarning("No character provided for loading difficulty.");
                return;
            }

            CurrentDexterityMultiplier = character.GetMultiplier("Dexterity");
            CurrentStrengthMultiplier = character.GetMultiplier("Strength");
            CurrentVitalityMultiplier = character.GetMultiplier("Vitality");
            CurrentEnergyMultiplier = character.GetMultiplier("Energy");

            isDifficultyLoaded = true;
            Debug.Log($"Loaded Difficulty for character: {character.name}");
            ApplyDifficultyToExistingEnemies();
        }

        private void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (playerLogger != null)
                {
                    AdjustDifficulty(playerLogger);
                }
                else
                {
                    Debug.LogError("PlayerBehaviorLogger not found!");
                }
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
                DontDestroyOnLoad(gameObject); // Zapewnia, że obiekt nie zostanie zniszczony podczas zmiany scen
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
