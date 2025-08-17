using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class QuestsSerializer
    {
        [System.Serializable]
        public class Quest
        {
            public int questId;
            public int progress;
            public bool completed;

            public int finalTargetProgress;
            public int finalCoins;
            public int finalExperience;
        }

        public List<Quest> quests = new List<Quest>();

        public QuestsSerializer(CharacterQuests quests)
        {
            foreach (var questInstance in quests.currentQuests)
            {
                var id = GameDatabase.instance.GetElementId<ARPGProject.Quest>(questInstance.data);

                var questData = new Quest()
                {
                    questId = id,
                    progress = questInstance.progress,
                    completed = questInstance.completed,

                    finalTargetProgress = questInstance.FinalTargetProgress,
                    finalCoins = questInstance.FinalCoins,
                    finalExperience = questInstance.FinalExperience
                };

                this.quests.Add(questData);

                Debug.Log(
                    $"[QuestsSerializer] Quest {id} " +
                    $"({questInstance.data.title}) progress={questInstance.progress} " +
                    $"completed={questInstance.completed}");
            }
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static QuestsSerializer FromJson(string json) =>
            JsonUtility.FromJson<QuestsSerializer>(json);
    }
}
