using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class QuestInstance
    {
        public Quest data;

        protected int m_progress;

        /// <summary>
        /// The current progress of this Quest Instance.
        /// </summary>
        public int progress
        {
            get { return m_progress; }

            set
            {
                if (!data.IsProgress()) return;

                m_progress = Mathf.Clamp(value, 0, data.targetProgress);
            }
        }

        /// <summary>
        /// Returns true if this Quest is completed.
        /// </summary>
        public bool completed { get; protected set; }

        public QuestInstance(Quest data)
        {
            this.data = data;
        }

        public QuestInstance(Quest data, int progress, bool completed)
        {
            this.data = data;
            this.completed = completed;
            this.progress = progress;
        }

        /// <summary>
        /// Completes the Quest.
        /// </summary>
        public virtual void Complete()
        {
            if (completed) return;

            completed = true;
        }

        /// <summary>
        /// Returns true if this Quest can be finished by reaching a given scene.
        /// </summary>
        /// <param name="scene">The name of the scene you want to check.</param>
        public virtual bool CanCompleteOnScene(string scene) =>
            !completed && data.IsProgress() && data.IsDestinationScene(scene);

        /// <summary>
        /// Returns true if this Quest can add progress with a given progress key.
        /// </summary>
        /// <param name="key">The progress key you want to check.</param>
        public virtual bool CanAddProgress(string key) =>
            !completed && data.IsProgress() && data.IsProgressKey(key);

        /// <summary>
        /// Returns true if this Quest can be completed with a trigger.
        /// </summary>
        public virtual bool CanCompleteByTrigger() => !completed && data.IsTrigger();

        /// <summary>
        /// Returns true if this Quest is completed by progress.
        /// </summary>
        public virtual bool HasProgress() => data.IsProgress();

        /// <summary>
        /// Returns the formatted progress text.
        /// </summary>
        public virtual string GetProgressText() => $"{progress} / {data.targetProgress}";

        /// <summary>
        /// Rewards a given Entity with all the Quest's rewards.
        /// </summary>
        /// <param name="entity">The Entity you want to reward.</param>
        public virtual void Reward(Entity entity)
        {
            if (!entity) return;

            if (entity.stats)
                entity.stats.AddExperience(data.experience);

            if (entity.inventory)
            {
                entity.inventory.instance.money += data.coins;

                foreach (var item in data.items)
                {
                    entity.inventory.instance.TryAddItem(item.CreateItemInstance());
                }
            }
        }

        /// <summary>
        /// Returns true if the FetchAfterKill quest is ready for completion.
        /// </summary>
        public bool CanCompleteFetchAfterKill()
        {
            if (!data.IsFetchAfterKill()) return false;
            if (RequiresManualCompletion()) return false; // Jeśli wymaga manualnego zakończenia, nie kończymy automatycznie.

            QuestGiver returnNPC = QuestGiver.FindReturnNPC(data.returnToNPC);
            return PlayerHasItem(data.requiredItem) && TalkedToNPC(returnNPC);
        }

        public bool RequiresManualCompletion()
        {
            return data.requiresManualCompletion;
        }

        /// <summary>
        /// Checks if the player has the required quest item.
        /// </summary>
        private bool PlayerHasItem(QuestItemReward item)
        {
            return Game.instance.currentCharacter.inventory.HasItem(item.data);
        }

        /// <summary>
        /// Checks if the player has interacted with the required NPC.
        /// </summary>
        private bool TalkedToNPC(QuestGiver npc)
        {
            return npc != null && npc.state == QuestGiver.State.QuestInProgress;
        }
    }
}
