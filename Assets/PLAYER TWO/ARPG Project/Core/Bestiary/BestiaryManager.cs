using System.Collections.Generic;
using UnityEngine;
using PLAYERTWO.ARPGProject;
using UnityEngine.Localization;

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

        private int GetEnemyId(Entity entity)
        {
            if (entity == null) return -1;

            var db = GameDatabase.instance;
            if (db == null || db.enemies == null)
                return -1;

            string cleanName = entity.name.Replace("(Clone)", "").Trim();
            for (int i = 0; i < db.enemies.Count; i++)
            {
                var prefab = db.enemies[i];
                if (prefab != null && prefab.name == cleanName)
                    return i;
            }

            return -1;
        }

        public void RegisterEncounter(Entity entity)
        {
            int id = GetEnemyId(entity);
            if (id < 0) return;

            if (!_entries.TryGetValue(id, out var entry))
            {
                var prefab = GameDatabase.instance.enemies[id];
                string enemyName = prefab != null ? prefab.name : entity.name.Replace("(Clone)", "").Trim();

                entry = new BestiaryEntry
                {
                    enemyId = id,
                    name = new LocalizedString { TableEntryReference = enemyName },
                    description = new LocalizedString { TableEntryReference = $"{enemyName}_Description" },
                    icon = prefab?.GetComponentInChildren<SpriteRenderer>()?.sprite
                };
                _entries[id] = entry;
            }

            entry.encounters++;
        }

        public void RegisterKill(Entity entity)
        {
            int id = GetEnemyId(entity);
            if (id < 0) return;

            RegisterEncounter(entity);

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
