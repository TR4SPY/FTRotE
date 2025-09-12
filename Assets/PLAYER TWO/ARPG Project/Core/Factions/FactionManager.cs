using System.Collections.Generic;
using UnityEngine;

namespace AI_DDA.Assets.Scripts
{
    /// <summary>
    /// Scene-level component that provides lookup for faction icons. This
    /// allows designers to assign icons directly in the inspector without
    /// requiring a <see cref="FactionData"/> asset.
    /// </summary>
    [AddComponentMenu("PLAYER TWO/ARPG Project/Factions/Faction Manager")]
    public class FactionManager : MonoBehaviour
    {
        [System.Serializable]
        public class FactionIconEntry
        {
            public Faction faction;
            public Sprite icon;
        }

        /// <summary>
        /// List of icon entries exposed to the inspector.
        /// </summary>
        public List<FactionIconEntry> icons = new();

        private Dictionary<Faction, Sprite> m_lookup;

        /// <summary>
        /// Singleton instance of the manager in the scene.
        /// </summary>
        public static FactionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildLookup();
        }

        private void OnValidate()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            m_lookup = new Dictionary<Faction, Sprite>();
            foreach (var entry in icons)
            {
                if (!m_lookup.ContainsKey(entry.faction))
                    m_lookup.Add(entry.faction, entry.icon);
                else
                    m_lookup[entry.faction] = entry.icon;
            }
        }

        /// <summary>
        /// Retrieves the icon associated with the given faction.
        /// </summary>
        public Sprite GetIcon(Faction faction)
        {
            return m_lookup != null && m_lookup.TryGetValue(faction, out var sprite)
                ? sprite
                : null;
        }
    }
}

