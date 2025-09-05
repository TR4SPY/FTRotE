using System.Collections.Generic;
using UnityEngine;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class BestiaryManager : MonoBehaviour
    {
        private static BestiaryManager _instance;
        public static BestiaryManager Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<BestiaryManager>();
#else
                    _instance = FindObjectOfType<BestiaryManager>();
#endif
                }
                return _instance;
            }
        }

        private Dictionary<int, BestiaryEntry> _entries = new Dictionary<int, BestiaryEntry>();
        public IReadOnlyDictionary<int, BestiaryEntry> Entries => _entries;

        public void RegisterEncounter(Entity entity)
        {
            if (entity == null) return;
            int id = entity.GetInstanceID();
            if (!_entries.TryGetValue(id, out var entry))
            {
                entry = new BestiaryEntry { enemyId = id };
                _entries[id] = entry;
            }
            entry.encounters++;
        }

        public void RegisterKill(Entity entity)
        {
            if (entity == null) return;
            RegisterEncounter(entity);
            int id = entity.GetInstanceID();
            if (_entries.TryGetValue(id, out var entry))
            {
                entry.kills++;
            }
        }

        public Dictionary<int, BestiaryEntry> GetEntries()
        {
            return _entries;
        }

        public void SetEntries(Dictionary<int, BestiaryEntry> entries)
        {
            _entries = entries ?? new Dictionary<int, BestiaryEntry>();
        }
    }
}
