using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterQuests
    {
        public List<QuestInstance> initialQuests = new List<QuestInstance>();
        public QuestsManager m_quests;

        /// <summary>
        /// Zwraca wszystkie misje; jeśli m_quests jest nieutworzony, zwracamy initialQuests.
        /// </summary>
        public QuestInstance[] currentQuests =>
            m_quests != null ? m_quests.list : initialQuests.ToArray();

        public QuestsManager manager => m_quests;

        /// <summary>
        /// Konstruktor domyślny – nie robi nic szczególnego. Może być używany,
        /// jeśli tworzysz puste CharacterQuests, a questy dodajesz w innym momencie.
        /// </summary>
        public CharacterQuests() { }

        /// <summary>
        /// Konstruktor przyjmujący tablicę QuestInstance – np. tych wczytanych z pliku save.
        /// </summary>
        public CharacterQuests(QuestInstance[] initialQuests)
        {
            this.initialQuests = new List<QuestInstance>(initialQuests);

            if (initialQuests != null && initialQuests.Length > 0)
            {
                m_quests = new QuestsManager();
                m_quests.SetQuests(initialQuests);
            }
        }

        /// <summary>
        /// Inicjalizuje QuestManager, jeśli jeszcze nie istnieje lub nie ma w nim żadnych misji.
        /// W praktyce – jeśli misje zostały już wczytane z pliku (i mamy je w m_quests),
        /// ta metoda nic nie zrobi i nie stworzy nowych QuestInstance.
        /// </summary>
        public virtual void InitializeQuests()
        {
            if (m_quests != null && m_quests.list.Length > 0)
            {
                // Debug.Log("[CharacterQuests] Quests already initialized from save, skipping re-init.");
                return;
            }

            m_quests = new QuestsManager();

            if (initialQuests == null || initialQuests.Count == 0)
                return;

            var instances = initialQuests.Select(q =>
                new QuestInstance(q.data, q.progress, q.completed)
            );

            m_quests.SetQuests(instances.ToArray());
        }

        /// <summary>
        /// Tworzy CharacterQuests z pliku (QuestsSerializer), gdzie QuestInstance wczytane
        /// są z finalTargetProgress, finalCoins, finalExperience. Zwraca obiekt,
        /// który ma już w m_quests finalnie te misje (dzięki konstruktorowi).
        /// </summary>
        public static CharacterQuests CreateFromSerializer(QuestsSerializer serializer)
        {
            var quests = serializer.quests.Select(q =>
            {
                var questScriptable = GameDatabase.instance.FindElementById<Quest>(q.questId);
                if (questScriptable == null)
                {
                    Debug.LogWarning($"Quest with ID {q.questId} not found in GameDatabase!");
                    return null;
                }

                return new QuestInstance(
                    questScriptable,
                    q.progress,
                    q.completed,
                    q.finalTargetProgress,
                    q.finalCoins,
                    q.finalExperience
                );
            })
            .Where(instance => instance != null)
            .ToArray();

            return new CharacterQuests(quests);
        }
    }
}
