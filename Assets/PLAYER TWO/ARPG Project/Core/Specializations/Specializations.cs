using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Character specialization definition:
    /// - Full descriptive data (name, tier, family, icon, tags, description).
    /// - Lists of stat modifiers and skill augments.
    /// - Unique identifier with lookup by ID.
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Specialization",
        menuName = "PLAYER TWO/ARPG Project/Specializations/Specialization")]
    public class Specializations : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique specialization identifier (must be unique across the project).")]
        public int id;

        [Tooltip("Displayed name of the specialization.")]
        public new string name;

        [Tooltip("Tier level of the specialization (e.g., 0 = starter, 1 = advanced).")]
        public int tier;

        [Tooltip("Specialization family, such as class type or archetype.")]
        public string family;

        [Tooltip("Icon representing the specialization in the UI.")]
        public Sprite icon;

        [Tooltip("Optional tags for filtering or conditional logic.")]
        public string[] tags;

        [Header("Effects")]
        [Tooltip("Passive stat modifiers granted by this specialization.")]
        public List<StatModifier> statModifiers = new();

        [Tooltip("Skill augments granted by this specialization.")]
        public List<SkillAugment> activeAugments = new();

        [Header("Description")]
        [Tooltip("Long-form description displayed in tooltips or UI panels.")]
        [TextArea]
        public string description;

        // ===== Static lookup =====
        private static Dictionary<int, Specializations> s_lookup = new Dictionary<int, Specializations>();

        private void OnEnable()
        {
            // Register in the static lookup
            if (!s_lookup.ContainsKey(id))
                s_lookup.Add(id, this);
            else
                s_lookup[id] = this;
        }

        /// <summary>
        /// Finds a specialization by its unique ID.
        /// </summary>
        public static Specializations FindById(int id)
        {
            s_lookup.TryGetValue(id, out var def);
            return def;
        }
    }
}
