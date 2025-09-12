using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Bank")]
    public class GUIBank : MonoBehaviour
    {
        [Header("Main Menu")]
        public GameObject mainMenu;
        public Button investButton;
        public Button checkAccountsButton;

        [Header("Offers")]
        public GameObject offersPanel;
        public RectTransform offersContent;
        public GUIBankOffer offerPrefab;
        public Button offersBackButton;

        [Header("Accounts")]
        public GameObject accountsPanel;
        public GUIBankAccount[] accountSlots;
        public Toggle[] accountToggles;
        public GameObject emptyText;
        public Button newAccountButton;
        public Button accountsBackButton;

        [Header("New Account")]
        public GameObject newAccountPanel;
        public InputField depositSolmireField;
        public InputField depositLunarisField;
        public InputField depositAmberlingsField;
        public InputField accountNameField;
        public Text offerNameText;
        public Text offerRateText;
        public Text offerDurationText;

        [Header("Total Proceeds Display")]
        public Transform totalProceedsContainer;
        public GameObject priceTagPrefab;
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        [Tooltip("Displays the minimum deposit required for the selected offer.")]
        public Text minimumValueText;
        public Button confirmButton;
        public Button newAccountBackButton;
        public Button resetButton;

        private SavingOffer m_selectedOffer;
        private int m_minimumDeposit;
        // private readonly List<GUIBankAccount> m_accounts = new();

        protected EntityInventory m_playerInventory => Level.instance.player.inventory;

        protected virtual void Start()
        {
            if (investButton) investButton.onClick.AddListener(ShowOffers);
            if (checkAccountsButton) checkAccountsButton.onClick.AddListener(ShowAccounts);
            if (offersBackButton) offersBackButton.onClick.AddListener(BackToMenu);
            if (accountsBackButton) accountsBackButton.onClick.AddListener(BackToMenu);
            if (newAccountButton) newAccountButton.onClick.AddListener(ShowOffers);
            if (newAccountBackButton) newAccountBackButton.onClick.AddListener(BackToMenu);
            if (confirmButton) confirmButton.onClick.AddListener(ConfirmInvest);
            if (resetButton) resetButton.onClick.AddListener(ResetNewAccountInputs);
            if (depositSolmireField)
            {
                AddInputFieldListeners(depositSolmireField);
                depositSolmireField.onValueChanged.AddListener(_ => UpdateTotalProceeds());
                depositSolmireField.characterLimit = MaxSolmireDeposit.ToString().Length;                
            }
            if (depositLunarisField)
            {
                AddInputFieldListeners(depositLunarisField);
                depositLunarisField.onValueChanged.AddListener(_ => UpdateTotalProceeds());
                depositLunarisField.characterLimit = MaxLunarisDeposit.ToString().Length;
            }
            if (depositAmberlingsField)
            {
                AddInputFieldListeners(depositAmberlingsField);
                depositAmberlingsField.onValueChanged.AddListener(_ => UpdateTotalProceeds());
                depositAmberlingsField.characterLimit = MaxTotalAmberlings.ToString().Length;
            }

            if (accountNameField) AddInputFieldListeners(accountNameField);

            if ((accountSlots == null || accountSlots.Length == 0) && accountsPanel)
            {
                accountSlots = accountsPanel.GetComponentsInChildren<GUIBankAccount>(true);
            }

            bool hasNull = false;
            if (accountSlots == null || accountSlots.Length == 0)
            {
                Debug.LogWarning($"{nameof(GUIBank)}: accountSlots array is empty.");
            }
            else
            {
                foreach (var slot in accountSlots)
                {
                    if (slot)
                    {
                        slot.gameObject.SetActive(false);
                    }
                    else
                    {
                        hasNull = true;
                    }
                }
                if (hasNull)
                    Debug.LogWarning($"{nameof(GUIBank)}: accountSlots contains null elements.");
            }

            hasNull = false;
            if (accountToggles == null || accountToggles.Length == 0)
            {
                Debug.LogWarning($"{nameof(GUIBank)}: accountToggles array is empty.");
            }
            else
            {
                if (accountSlots != null && accountToggles.Length != accountSlots.Length)
                {
                    Debug.LogWarning($"{nameof(GUIBank)}: accountToggles length does not match accountSlots length.");
                }
                for (int i = 0; i < accountToggles.Length; i++)
                {
                    var toggle = accountToggles[i];
                    if (toggle)
                    {
                        toggle.gameObject.SetActive(false);
                        toggle.interactable = false;
                    }
                    else
                    {
                        hasNull = true;
                    }
                }
                if (hasNull)
                    Debug.LogWarning($"{nameof(GUIBank)}: accountToggles contains null elements.");
            }

            BackToMenu();
            UpdateTotalProceeds();
        }

        protected virtual void BackToMenu()
        {
            if (mainMenu) mainMenu.SetActive(true);
            if (offersPanel) offersPanel.SetActive(false);
            if (accountsPanel) accountsPanel.SetActive(false);
            if (newAccountPanel) newAccountPanel.SetActive(false);
        }

        protected virtual void ShowOffers()
        {
            if (!BankManager.instance) return;
            if (!BankManager.instance.HasAvailableSlot)
            {
                ShowMaximumAccountsWarning();
                return;
            }

            mainMenu.SetActive(false);
            accountsPanel.SetActive(false);
            newAccountPanel.SetActive(false);
            offersPanel.SetActive(true);
            PopulateOffers();
        }

        protected virtual void ShowAccounts()
        {
            if (!BankManager.instance) return;
            mainMenu.SetActive(false);
            offersPanel.SetActive(false);
            newAccountPanel.SetActive(false);
            accountsPanel.SetActive(true);
            //RefreshAccounts();

            var accounts = BankManager.instance.accounts;
            bool hasAccounts = accounts.Count > 0;

            if (emptyText) emptyText.SetActive(!hasAccounts);

            int length = Mathf.Max(accountSlots.Length, accountToggles != null ? accountToggles.Length : 0);
            for (int i = 0; i < length; i++)
            {
                var slot = i < accountSlots.Length ? accountSlots[i] : null;
                var toggle = (accountToggles != null && i < accountToggles.Length) ? accountToggles[i] : null;
                bool active = hasAccounts && i < accounts.Count;

                if (slot)
                {
                    slot.gameObject.SetActive(active && i == 0);
                    if (active)
                    {
                        int index = i;
                        slot.Set(accounts[i], () => OnWithdraw(index));
                    }
                }

                if (toggle)
                {
                    toggle.gameObject.SetActive(active);
                    toggle.interactable = active;
                    toggle.isOn = active && i == 0;
                    toggle.onValueChanged.RemoveAllListeners();
                    if (active && slot)
                    {
                        var slotRef = slot;
                        toggle.onValueChanged.AddListener(isOn => slotRef.gameObject.SetActive(isOn));
                    }
                }
            }
        }

        protected virtual void PopulateOffers()
        {
            foreach (Transform child in offersContent)
                Destroy(child.gameObject);

            foreach (var offer in BankManager.instance.offers)
            {
                var item = Instantiate(offerPrefab, offersContent);
                item.Set(offer, () => OnSelectOffer(offer));
            }
        }

/*
        protected virtual void RefreshAccounts()
        {
            foreach (Transform child in accountsContent)
                Destroy(child.gameObject);
            m_accounts.Clear();

            var accounts = BankManager.instance.accounts;
            for (int i = 0; i < accounts.Count; i++)
            {
                int index = i;
                var acc = accounts[i];
                var item = Instantiate(accountPrefab, accountsContent);
                item.Set(acc, () => OnWithdraw(index));
                m_accounts.Add(item);
            }
        }
*/

        protected virtual void OnSelectOffer(SavingOffer offer)
        {
            if (!BankManager.instance.HasAvailableSlot)
            {
                ShowMaximumAccountsWarning();
                return;
            }

            if (!BankManager.instance.HasAvailableSlot) return;
            m_selectedOffer = offer;
            m_minimumDeposit = BankManager.GetMinimumDeposit(offer);
            ResetNewAccountInputs();
            if (offerNameText) offerNameText.text = offer.offerName;
            if (offerRateText) offerRateText.text = $"{offer.interestRate:P0}";
            if (offerDurationText) offerDurationText.text = $"{offer.durationHours:0}h";
            if (minimumValueText) minimumValueText.text = Currency.FormatCurrencyString(m_minimumDeposit);
            offersPanel.SetActive(false);
            newAccountPanel.SetActive(true);
            UpdateTotalProceeds();
        }

        protected virtual void ConfirmInvest()
        {
            if (m_selectedOffer == null) return;
            int solmire = 0;
            int lunaris = 0;
            int amberlings = 0;

            if (depositSolmireField) int.TryParse(depositSolmireField.text, out solmire);
            if (depositLunarisField) int.TryParse(depositLunarisField.text, out lunaris);
            if (depositAmberlingsField) int.TryParse(depositAmberlingsField.text, out amberlings);

            int deposit = Currency.ConvertToAmberlings(solmire, CurrencyType.Solmire)
                        + Currency.ConvertToAmberlings(lunaris, CurrencyType.Lunaris)
                        + Currency.ConvertToAmberlings(amberlings, CurrencyType.Amberlings);

            if (deposit <= 0) return;
            if (deposit < m_minimumDeposit)
            {
                var info = GUIWindowsManager.instance?.GetInformation();
                if (info != null)
                {
                    string minText = Currency.FormatCurrencyString(m_minimumDeposit);
                    info.SetInformation("Investment Too Low", $"The investment is too low! You have to invest at least {minText}.");
                    info.GetComponent<GUIWindow>()?.Show();
                }
                return;
            }
            
            if (m_playerInventory.instance.currency.GetTotalAmberlings() < deposit)
            {
                var info = GUIWindowsManager.instance?.GetInformation();
                if (info != null)
                {
                    info.SetInformation("Insufficient Funds", "I’m afraid you don’t have sufficient funds for that deposit. Please, only invest what you truly own and can comfortably set aside.");
                    info.GetComponent<GUIWindow>()?.Show();
                }
                return;
            }

            if (!BankManager.instance.HasAvailableSlot)
            {
                ShowMaximumAccountsWarning();
                return;
            }

            string name = string.IsNullOrEmpty(accountNameField.text) ? "Account" : accountNameField.text;

            m_playerInventory.instance.SpendMoney(deposit);
            BankManager.instance.OpenAccount(name, deposit, m_selectedOffer);

            BackToMenu();
            // RefreshAccounts();
        }

        protected virtual void ShowMaximumAccountsWarning()
        {
            var info = GUIWindowsManager.instance?.GetInformation();
            if (info != null)
            {
                info.SetInformation("Maximum Accounts Reached", "You have reached maximum number of active accounts in our bank as a normal customer. Please, wait until at least one of them fullfil.");
                info.GetComponent<GUIWindow>()?.Show();
            }
        }

        protected virtual void ResetNewAccountInputs()
        {
            if (depositSolmireField) depositSolmireField.text = "0";
            if (depositLunarisField) depositLunarisField.text = "0";
            if (depositAmberlingsField) depositAmberlingsField.text = "0";
            if (accountNameField) accountNameField.text = string.Empty;
            UpdateTotalProceeds();
        }

        private const long MaxTotalAmberlings = int.MaxValue;
        private const long MaxSolmireDeposit = MaxTotalAmberlings / 100000L;
        private const long MaxLunarisDeposit = MaxTotalAmberlings / 100L;

        protected virtual void UpdateTotalProceeds()
        {
            if (!totalProceedsContainer || !priceTagPrefab) return;

            foreach (Transform child in totalProceedsContainer)
            {
                Destroy(child.gameObject);
            }

            long solmire = ParseAndClamp(depositSolmireField, MaxSolmireDeposit);
            long lunaris = ParseAndClamp(depositLunarisField, MaxLunarisDeposit);
            long amberlings = ParseAndClamp(depositAmberlingsField, MaxTotalAmberlings);

            long deposit = solmire * 100000L
                         + lunaris * 100L
                         + amberlings;
            if (deposit > MaxTotalAmberlings)
            {
                deposit = MaxTotalAmberlings;
            }

            float rate = m_selectedOffer != null ? m_selectedOffer.interestRate : 0f;
            long total = (long)System.Math.Round(deposit * (1.0 + rate));
            if (total > MaxTotalAmberlings)
            {
                total = MaxTotalAmberlings;
            }

            Currency.SplitCurrency((int)total, out int totalSolmire, out int totalLunaris, out int totalAmberlings);
            AddPriceTag(totalSolmire, solmireIcon);
            AddPriceTag(totalLunaris, lunarisIcon);
            AddPriceTag(totalAmberlings, amberlingsIcon);
        }

        private long ParseAndClamp(InputField field, long max)
        {
            if (!field) return 0;
            if (!long.TryParse(field.text, out var value))
            {
                value = max;
                field.text = max.ToString();
                return value;
            }
            long clamped = System.Math.Clamp(value, 0, max);
            if (clamped != value || field.text != clamped.ToString())
            {
                field.text = clamped.ToString();
            }
            return clamped;
        }

        private void AddPriceTag(int amount, Sprite icon)
        {
            var go = Instantiate(priceTagPrefab, totalProceedsContainer);

            var textObj = go.transform.Find("Name")?.GetComponent<Text>();
            if (textObj) textObj.text = amount.ToString();

            var iconObj = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconObj)
            {
                iconObj.sprite = icon;
            }
        }

        protected virtual void AddInputFieldListeners(InputField field)
        {
            var trigger = field.GetComponent<EventTrigger>();
            if (!trigger) trigger = field.gameObject.AddComponent<EventTrigger>();

            var selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener(_ => OnInputFieldSelect());
            trigger.triggers.Add(selectEntry);

            var deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselectEntry.callback.AddListener(_ => OnInputFieldDeselect());
            trigger.triggers.Add(deselectEntry);

            field.onEndEdit.AddListener(_ => OnInputFieldDeselect());
        }

        protected virtual void OnInputFieldSelect()
        {
            Game.instance.gameplayActions?.Disable();
            Game.instance.guiActions?.Disable();
        }

        protected virtual void OnInputFieldDeselect()
        {
            Game.instance.gameplayActions?.Enable();
            Game.instance.guiActions?.Enable();
        }

        protected virtual void OnWithdraw(int index)
        {
            int value = BankManager.instance.Withdraw(index);
            if (value > 0)
            {
                m_playerInventory.instance.AddMoney(value);
                // RefreshAccounts();
                ShowAccounts();
            }
        }

        protected virtual void Update()
        {
            if (accountsPanel && accountsPanel.activeSelf)
            {
                var accounts = BankManager.instance.accounts;
/*
                for (int i = 0; i < accounts.Count && i < m_accounts.Count; i++)
                {
                    m_accounts[i].UpdateTime(accounts[i]);
                }
*/

                int activeCount = Mathf.Min(accountSlots.Length, accounts.Count);
                for (int i = 0; i < activeCount; i++)
                {
                    if (accountSlots[i].gameObject.activeSelf)
                    {
                        accountSlots[i].UpdateTime(accounts[i]);
                    }
                }
            }
        }
    }
}
