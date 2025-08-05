using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class QuestInstance
    {
        public Quest data;

        protected int finalTargetProgress;
        protected int finalCoins;
        protected int finalExperience;

        public int FinalTargetProgress => finalTargetProgress;
        public int FinalCoins => finalCoins;
        public int FinalExperience => finalExperience;
        public int currentStageIndex = 0;

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
                m_progress = Mathf.Clamp(value, 0, finalTargetProgress);
            }
        }

        /// <summary>
        /// Returns true if this Quest is completed.
        /// </summary>
        public bool completed { get; protected set; }

        public QuestInstance(Quest data)
        {
            this.data = data;

            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;
            finalTargetProgress = data.GetTargetProgressForPlayerType(playerType);
            finalCoins = data.GetCoinsForPlayerType(playerType);
            finalExperience = data.GetExpForPlayerType(playerType);

            m_progress = 0;
            completed = false;
        }

        public QuestInstance(Quest data, int progress, bool completed)
        {
            this.data = data;
            this.completed = completed;

            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;
            finalTargetProgress = data.GetTargetProgressForPlayerType(playerType);
            finalCoins = data.GetCoinsForPlayerType(playerType);
            finalExperience = data.GetExpForPlayerType(playerType);

            m_progress = Mathf.Clamp(progress, 0, finalTargetProgress);
        }

        public QuestInstance(Quest data, int progress, bool completed,
                            int finalTargetProgress, int finalCoins, int finalExperience)
        {
            this.data = data;
            this.completed = completed;

            this.finalTargetProgress = finalTargetProgress;
            this.finalCoins = finalCoins;
            this.finalExperience = finalExperience;

            m_progress = Mathf.Clamp(progress, 0, finalTargetProgress);
        }

        /// <summary>
        /// Completes the Quest.
        /// </summary>
        public virtual void Complete()
        {
            if (completed) return;
            completed = true;
        }

        public int GetFinalTargetProgress()
        {
            return finalTargetProgress;
        }

        /// <summary>
        /// Returns true if this Quest can be finished by reaching a given scene.
        /// </summary>
        public virtual bool CanCompleteOnScene(string scene) =>
            !completed && data.IsProgress() && data.IsDestinationScene(scene);

        /// <summary>
        /// Returns true if this Quest can add progress with a given progress key.
        /// </summary>
        /*
        public virtual bool CanAddProgress(string key) =>
            !completed && data.IsProgress() && data.IsProgressKey(key);
        */
        public virtual bool CanAddProgress(string key)
        {
            if (completed)
                return false;

            if (IsMultiStage())
            {
                var stage = GetCurrentStage();
                return stage.completingMode == Quest.CompletingMode.Progress &&
                    stage.progressKey == key;
            }

            return data.IsProgress() && data.IsProgressKey(key);
        }

        /// <summary>
        /// Returns true if this Quest can be completed with a trigger.
        /// </summary>
        public virtual bool CanCompleteByTrigger()
        {
            if (completed)
                return false;

            if (IsMultiStage())
            {
                var stage = GetCurrentStage();
                return stage.completingMode == Quest.CompletingMode.Trigger &&
                    !stage.requiresManualCompletion;
            }

            return data.IsTrigger() && !RequiresManualCompletion();
        }

        /// <summary>
        /// Returns true if this Quest is completed by progress.
        /// </summary>
        public virtual bool HasProgress() => data.IsProgress();

        /// <summary>
        /// Returns the formatted progress text.
        /// </summary>
        public virtual string GetProgressText() => $"{progress} / {GetFinalTargetProgress()}";

        /// <summary>
        /// Rewards a given Entity with all the Quest's rewards.
        /// </summary>
        public virtual void Reward(Entity entity)
        {
            if (entity.stats)
                entity.stats.AddExperience(finalExperience);

            if (entity.inventory)
            {
                int coins = finalCoins;
                if (entity.stats)
                    coins = Mathf.RoundToInt(coins * (1f + entity.stats.additionalMoneyRewardPercent / 100f));
                entity.inventory.currency.AddAmberlings(coins);

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
            if (completed) return false;

            if (IsMultiStage() && GetCurrentStage().completingMode == Quest.CompletingMode.FetchAfterKill)
            {
                var stage = GetCurrentStage();

                if (stage.requiresManualCompletion)
                    return false;

                QuestGiver returnNPC = QuestGiver.FindReturnNPC(stage.returnToNPC);
                return PlayerHasItem(stage.requiredItem) && TalkedToNPC(returnNPC);
            }

            if (!data.IsFetchAfterKill()) return false;
            if (RequiresManualCompletion()) return false;

            QuestGiver npc = QuestGiver.FindReturnNPC(data.returnToNPC);
            return PlayerHasItem(data.requiredItem) && TalkedToNPC(npc);
        }

        public bool RequiresManualCompletion()
        {
            return data.requiresManualCompletion;
        }

        private bool PlayerHasItem(QuestItemReward item)
        {
            return Game.instance.currentCharacter.inventory.HasItem(item.data);
        }

        private bool TalkedToNPC(QuestGiver npc)
        {
            return npc != null && npc.state == QuestGiver.State.QuestInProgress;
        }

        public bool IsMultiStage() => data.isMultiStage && data.stages.Count > 0;

        public QuestStage GetCurrentStage()
        {
            if (!IsMultiStage()) return null;
            return data.stages[Mathf.Clamp(currentStageIndex, 0, data.stages.Count - 1)];
        }

        public bool IsFullyCompleted()
        {
            return IsMultiStage()
                ? currentStageIndex >= data.stages.Count
                : completed;
        }

        public void AdvanceStage()
        {
            if (!IsMultiStage()) return;

            currentStageIndex++;

            if (currentStageIndex >= data.stages.Count)
            {
                completed = true;
                Debug.Log($"[Quest] Zakończono quest etapowy: {data.title}");
            }
            else
            {
                Debug.Log($"[Quest] Rozpoczęto etap {currentStageIndex + 1} w queście: {data.title}");
            }
        }
    }
}
