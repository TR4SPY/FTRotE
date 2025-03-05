using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AI_DDA.Assets.Scripts;
using UnityEngine.InputSystem;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(GUIWindow))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Quest Window")]
    public class GUIQuestWindow : MonoBehaviour
    {
        [Header("Text References")]
        [Tooltip("A reference to the Text component that represents the Quest title.")]
        public Text title;

        [Tooltip("A reference to the Text component that represents the Quest description.")]
        public Text description;

        [Tooltip("A reference to the Text component that represents the Quest objective.")]
        public Text objective;

        [Tooltip("A reference to the Text component that represents the Quest rewards.")]
        public Text rewards;

        [Header("Containers Reference")]
        [Tooltip(
            "A reference to the GameObject that contains the actions from Quest when the window from a giver."
        )]
        public GameObject giverActions;

        [Tooltip(
            "A reference to the GameObject that contains the actions from Quest when the window is from a Log."
        )]
        public GameObject logActions;

        [Header("Actions Reference")]
        [Tooltip("A reference to the Button used to accept the Quest.")]
        public Button accept;

        [Tooltip("A reference to the Button used to decline the Quest.")]
        public Button decline;

        [Tooltip("A reference to the Button used to cancel the Quest.")]
        public Button cancel;

        [Tooltip("A reference to the Button used to manually complete the Quest.")]
        public Button complete;

        public GUITooltip tooltip;

        protected GUIWindow m_window;

        /// <summary>
        /// Returns the GUI Window component associated to this GUI Quest Window.
        /// </summary>
        public GUIWindow window
        {
            get
            {
                if (!m_window)
                    m_window = GetComponent<GUIWindow>();

                return m_window;
            }
        }

        /// <summary>
        /// Returns the Quest this GUI Quest Window represents.
        /// </summary>
        public Quest quest { get; protected set; }

        protected virtual void InitializeCallbacks()
        {
            accept.onClick.AddListener(Accept);
            decline.onClick.AddListener(Decline);
            cancel.onClick.AddListener(Cancel);
            complete.onClick.AddListener(CompleteQuest);
        }

        /// <summary>
        /// Accepts the current Quest.
        /// </summary>
        public virtual void Accept()
        {
            if (!quest)
                return;

            window.Toggle();
            Game.instance.quests.AddQuest(quest);
        }

        /// <summary>
        /// Declines the current Quest.
        /// </summary>
        public virtual void Decline() => window.Toggle();

        /// <summary>
        /// Cancels the current Quest.
        /// </summary>
        public virtual void Cancel()
        {
            if (!quest)
                return;

            window.Toggle();
            Game.instance.quests.RemoveQuest(quest);
        }

        /// <summary>
        /// Sets the current Quest.
        /// </summary>
        /// <param name="quest">The Quest you want to set.</param>
        public virtual void SetQuest(Quest quest)
        {
            if (!quest)
                return;

            this.quest = quest;
            window.Show();
            UpdateTexts();
        }

        public void CompleteQuest()
        {
            if (!quest)
                return;

            if (quest.requiresManualCompletion && !Game.instance.currentCharacter.inventory.HasItem(quest.requiredItem.data))
            {
                Debug.LogWarning("Player no longer has the required item! Cannot complete quest.");
                return;
            }

            Game.instance.quests.CompleteManualQuest(quest);
            window.Toggle();
        }

        protected virtual void UpdateTexts()
        {
            title.text = quest.title;
            description.text = quest.description;

            int baseProgress = quest.targetProgress;
            int finalProgress = quest.GetTargetProgress();
            int progressDifference = finalProgress - baseProgress;

            Color progressColor = GameColors.LightBlue; // 🔵 Domyślnie niebieski (bez zmian)
            if (progressDifference > 0) progressColor = GameColors.LightRed;  // 🔴 Trudniejsze zadanie
            else if (progressDifference < 0) progressColor = GameColors.Green; // 🟢 Łatwiejsze zadanie

            string progressText = finalProgress > 0 ? $" x{StringUtils.StringWithColor(finalProgress.ToString(), progressColor)}" : "";

            objective.text = $"{quest.objective}{progressText}".Trim(); // ✅ Teraz poprawnie wyświetla sam Objective jeśli `progress == 0`

            AttachTooltipsToObjective(); // ✅ Dodajemy tooltip do Objective

            int finalExp = quest.GetTotalExperience();
            int finalGold = quest.GetTotalCoins();

            Color expColor = GameColors.GetMultiplierColor((float)finalExp / (quest.experience != 0 ? quest.experience : 1));
            Color goldColor = GameColors.GetMultiplierColor((float)finalGold / (quest.coins != 0 ? quest.coins : 1));

            string expDisplay = finalExp > 0 ? $"{StringUtils.StringWithColor(finalExp.ToString(), expColor)} Experience\n" : "";
            string goldDisplay = finalGold > 0 ? $"{StringUtils.StringWithColor(finalGold.ToString(), goldColor)} Coins\n" : "";

            rewards.text = $"{expDisplay}{goldDisplay}".Trim(); // ✅ Teraz wyświetla się pod sobą

            AttachTooltipsToRewards();
        }

        private void AttachTooltipsToRewards()
        {
            if (rewards == null || quest == null)
                return;

            int baseExp = quest.experience;
            int finalExp = quest.GetTotalExperience();
            int baseGold = quest.coins;
            int finalGold = quest.GetTotalCoins();

            float expMultiplier = baseExp != 0 ? (float)finalExp / baseExp : 1f;
            float goldMultiplier = baseGold != 0 ? (float)finalGold / baseGold : 1f;

            string expTooltipMessage = TooltipFormatter.FormatRewardText("Experience", baseExp, finalExp, expMultiplier);
            string goldTooltipMessage = TooltipFormatter.FormatRewardText("Coins", baseGold, finalGold, goldMultiplier);
            string itemRewards = TooltipFormatter.GenerateItemRewardsTooltip(quest.items);

            string fullTooltip = "";
            if (!string.IsNullOrEmpty(expTooltipMessage)) fullTooltip += expTooltipMessage + "\n";
            if (!string.IsNullOrEmpty(goldTooltipMessage)) fullTooltip += goldTooltipMessage + "\n";
            if (!string.IsNullOrEmpty(itemRewards)) fullTooltip += itemRewards;
            fullTooltip += "\n" + TooltipFormatter.GetRewardMessage();

            EventTrigger trigger = rewards.gameObject.GetComponent<EventTrigger>() ?? rewards.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) =>
            {
                GUITooltip.instance.ShowTooltip("Quest Rewards\n", fullTooltip, rewards.gameObject);
            });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) => GUITooltip.instance.HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        private void AttachTooltipsToObjective()
        {
            if (objective == null || quest == null)
                return;

            QuestInstance instance;
            if (!Game.instance.quests.TryGetQuest(quest, out instance))
                return;

            int currentProgress = instance.progress;
            int baseTargetProgress = quest.targetProgress;
            int targetProgress = instance.data.GetTargetProgress();

            string objectiveTooltipMessage = TooltipFormatter.FormatObjectiveTooltip(quest.objective, currentProgress, targetProgress, baseTargetProgress);

            EventTrigger trigger = objective.gameObject.GetComponent<EventTrigger>() ?? objective.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) =>
            {
                GUITooltip.instance.ShowTooltip("Quest Objective\n", objectiveTooltipMessage, objective.gameObject);
            });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) => GUITooltip.instance.HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        protected virtual void UpdateButtons()
        {
            if (!quest)
                return;

            var isLog = Game.instance.quests.ContainsQuest(quest);
            logActions.SetActive(isLog);
            giverActions.SetActive(!isLog);

            bool hasRequiredItem = Game.instance.currentCharacter.inventory.HasItem(quest.requiredItem.data);
            complete.gameObject.SetActive(quest.requiresManualCompletion && hasRequiredItem);
        }

        protected virtual void OnInventoryUpdated()
        {
            UpdateButtons();
        }

        protected virtual void Start()
        {
            InitializeCallbacks();
            Game.instance.currentCharacter.inventory.onInventoryUpdated += UpdateButtons;
        }

        protected virtual void OnEnable() => UpdateButtons();
    }
}
