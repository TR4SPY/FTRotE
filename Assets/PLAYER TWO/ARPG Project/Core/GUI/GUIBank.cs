using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        public OfferItem offerItemPrefab;
        public Button offersBackButton;

        [Header("Accounts")]
        public GameObject accountsPanel;
        public RectTransform accountsContent;
        public AccountItem accountItemPrefab;
        public Button accountsBackButton;

        [Header("New Account")]
        public GameObject newAccountPanel;
        public InputField depositField;
        public InputField accountNameField;
        public Button confirmButton;
        public Button newAccountBackButton;

        private SavingOffer m_selectedOffer;
        private readonly List<AccountItem> m_accountItems = new();

        protected EntityInventory m_playerInventory => Level.instance.player.inventory;

        protected virtual void Start()
        {
            if (investButton) investButton.onClick.AddListener(ShowOffers);
            if (checkAccountsButton) checkAccountsButton.onClick.AddListener(ShowAccounts);
            if (offersBackButton) offersBackButton.onClick.AddListener(BackToMenu);
            if (accountsBackButton) accountsBackButton.onClick.AddListener(BackToMenu);
            if (newAccountBackButton) newAccountBackButton.onClick.AddListener(BackToMenu);
            if (confirmButton) confirmButton.onClick.AddListener(ConfirmInvest);

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
            RefreshAccounts();
        }

        protected virtual void PopulateOffers()
        {
            foreach (Transform child in offersContent)
                Destroy(child.gameObject);

            foreach (var offer in BankManager.instance.offers)
            {
                var item = Instantiate(offerItemPrefab, offersContent);
                item.Set(offer, () => OnSelectOffer(offer));
            }
        }

        protected virtual void RefreshAccounts()
        {
            foreach (Transform child in accountsContent)
                Destroy(child.gameObject);
            m_accountItems.Clear();

            var accounts = BankManager.instance.accounts;
            for (int i = 0; i < accounts.Count; i++)
            {
                int index = i;
                var acc = accounts[i];
                var item = Instantiate(accountItemPrefab, accountsContent);
                item.Set(acc, () => OnWithdraw(index));
                m_accountItems.Add(item);
            }
        }

        protected virtual void OnSelectOffer(SavingOffer offer)
        {
            if (!BankManager.instance.HasAvailableSlot) return;
            m_selectedOffer = offer;
            if (depositField) depositField.text = string.Empty;
            if (accountNameField) accountNameField.text = string.Empty;
            offersPanel.SetActive(false);
            newAccountPanel.SetActive(true);
        }

        protected virtual void ConfirmInvest()
        {
            if (m_selectedOffer == null) return;
            if (!int.TryParse(depositField.text, out int deposit) || deposit <= 0) return;
            if (m_playerInventory.instance.currency.GetTotalAmberlings() < deposit) return;
            if (!BankManager.instance.HasAvailableSlot) return;

            string name = string.IsNullOrEmpty(accountNameField.text) ? "Account" : accountNameField.text;

            m_playerInventory.instance.SpendMoney(deposit);
            BankManager.instance.OpenAccount(name, deposit, m_selectedOffer);

            BackToMenu();
            RefreshAccounts();
        }

        protected virtual void OnWithdraw(int index)
        {
            int value = BankManager.instance.Withdraw(index);
            if (value > 0)
            {
                m_playerInventory.instance.AddMoney(value);
                RefreshAccounts();
            }
        }

        protected virtual void Update()
        {
            if (accountsPanel && accountsPanel.activeSelf)
            {
                var accounts = BankManager.instance.accounts;
                for (int i = 0; i < accounts.Count && i < m_accountItems.Count; i++)
                {
                    m_accountItems[i].UpdateTime(accounts[i]);
                }
            }
        }

        [System.Serializable]
        public class OfferItem : MonoBehaviour
        {
            public Text nameText;
            public Text rateText;
            public Text durationText;
            public Button selectButton;

            public void Set(SavingOffer offer, UnityAction onSelect)
            {
                if (nameText) nameText.text = offer.offerName;
                if (rateText) rateText.text = $"{offer.interestRate:P0}";
                if (durationText) durationText.text = $"{offer.durationHours:0}h";
                if (selectButton)
                {
                    selectButton.onClick.RemoveAllListeners();
                    selectButton.onClick.AddListener(onSelect);
                }
            }
        }

        [System.Serializable]
        public class AccountItem : MonoBehaviour
        {
            public Text nameText;
            public Text valueText;
            public Text timeText;
            public Button withdrawButton;

            public void Set(BankManager.InvestmentAccount account, UnityAction onWithdraw)
            {
                if (nameText) nameText.text = account.name;
                if (withdrawButton)
                {
                    withdrawButton.onClick.RemoveAllListeners();
                    withdrawButton.onClick.AddListener(onWithdraw);
                }
                UpdateTime(account);
            }

            public void UpdateTime(BankManager.InvestmentAccount account)
            {
                if (valueText) valueText.text = Currency.FormatCurrencyString(account.CurrentValue);
                if (timeText) timeText.text = account.Matured ? "Ready!" : $"{account.RemainingTime:0}s";
                if (withdrawButton) withdrawButton.interactable = account.Matured;
            }
        }
    }
}
