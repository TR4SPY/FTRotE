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
        }

        public List<Quest> quests = new List<Quest>();

        public QuestsSerializer(CharacterQuests quests)
        {
            foreach (var quest in quests.currentQuests)
            {
                var id = GameDatabase.instance
                    .GetElementId<ARPGProject.Quest>(quest.data);

                var questData = new Quest()
                {
                    questId = id,
                    progress = quest.progress,
                    completed = quest.completed
                };

                this.quests.Add(questData);
            }
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static QuestsSerializer FromJson(string json) =>
            JsonUtility.FromJson<QuestsSerializer>(json);
    }
}
