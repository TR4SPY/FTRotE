using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        public Button newAccountButton;
        public Button accountsBackButton;

        [Header("New Account")]
        public GameObject newAccountPanel;
        public InputField depositSolmireField;
        public InputField depositLunarisField;
        public InputField depositAmberlingsField;
        public InputField accountNameField;
        public Button confirmButton;
        public Button newAccountBackButton;
        public Button resetButton;

        private SavingOffer m_selectedOffer;
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
            if (depositSolmireField) AddInputFieldListeners(depositSolmireField);
            if (depositLunarisField) AddInputFieldListeners(depositLunarisField);
            if (depositAmberlingsField) AddInputFieldListeners(depositAmberlingsField);
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

            BackToMenu();
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
            for (int i = 0; i < accountSlots.Length; i++)
            {
                var slot = accountSlots[i];
                bool active = i < accounts.Count;
                slot.gameObject.SetActive(active);
                if (active)
                {
                    int index = i;
                    slot.Set(accounts[i], () => OnWithdraw(index));
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
            if (!BankManager.instance.HasAvailableSlot) return;
            m_selectedOffer = offer;
            ResetNewAccountInputs();
            offersPanel.SetActive(false);
            newAccountPanel.SetActive(true);
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
            if (m_playerInventory.instance.currency.GetTotalAmberlings() < deposit) return;
            if (!BankManager.instance.HasAvailableSlot) return;

            string name = string.IsNullOrEmpty(accountNameField.text) ? "Account" : accountNameField.text;

            m_playerInventory.instance.SpendMoney(deposit);
            BankManager.instance.OpenAccount(name, deposit, m_selectedOffer);

            BackToMenu();
            // RefreshAccounts();
        }

        protected virtual void ResetNewAccountInputs()
        {
            if (depositSolmireField) depositSolmireField.text = "0";
            if (depositLunarisField) depositLunarisField.text = "0";
            if (depositAmberlingsField) depositAmberlingsField.text = "0";
            if (accountNameField) accountNameField.text = string.Empty;
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
