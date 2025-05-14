using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class QuestGiverRegistry : MonoBehaviour
    {
        public static QuestGiverRegistry Instance { get; private set; }

        private readonly Dictionary<string, QuestGiver> m_registry = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Register(QuestGiver giver)
        {
            if (giver != null && !string.IsNullOrEmpty(giver.name))
            {
                if (!m_registry.ContainsKey(giver.name))
                {
                    m_registry.Add(giver.name, giver);
                }
            }
        }

        public void Unregister(QuestGiver giver)
        {
            if (giver != null && m_registry.ContainsKey(giver.name))
            {
                m_registry.Remove(giver.name);
            }
        }

        public QuestGiver Get(string name)
        {
            m_registry.TryGetValue(name, out var giver);
            return giver;
        }

        public IEnumerable<QuestGiver> GetAll() => m_registry.Values;

        public bool Has(string name) => m_registry.ContainsKey(name);
    }
}
