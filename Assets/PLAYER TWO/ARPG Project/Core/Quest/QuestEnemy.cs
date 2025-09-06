using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Quest/Quest Enemy")]
    public class QuestEnemy : MonoBehaviour
    {
        [Tooltip("The enemy key that matches the progress key of the Quest.")]
        public string enemyKey;

        protected Entity m_entity;

        protected virtual void InitializeEntity()
        {
            m_entity = GetComponent<Entity>();
            m_entity.onDie.AddListener(AddQuestProgression);
        }

        public virtual void AddQuestProgression()
        {
            var questManager = Game.instance.quests;
            if (questManager == null)
                return;

            questManager.AddProgress(enemyKey);

            foreach (var quest in questManager.list)
            {
                if (
                    (!quest.IsMultiStage() && quest.data.IsProgressKey(enemyKey)) ||
                    (quest.IsMultiStage() &&
                    quest.GetCurrentStage().completingMode == Quest.CompletingMode.Progress &&
                    quest.GetCurrentStage().progressKey == enemyKey)
                )
                {
                    int target = quest.IsMultiStage()
                        ? quest.GetCurrentStage().targetProgress
                        : quest.data.GetTargetProgress();

                    int current = quest.progress;
                    int remaining = target - current;
                }
            }
        }

        protected virtual void Start() => InitializeEntity();
    }
}
