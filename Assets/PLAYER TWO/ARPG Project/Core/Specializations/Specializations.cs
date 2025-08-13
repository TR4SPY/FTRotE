using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
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

        [Tooltip("Background sprite used behind the specialization icon.")]
        public Sprite background;

        [Tooltip("Optional tags for filtering or conditional logic.")]
        public string[] tags;

        [Header("Effects")]
        [Tooltip("Passive stat modifiers granted by this specialization.")]
        public List<StatModifier> statModifiers = new();

        [Tooltip("Skill augments granted by this specialization.")]
        public List<SkillAugment> activeAugments = new();

        [Header("Skills")]
        [Tooltip("Skills that belong to this specialization (used by GUIMasterSkillTree to build the tree).")]
        public List<Skill> skills = new();

        [Header("Description")]
        [Tooltip("Long-form description displayed in tooltips or UI panels.")]
        [TextArea]
        public string description;

        // ===== Static lookup =====
        private static Dictionary<int, Specializations> s_lookup = new Dictionary<int, Specializations>();

        private void OnEnable()
        {
            if (!s_lookup.ContainsKey(id))
                s_lookup.Add(id, this);
            else
                s_lookup[id] = this;
        }

        public static Specializations FindById(int id)
        {
            s_lookup.TryGetValue(id, out var def);
            return def;
        }

        /// <summary>
        /// Returns all loaded specializations that belong to the given family,
        /// sorted by their identifier.
        /// </summary>
        public static List<Specializations> FindByFamily(string family)
        {
            List<Specializations> list = null;
            foreach (var def in s_lookup.Values)
            {
                if (def != null && def.family == family)
                {
                    list ??= new List<Specializations>();
                    list.Add(def);
                }
            }

            if (list != null)
                list.Sort((a, b) => a.id.CompareTo(b.id));

            return list ?? new List<Specializations>();
        }
    }
}
