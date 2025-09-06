using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Quest/Quest Item")]
    public class QuestItem : Interactive
    {
        [Header("Quest Item Settings")]
        [Tooltip(
            "If true, this Game Object will stay disabled until the Player accepts the Quest with the progress key of this item."
        )]
        public bool showOnlyWhenQuestIsActive = true;

        [Tooltip("The progress key that matches the Quest's progress key.")]
        public string itemKey;

        protected virtual void InitializeCallbacks()
        {
            var manager = Game.instance.quests;
            if (manager == null)
                return;

            manager.onQuestAdded += _ => HandleActive();
            manager.onQuestRemoved += _ => HandleActive();
        }

        protected virtual void HandleActive()
        {
            if (!showOnlyWhenQuestIsActive)
                return;

            gameObject.SetActive(false);

            if (!interactive)
                return;

            var manager = Game.instance.quests;
            if (manager == null)
                return;

            foreach (var quest in manager.list)
            {
                if (!quest.HasProgress())
                    continue;
                if (!quest.CanAddProgress(itemKey))
                    continue;

                gameObject.SetActive(true);
            }
        }

        protected override void OnInteract(object other)
        {
            if (other is not Entity)
                return;

            gameObject.SetActive(false);
            Game.instance.quests?.AddProgress(itemKey);
        }

        protected override void Start()
        {
            base.Start();
            InitializeCallbacks();
            HandleActive();
        }
    }
}
