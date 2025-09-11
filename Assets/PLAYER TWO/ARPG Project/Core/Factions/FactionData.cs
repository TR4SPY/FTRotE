using System.Collections.Generic;
using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// Scriptable object holding visual information for factions. An instance
    /// can be placed anywhere under a Resources folder to be picked up at
    /// runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "PLAYER TWO/ARPG Project/Factions/Faction Data")]
    public class FactionData : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public Faction faction;
            public string displayName;
            public Sprite icon;
        }

        public List<Entry> entries = new();

        private Dictionary<Faction, Entry> m_lookup;

        private void OnEnable()
        {
            m_lookup = new Dictionary<Faction, Entry>();
            foreach (var entry in entries)
            {
                if (!m_lookup.ContainsKey(entry.faction))
                    m_lookup.Add(entry.faction, entry);
                else
                    m_lookup[entry.faction] = entry;
            }
        }

        /// <summary>
        /// Retrieves a mapping entry for the given faction.
        /// </summary>
        public Entry Get(Faction faction)
        {
            if (m_lookup == null || m_lookup.Count != entries.Count)
                OnEnable();

            m_lookup.TryGetValue(faction, out var entry);
            return entry;
        }

        private static FactionData s_instance;

        /// <summary>
        /// Loads the FactionData asset from Resources if available.
        /// </summary>
        public static FactionData Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = Resources.Load<FactionData>("FactionData");
                return s_instance;
            }
        }
    }
}
