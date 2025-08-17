using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Kompletny menedżer specjalizacji postaci:
    /// - Komponent Unity (nakłada modyfikatory statystyk i augmenty skilli).
    /// - Obsługa wielu poziomów (tierów) wyboru specjalizacji.
    /// - Punkty specjalizacji (globalne niewydane + per-specjalizacja).
    /// - Respec z kosztem w walucie.
    /// - Snapshot (zapis/odtworzenie) oraz wczytywanie po ID.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/Character/Character Specializations (Full)")]
    public class CharacterSpecializations : MonoBehaviour
    {
        [Header("Progression")]
        [Tooltip("Aktualna, globalna pula niewydanych punktów specjalizacji.")]
        public int unspentSkillPoints;

        [Tooltip("Koszt w amberlings za pełny respec.")]
        public int specializationRespecCost;

        [Header("Runtime Selections & Points")]
        /// <summary>
        /// Aktualne wybory specjalizacji per tier.
        /// </summary>
        public Dictionary<int, Specializations> selected = new Dictionary<int, Specializations>();

        /// <summary>
        /// Zebrane punkty przypisane do konkretnej definicji specjalizacji.
        /// </summary>
        public Dictionary<Specializations, int> skillPoints = new Dictionary<Specializations, int>();

        [Header("Dependencies")]
        [SerializeField] private Currency m_currency;

        protected EntityStatsManager m_stats;
        protected EntitySkillManager m_skills;

        #region Tier unlocking helpers

        private readonly HashSet<int> m_unlockedTiers = new HashSet<int>();
       
        public event Action onTiersChanged;

        public void UnlockTierInstance(int tier)
        {
            if (m_unlockedTiers.Add(tier))
                onTiersChanged?.Invoke();
        }

        public IEnumerable<int> GetUnlockedTiersInstance() => m_unlockedTiers;

        public void ClearUnlockedTiersInstance()
        {
            if (m_unlockedTiers.Count == 0) return;
            m_unlockedTiers.Clear();
            onTiersChanged?.Invoke();
        }

        public void NotifyTierChange() => onTiersChanged?.Invoke();

        public bool IsTierUnlocked(int tier) => tier == 0 || m_unlockedTiers.Contains(tier);

        public static int GetTierUnlockLevel(int tier)
        {
            if (!Game.instance)
                return int.MaxValue;

            switch (tier)
            {
                case 1: return Game.instance.tier1UnlockLevel;
                case 2: return Game.instance.tier2UnlockLevel;
                case 3: return Game.instance.tier3UnlockLevel;
                default: return int.MaxValue;
            }
        }

        #endregion
        
        #region Unity lifecycle

        protected virtual void Awake()
        {
            m_stats  = GetComponent<EntityStatsManager>();
            m_skills = GetComponent<EntitySkillManager>();
            m_unlockedTiers.Add(0);
            ReapplyEffects();
        }

        #endregion

        #region Selection API

        /// <summary>
        /// Wygodna wersja dla pojedynczego „domyślnego” tieru (0) – kompatybilność z Kodem 1.
        /// </summary>
        public void SelectSpecialization(Specializations specialization)
        {
            SelectSpecialization(0, specialization);
        }

        /// <summary>
        /// Ustaw/zmień specjalizację w danym tierze.
        /// </summary>
        public void SelectSpecialization(int tier, Specializations def)
        {
            if (def == null)
                return;

            selected[tier] = def;
            if (!skillPoints.ContainsKey(def))
                skillPoints[def] = 0;

            ReapplyEffects();
        }

        /// <summary>
        /// Pobierz aktualnie wybraną specjalizację w danym tierze.
        /// </summary>
        public Specializations GetSelected(int tier)
        {
            selected.TryGetValue(tier, out var def);
            return def;
        }

        /// <summary>
        /// Pełny reset (bez kosztu waluty). Czyści wybory i punkty per-specjalizacja.
        /// </summary>
        public void ResetAll()
        {
            selected.Clear();
            skillPoints.Clear();
            ReapplyEffects();
        }

        #endregion

        #region Effects application (Kod 1 rozszerzony na wiele speców)

        /// <summary>
        /// Ponownie nakłada modyfikatory statystyk i augmenty umiejętności na podstawie wszystkich aktualnych wyborów.
        /// </summary>
        public void ReapplyEffects()
        {
            // Zbierz modyfikatory i augmenty ze wszystkich wybranych specjalizacji
            List<StatModifier> allMods = null;
            List<SkillAugment> allAugments = null;

            if (selected != null && selected.Count > 0)
            {
                foreach (var kvp in selected)
                {
                    var def = kvp.Value;
                    if (def == null) continue;

                    if (def.statModifiers != null && def.statModifiers.Count > 0)
                    {
                        allMods ??= new List<StatModifier>();
                        allMods.AddRange(def.statModifiers);
                    }

                    if (def.activeAugments != null && def.activeAugments.Count > 0)
                    {
                        allAugments ??= new List<SkillAugment>();
                        allAugments.AddRange(def.activeAugments);
                    }
                }
            }

            // Zastosuj do systemów
            if (m_stats)
            {
                // SetExternalModifiers(null) powinno zdjąć efekty zewnętrzne.
                m_stats.SetExternalModifiers(allMods);
            }

            if (m_skills)
            {
                // ApplySkillAugments(null) powinno zdjąć augmenty.
                m_skills.ApplySkillAugments(allAugments);
            }
        }

        #endregion

        #region Respec (Kod 3)

        /// <summary>
        /// Próbuje wykonać respec:
        /// - pobiera koszt w amberlings (jeśli Currency ustawione),
        /// - zwraca do puli globalnej 1 punkt za każdy zajęty tier,
        /// - czyści wybory i punkty per-specjalizacja,
        /// - ponownie nakłada efekty.
        /// </summary>
        /// <returns>True gdy respec wykonany (i waluta pobrana, jeśli wymagana).</returns>
        public bool Respec()
        {
            // Jeśli mamy currency, egzekwuj koszt
            if (m_currency != null)
            {
                if (m_currency.GetTotalAmberlings() < specializationRespecCost)
                    return false;

                m_currency.RemoveAmberlings(specializationRespecCost);
            }

            // Zwróć punkty za liczbę zajętych tierów
            unspentSkillPoints += selected.Count;

            // Wyczyść
            selected.Clear();
            skillPoints.Clear();

            ReapplyEffects();
            return true;
        }

        /// <summary>
        /// Respec bez kosztu waluty (np. debug / tryb edytora).
        /// </summary>
        public void RespecFree(bool refundPoints = true)
        {
            if (refundPoints)
                unspentSkillPoints += selected.Count;

            selected.Clear();
            skillPoints.Clear();
            ReapplyEffects();
        }

        #endregion

        #region Snapshot (Kod 3 rozszerzony o dane z Kodu 2)

        /// <summary>
        /// Zapisz stan: globalne niewydane punkty, wybory (tier->id) oraz punkty per-specjalizacja (id->punkty).
        /// </summary>
        public SpecializationSnapshot Capture()
        {
            var selectedIds = new Dictionary<int, int>();
            foreach (var kvp in selected)
            {
                if (kvp.Value != null)
                    selectedIds[kvp.Key] = kvp.Value.id; // zakładamy, że Specializations ma pole 'id'
            }

            var pointsById = new Dictionary<int, int>();
            foreach (var kvp in skillPoints)
            {
                if (kvp.Key != null)
                    pointsById[kvp.Key.id] = kvp.Value;
            }

            Debug.Log($"[CharacterSpecializations] Capture - unspentSkillPoints: {unspentSkillPoints}, " +
                      $"selectedIds: {string.Join(", ", selectedIds.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}, " +
                      $"unlockedTiers: {string.Join(", ", m_unlockedTiers)}");


            return new SpecializationSnapshot(
                unspentSkillPoints,
                selectedIds,
                pointsById,
                new List<int>(m_unlockedTiers));
        }

        /// <summary>
        /// Odtwórz stan z snapshotu.
        /// </summary>
        public void Restore(SpecializationSnapshot snap)
        {
            unspentSkillPoints = snap.unspentSkillPoints;

            selected.Clear();
            if (snap.selectedIds != null)
            {
                foreach (var kvp in snap.selectedIds)
                {
                    var def = Specializations.FindById(kvp.Value);
                    if (def != null)
                        selected[kvp.Key] = def;
                }
            }

            skillPoints.Clear();
            if (snap.pointsPerSpecId != null)
            {
                foreach (var kvp in snap.pointsPerSpecId)
                {
                    var def = Specializations.FindById(kvp.Key);
                    if (def != null)
                        skillPoints[def] = kvp.Value;
                }
            }

            m_unlockedTiers.Clear();
            if (snap.unlockedTiers != null)
                m_unlockedTiers.UnionWith(snap.unlockedTiers);

            ReapplyEffects();

            onTiersChanged?.Invoke();

            var restoredSelectedIds = selected.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.id ?? -1);
            Debug.Log($"[CharacterSpecializations] Restore - unspentSkillPoints: {unspentSkillPoints}, " +
                      $"selectedIds: {string.Join(", ", restoredSelectedIds.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}, " +
                      $"unlockedTiers: {string.Join(", ", m_unlockedTiers)}");
        }

        #endregion

        #region Data loading helpers (Kod 2)

        /// <summary>
        /// Wczytaj stan na podstawie surowych ID (jak w Kodzie 2).
        /// </summary>
        public void LoadFromData(Dictionary<int, int> selectedIds, Dictionary<int, int> pointsById)
        {
            selected.Clear();
            if (selectedIds != null)
            {
                foreach (var kvp in selectedIds)
                {
                    // Try resolve specialization definition via static lookup first.
                    var def = Specializations.FindById(kvp.Value);

                    // When loading very early in the session the lookup dictionary
                    // might not yet be populated. In that case fall back to the
                    // GameDatabase which has direct references to all
                    // Specializations assets.
                    if (def == null)
                        def = GameDatabase.instance?.GetSpecializationById(kvp.Value);

                    if (def != null)
                        selected[kvp.Key] = def;
                }
            }

            skillPoints.Clear();
            if (pointsById != null)
            {
                foreach (var kvp in pointsById)
                {
                    var def = Specializations.FindById(kvp.Key) ??
                              GameDatabase.instance?.GetSpecializationById(kvp.Key);

                    if (def != null)
                        skillPoints[def] = kvp.Value;
                }
            }

            ReapplyEffects();
        }

        /// <summary>
        /// Tworzy nowy komponent <see cref="CharacterSpecializations"/> na dedykowanym obiekcie
        /// i zwraca zainicjalizowaną referencję.
        /// </summary>
        public static CharacterSpecializations Create(Currency currency = null, int unspent = 0, int respecCost = 0)
        {
            var go = new GameObject("CharacterSpecializationsRuntime");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var specs = go.AddComponent<CharacterSpecializations>();
            specs.m_currency = currency;
            specs.unspentSkillPoints = unspent;
            specs.specializationRespecCost = respecCost;
            specs.m_unlockedTiers.Add(0);
            return specs;
        }

        /// <summary>
        /// Utwórz nowy komponent w runtime i załaduj dane po ID (wygodny odpowiednik CreateFromData z Kodu 2).
        /// </summary>
        public static CharacterSpecializations CreateFromData(
        Dictionary<int,int> selectedIds,
        Dictionary<int,int> pointsById,
        IEnumerable<int> unlockedTiers,
        Currency currency = null, int unspent = 0, int respecCost = 0)
        {
            var specs = Create(currency, unspent, respecCost);

            if (unlockedTiers != null)
                specs.m_unlockedTiers.UnionWith(unlockedTiers);

            specs.m_unlockedTiers.Add(0);
            specs.LoadFromData(selectedIds, pointsById);
            specs.onTiersChanged?.Invoke();
            return specs;
        }

        #endregion
    }

    /// <summary>
    /// Snapshot serializowalny: niewydane punkty + mapy ID (tier->id specjalizacji, id->punkty).
    /// </summary>
    [System.Serializable]
    public struct SpecializationSnapshot
    {
        public int unspentSkillPoints;
        public Dictionary<int, int> selectedIds;
        public Dictionary<int, int> pointsPerSpecId;
        public List<int> unlockedTiers;

        public SpecializationSnapshot(int unspentSkillPoints,
                                      Dictionary<int, int> selectedIds,
                                      Dictionary<int, int> pointsPerSpecId,
                                      List<int> unlockedTiers)
        {
            this.unspentSkillPoints = unspentSkillPoints;
            this.selectedIds = selectedIds;
            this.pointsPerSpecId = pointsPerSpecId;
            this.unlockedTiers = unlockedTiers;
        }
    }
}