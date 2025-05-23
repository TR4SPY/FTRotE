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
        [SerializeField] private float aggroRangeScale   = 0.15f;
        [SerializeField] private float attackSpeedScale = 0.12f;
        [SerializeField] private float comboCountScale = 0.25f;

        private float lastUpdateTime = 0f;
        private float minInterval = 2f;
        private float oldGlobalDifficulty = 5f;

        public float AggroMult              => 1f + (GetRawDifficulty() - 5f) * aggroRangeScale;
        public float AttackSpeedMult        => 1f + (GetRawDifficulty() - 5f) * attackSpeedScale;
        public float ComboMult              => 1f + (GetRawDifficulty() - 5f) * comboCountScale;
        public float StrengthMultiplier     => strengthMultiplier;
        public float DexterityMultiplier    => dexterityMultiplier;
        public float VitalityMultiplier     => vitalityMultiplier;
        public float EnergyMultiplier       => energyMultiplier;

        private Dictionary<Entity, float> lastDifficultyTextTimes = new();
        private readonly Dictionary<int, int> enemyBaseCombos = new();
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

        public event System.Action<float> OnDifficultyChanged;
        private void FireDifficultyChanged(float newDiff) => OnDifficultyChanged?.Invoke(newDiff);
        public void SetDifficultyFromAI(float predictedDifficulty)
        {
            // ---------- CHECKPOINT 0 ----------
            // Debug.Log($"[DM-CHK0]   → Start | pred = {predictedDifficulty:F2}");

            if (isManualOverride)
            {
                // Debug.Log("[DM-CHK1]   → ManualOverride = TRUE  – wychodzę");
                return;
            }

            if (!useAIDDA)
            {
                // Debug.Log("[DM-CHK1b]  → useAIDDA = FALSE – wychodzę");
                return;
            }

            if (playerLogger != null &&
                (playerLogger.totalCombatTime < 30f || playerLogger.enemiesDefeated < 3))
            {
                // Debug.Log($"[DM-CHK2]   → Too few data (time={playerLogger.totalCombatTime:F1}s, kills={playerLogger.enemiesDefeated}) – wychodzę");
                return;
            }

            if (Mathf.Abs(predictedDifficulty - oldGlobalDifficulty) < 0.05f)
            {
                // Debug.Log($"[DM-CHK3]   → Δ < 0.05  (old={oldGlobalDifficulty:F2}) – wychodzę");
                return;
            }


            float smoothed = Mathf.Lerp(oldGlobalDifficulty, predictedDifficulty, 0.20f);
            // Debug.Log($"[DM-UPD]    lerp: {oldGlobalDifficulty:F2} → {predictedDifficulty:F2} = {smoothed:F2}");

            oldGlobalDifficulty = smoothed; 
            FireDifficultyChanged(smoothed);

            float delta = smoothed - 5f;

            CurrentStrengthMultiplier  = 1f + delta * strengthMultiplier;
            CurrentDexterityMultiplier = 1f + delta * dexterityMultiplier;
            CurrentVitalityMultiplier  = 1f + delta * vitalityMultiplier;
            CurrentEnergyMultiplier    = 1f + delta * energyMultiplier;

            //Debug.Log("[DM-CHK4]   → Idę do UpdateAllEnemyStats()");
            UpdateAllEnemyStats(oldGlobalDifficulty, smoothed);
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
                // Debug.Log($"[DM] update {enemy.name} boosted={enemy.stats.wasBoosted}");

                if (enemy == null || enemy.isDead || enemy.stats == null)
                    continue;

                enemy.stats.Initialize();

                if (enemy.stats.isNewlySpawned)
                {
                    enemy.stats.isNewlySpawned = false;
                    ApplyCurrentDifficultyToNewEnemy(enemy);
                    continue;
                }

                int oldStrength = enemy.stats.strength;
                int oldDexterity = enemy.stats.dexterity;
                int oldVitality = enemy.stats.vitality;
                int oldEnergy = enemy.stats.energy;
                int oldMinDamage = enemy.stats.minDamage;
                int oldMaxDamage = enemy.stats.maxDamage;

                float oldHealth = enemy.stats.health;
                int oldMaxHealth = enemy.stats.maxHealth;
                int oldMaxMana = enemy.stats.maxMana;
                int oldMana = enemy.stats.mana;

                if (!enemy.stats.wasBoosted)
                {
                    ApplyCurrentDifficultyToNewEnemy(enemy);
                    continue;
                }

                float difficulty = GetRawDifficulty();

                enemy.stats.strength  = GetCurvedStat(enemy.stats.GetBaseStrength(),  difficulty, 0.25f, 5.0f);
                enemy.stats.dexterity = GetCurvedStat(enemy.stats.GetBaseDexterity(), difficulty, 0.25f, 6.0f);
                enemy.stats.vitality  = GetCurvedStat(enemy.stats.GetBaseVitality(),  difficulty, 0.25f, 6.0f);
                enemy.stats.energy    = GetCurvedStat(enemy.stats.GetBaseEnergy(),    difficulty, 0.25f, 5.0f);

                enemy.stats.Recalculate();
                ScaleEnemyCombos(enemy);

                // Debug.Log($"[DM] {enemy.name} HP={enemy.stats.health} STR={enemy.stats.strength}");

                float healthPercent = oldMaxHealth > 0 ? oldHealth / (float)oldMaxHealth : 1f;
                enemy.stats.health = Mathf.RoundToInt(healthPercent * enemy.stats.maxHealth);

                float manaPercent = oldMaxMana > 0 ? oldMana / (float)oldMaxMana : 1f;
                enemy.stats.mana = Mathf.RoundToInt(manaPercent * enemy.stats.maxMana);

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
            enemy.stats.Revitalize();
            ScaleEnemyCombos(enemy);

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
            if (isManualOverride) return;
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
                FireDifficultyChanged(oldGlobalDifficulty);
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
                FireDifficultyChanged(oldGlobalDifficulty);
            }

            CurrentDexterityMultiplier = 1.0f + (oldGlobalDifficulty * dexterityMultiplier);
            CurrentStrengthMultiplier  = 1.0f + (oldGlobalDifficulty * strengthMultiplier);
            CurrentVitalityMultiplier  = 1.0f + (oldGlobalDifficulty * vitalityMultiplier);
            CurrentEnergyMultiplier    = 1.0f + (oldGlobalDifficulty * energyMultiplier);

            RLModel.Instance?.SetCurrentDifficulty(character.savedDifficulty);

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

        /// <summary>
        /// Skaluje bazową wartość statystyki zgodnie z poziomem trudności.
        /// • difficulty   – zakładamy zakres 1-10 (środek = 5)  
        /// • minFactor    – ile % bazowej wartości ma mieć wróg na bardzo niskim diff (1)  
        /// • maxFactor    – maks. mnożnik statystyki przy diff = 9-10  
        /// • width        – „szerokość” rampy (im większa, tym łagodniej rośnie)  
        /// • curveExp     – krzywizna (1 = linia, 2 = bardziej S-curve)  
        /// </summary>
        public int GetCurvedStat(
                int   baseValue,
                float difficulty,
                float minFactor = 0.50f,   // 50 % bazowej przy diff = 1
                float maxFactor = 4.00f,   // 4× bazowej przy diff = 9-10
                float pivot     = 5f,      // środek krzywej
                float width     = 5f,      // im większa, tym łagodniejszy wzrost
                float curveExp  = 1.3f)    // 1 = linia, 2 = mocna S-curve
        {
            // 1) Normalizujemy diff do [-1, +1] z clampingiem
            float t = Mathf.Clamp((difficulty - pivot) / width, -1f, 1f);

            // 2) Nadajemy „krzywiznę” (|t| ^ curveExp) –- dzięki temu
            //    początek i koniec skali rosną wolniej niż środek
            float k = Mathf.Pow(Mathf.Abs(t), curveExp);

            // 3) Lerp pomiędzy min→1   (dla t < 0)  lub  1→max (dla t ≥ 0)
            float factor = t < 0f
                ? Mathf.Lerp(minFactor, 1f, 1f - k)   // lewa połówka krzywej
                : Mathf.Lerp(1f,        maxFactor, k); // prawa połówka

            return Mathf.RoundToInt(baseValue * factor);
        }

        /// <summary>
        /// Ustawia baseMaxCombos przeciwnika zgodnie z aktualnym ComboMult.
        /// (Zapamiętuje jego pierwotną wartość, żeby skalowanie nie nakładało się wielokrotnie.)
        /// </summary>
        private void ScaleEnemyCombos(Entity enemy)
        {
            if (enemy == null || enemy.stats == null) return;

            int id = enemy.GetInstanceID();

            if (!enemyBaseCombos.TryGetValue(id, out int baseline))
            {
                baseline = enemy.stats.baseMaxCombos;
                enemyBaseCombos[id] = baseline;
            }

            int target = Mathf.Max(1, Mathf.RoundToInt(baseline * ComboMult));

            if (enemy.stats.baseMaxCombos != target)
            {
                enemy.stats.alwaysUseBaseComboStats = true;
                enemy.stats.baseMaxCombos = target;
                enemy.stats.Recalculate();
            }
        }

        public void ResetDifficultyLoad() => isDifficultyLoaded = false;

        public void ForceSetRawDifficulty(float newDiff)
        {
            oldGlobalDifficulty = Mathf.Clamp(newDiff, 1f, 10f);
            FireDifficultyChanged(oldGlobalDifficulty);
        }

        public bool isManualOverride { get; private set; } = false;
        public void ManualSetDifficulty(float val)
        {
            isManualOverride   = true;
            oldGlobalDifficulty = Mathf.Clamp(val, 1f, 10f);
            FireDifficultyChanged(oldGlobalDifficulty);
        }
        public void ClearManualOverride() => isManualOverride = false;

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