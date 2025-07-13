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

        [Tooltip("A reference to the Container containing both rewards and currency rewards.")]
        public Transform rewards;

        [Tooltip("Shown instead of the Cancel button when the quest is complete.")]
        public Text completeText;

        [Header("Containers")]
        [Tooltip("Container for displaying the reward entries dynamically.")]
        public Transform rewardsContainer;

        [Tooltip("Container dla ikon monet w Quest Window.")]
        public Transform currencyContainer;

        [Header("Prefabs")]
        [Tooltip("Prefab z childami Name(Text) i Icon(Image).")]
        public GameObject priceTagPrefab;
        
        [Tooltip("Prefab z childami Name(Text).")]
        public GameObject textLinePrefab;

        [Tooltip("Prefab for header lines.")]
        public GameObject textLineHeaderPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        [Header("Containers Reference")]
        [Tooltip("A reference to the GameObject that contains the actions from Quest when the window from a giver.")]
        public GameObject giverActions;

        [Tooltip("A reference to the GameObject that contains the actions from Quest when the window is from a Log.")]
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
            bool isAccepted = Game.instance.quests.TryGetQuest(quest, out var questInst);

            title.text = quest.title;
            description.text = quest.description;

            int baseProgress = quest.targetProgress;
            int finalProgress = isAccepted
                ? questInst.GetFinalTargetProgress()
                : quest.GetTargetProgress();

            int progressDifference = finalProgress - baseProgress;

            Color progressColor = GameColors.LightBlue;
            if (progressDifference > 0) progressColor = GameColors.LightRed;
            else if (progressDifference < 0) progressColor = GameColors.Green;

            string progressText = finalProgress > 0
                ? $" x{StringUtils.StringWithColor(finalProgress.ToString(), progressColor)}"
                : "";

            objective.text = $"{quest.objective}{progressText}".Trim();

            ClearContainer(rewardsContainer);
            ClearContainer(currencyContainer);

            ToggleContainerActive(rewardsContainer, false);
            ToggleContainerActive(currencyContainer, false);

            int finalExp, finalCoins;

            if (!isAccepted)
            {
                finalExp = quest.GetTotalExperience();
                finalCoins = quest.GetTotalCoins();
            }
            else
            {
                finalExp = questInst.FinalExperience;
                finalCoins = questInst.FinalCoins;
            }

            bool hasCurrencyRewards = finalCoins > 0;
            bool hasItemRewards = finalExp > 0 || (quest.items != null && quest.items.Length > 0);

            ToggleContainerActive(currencyContainer, true);

            if (finalExp > 0)
            {
                AddLine($"{finalExp} XP");
                ToggleContainerActive(rewardsContainer, true);
            }

            var c = new Currency();
            c.SetFromTotalAmberlings(finalCoins);

            if (c.solmire > 0)
                AddPriceTag(currencyContainer, c.solmire, solmireIcon);
            if (c.lunaris > 0)
                AddPriceTag(currencyContainer, c.lunaris, lunarisIcon);
            if (c.amberlings > 0)
                AddPriceTag(currencyContainer, c.amberlings, amberlingsIcon);

            if (c.solmire == 0 && c.lunaris == 0 && c.amberlings == 0)
                AddPriceTag(currencyContainer, 0, amberlingsIcon);

            if (quest.items != null && quest.items.Length > 0)
            {
                foreach (var item in quest.items)
                {
                    if (item?.data != null && item.amount > 0)
                    {
                        AddLine($"{item.amount}x {item.data.name}");
                        ToggleContainerActive(rewardsContainer, true);
                    }
                }
            }

            AttachTooltipsToObjective();
            AttachTooltipsToRewards();
        }
        private void ClearContainer(Transform container)
        {
            if (!container) return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private void AddLine(string text)
        {
            if (!rewardsContainer || !textLinePrefab) return;

            var go = Instantiate(textLinePrefab, rewardsContainer);
            var txt = go.transform.Find("Text")?.GetComponent<Text>();
            if (txt) txt.text = text;
        }

        private void AddPriceTag(Transform parent, int amount, Sprite icon)
        {
            if (!parent || !priceTagPrefab) return;

            var go = Instantiate(priceTagPrefab, parent);
            var txt = go.transform.Find("Name")?.GetComponent<Text>();
            if (txt) txt.text = amount.ToString();

            var img = go.transform.Find("Icon")?.GetComponent<Image>();
            if (img && icon) img.sprite = icon;
        }

        private void AddHeader(Transform container, string text)
        {
            if (!container || !textLineHeaderPrefab) return;

            var go = Instantiate(textLineHeaderPrefab, container);
            var txt = go.transform.Find("Text")?.GetComponent<Text>();
            if (txt) 
            {
                txt.text = text;
                txt.fontStyle = FontStyle.Bold;
            }
        }

        private void TryAddCurrencyHeader(int finalCoins)
        {
            if (finalCoins > 0)
            {
                AddHeader(currencyContainer, "Rewards:");
            }
        }

        private void ToggleContainerActive(Transform container, bool shouldBeActive)
        {
            if (container && container.gameObject.activeSelf != shouldBeActive)
            {
                container.gameObject.SetActive(shouldBeActive);
            }
        }

        private void AttachTooltipsToRewards()
        {
            if (rewards == null || quest == null)
                return;

            bool isAccepted = Game.instance.quests.TryGetQuest(quest, out var instance);

            int baseExp = quest.experience;
            int finalExp = isAccepted ? instance.FinalExperience : quest.GetTotalExperience();
            float expMultiplier = baseExp != 0 ? (float)finalExp / baseExp : 1f;

            string expText = "";
            if (finalExp > 0)
            {
                expText = $"{finalExp} Experience";
                expText += $" ({baseExp} base {TooltipFormatter.FormatColoredMultiplier(expMultiplier)})\n";
            }

            int baseCoins = quest.coins;
            int finalCoins = isAccepted ? instance.FinalCoins : quest.GetTotalCoins();
            float coinsMultiplier = baseCoins != 0 ? (float)finalCoins / baseCoins : 1f;

            string coinsText = "";
            if (finalCoins > 0)
            {
                var currency = new Currency();
                currency.SetFromTotalAmberlings(finalCoins);

                var baseCurrency = new Currency();
                baseCurrency.SetFromTotalAmberlings(baseCoins);

                string currencyBreakdown = "";
                if (currency.solmire > 0) currencyBreakdown += $"{currency.solmire} Solmire ";
                if (currency.lunaris > 0) currencyBreakdown += $"{currency.lunaris} Lunaris ";
                if (currency.amberlings > 0) currencyBreakdown += $"{currency.amberlings} Amberlings";

                string baseBreakdown = "";
                if (baseCurrency.solmire > 0) baseBreakdown += $"{baseCurrency.solmire} Solmire ";
                if (baseCurrency.lunaris > 0) baseBreakdown += $"{baseCurrency.lunaris} Lunaris ";
                if (baseCurrency.amberlings > 0) baseBreakdown += $"{baseCurrency.amberlings} Amberlings";

                coinsText = $"{currencyBreakdown.Trim()} ({baseBreakdown.Trim()} base {TooltipFormatter.FormatColoredMultiplier(coinsMultiplier)})\n";
            }

            string itemsText = "";
            if (quest.items != null && quest.items.Length > 0)
            {
                //itemsText = "Item Rewards:\n";
                foreach (var item in quest.items)
                {
                    if (item?.data != null && item.amount > 0)
                    {
                        Color itemColor;

                        if (item.data.IsQuestSpecific)
                        {
                            itemColor = GameColors.Gold;
                        }
                        else
                        {
                            itemColor = GameColors.RarityColor(item.data.rarity);
                        }

                        string itemName = StringUtils.StringWithColor($"[{item.data.name}]", itemColor);

                        string attributes = item.attributes > 0 ? $" (+{item.attributes} Attributes)" : "";

                        itemsText += $"   🔹 {item.amount}x {itemName}{attributes}\n";
                    }
                }
            }

            string fullTooltip = $"{expText}{coinsText}{itemsText}".Trim();
            fullTooltip += "\n\n" + TooltipFormatter.GetRewardMessage();

            CreateTooltipTrigger(rewards.gameObject, "Quest Rewards\n", fullTooltip);
        }
        
        private void CreateTooltipTrigger(GameObject target, string title, string content)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) =>
            {
                GUITooltip.instance.ShowTooltip(title, content, target);
            });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) => GUITooltip.instance.HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        private void CreateDynamicTooltipTrigger(GameObject target, System.Action onPointerEnter)
        {
            var trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) =>
            {
                onPointerEnter?.Invoke();
            });
            trigger.triggers.Add(entryEnter);

            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) =>
            {
                GUITooltip.instance.HideTooltipDynamic();
            });
            trigger.triggers.Add(entryExit);
        }

        private void AttachTooltipsToObjective()
        {
            if (objective == null || quest == null)
                return;

            EventTrigger trigger = objective.gameObject.GetComponent<EventTrigger>() ?? objective.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((eventData) =>
            {
                if (Game.instance.quests.TryGetQuest(quest, out var instance))
                {
                    int currentProgress = instance.progress;
                    int finalProgress = instance.GetFinalTargetProgress();
                    int baseTargetProgress = quest.targetProgress;

                    string objectiveTooltipMessage = TooltipFormatter.FormatObjectiveTooltip(
                        quest.objective,
                        currentProgress,
                        finalProgress,
                        baseTargetProgress
                    );

                    GUITooltip.instance.ShowTooltip("Quest Objective\n", objectiveTooltipMessage, objective.gameObject);
                }
                else
                {
                    int previewProgress = quest.GetTargetProgress();
                    string objectiveTooltipMessage = $"{quest.objective} x{previewProgress}";
                    GUITooltip.instance.ShowTooltip("Quest Objective\n", objectiveTooltipMessage, objective.gameObject);
                }
            });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((eventData) => GUITooltip.instance.HideTooltipDynamic());
            trigger.triggers.Add(entryExit);
        }

        protected virtual void UpdateButtons()
        {
        if (!quest)
            return;

        // If the completeText field isn't assigned, warn and fallback to default cancel logic
        if (completeText == null)
        {
            Debug.LogWarning("You need to assign a UI Text (Legacy) to the Complete Text field", this);
            bool isLogFallback = Game.instance.quests.ContainsQuest(quest);
            logActions.SetActive(isLogFallback);
            giverActions.SetActive(!isLogFallback);
            cancel.gameObject.SetActive(true);
            cancel.interactable = Game.instance.quests.TryGetQuest(quest, out var questInstance) && !questInstance.completed;
            return;
        }

            var isLog = Game.instance.quests.ContainsQuest(quest);
            logActions.SetActive(isLog);
            giverActions.SetActive(!isLog);
            
            // Check if this quest instance is completed
            bool isComplete = false;
            if (Game.instance.quests.TryGetQuest(quest, out var instance))
            isComplete = instance.completed;
            if (isComplete)
            {
                // Hide the cancel button and show completion text
                completeText.gameObject.SetActive(true);
                cancel.gameObject.SetActive(false);
                completeText.text = "This Quest is Complete";
            }
            else
            {
                // Show the cancel button, hide completion text, and set interactable
                completeText.gameObject.SetActive(false);
                cancel.gameObject.SetActive(true);
                cancel.interactable = Game.instance.quests.TryGetQuest(quest, out var questInstance) && !questInstance.completed;
            }
        }

        protected virtual void OnInventoryUpdated()
        {
            UpdateButtons();
        }

        protected virtual void Start()
        {
            if (completeText == null)
            {
                Debug.LogWarning("the Complete Text field hasn't been assigned, You need to assign a UI Text (Legacy) to the Complete Text field", this);
            }
            
            InitializeCallbacks();
            Game.instance.currentCharacter.inventory.onInventoryUpdated += UpdateButtons;
        }

        protected virtual void OnEnable() => UpdateButtons();
    }
}
