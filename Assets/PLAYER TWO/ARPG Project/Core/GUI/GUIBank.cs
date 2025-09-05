using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Bank")]
    public class GUIBank : MonoBehaviour
    {
        [System.Serializable]
        public class Account
        {
            public string name;
            public Currency currency = new();
            [Tooltip("Interest rate applied when claiming interest.")]
            public float interestRate = 0.05f;
            [Tooltip("Time in seconds between interest claims.")]
            public float interestInterval = 60f;
            [HideInInspector] public float lastClaimTime;

            public bool CanClaim => Time.time >= lastClaimTime + interestInterval;
            public float RemainingTime => Mathf.Max(0f, (lastClaimTime + interestInterval) - Time.time);

            public int ClaimInterest()
            {
                if (!CanClaim) return 0;
                int reward = Mathf.FloorToInt(currency.GetTotalAmberlings() * interestRate);
                currency.AddAmberlings(reward);
                lastClaimTime = Time.time;
                return reward;
            }
        }

        [Header("Accounts")]
        public Account[] accounts;

        [Header("UI References")]
        public Dropdown accountDropdown;
        public InputField depositSolmireField;
        public InputField depositLunarisField;
        public InputField depositAmberlingsField;
        public InputField withdrawSolmireField;
        public InputField withdrawLunarisField;
        public InputField withdrawAmberlingsField;
        public Button depositButton;
        public Button withdrawButton;
        public Text timeRemainingText;
        public Button claimButton;

        protected GUIWindow m_window;
        protected EntityInventory m_playerInventory => Level.instance.player.inventory;

        protected Account currentAccount
        {
            get
            {
                if (accounts == null || accounts.Length == 0) return null;
                int index = Mathf.Clamp(accountDropdown.value, 0, accounts.Length - 1);
                return accounts[index];
            }
        }

        protected virtual void Start()
        {
            InitializeWindow();
            InitializeDropdown();
            InitializeActions();
            UpdateTime();
        }

        protected virtual void InitializeWindow()
        {
            m_window = GetComponentInParent<GUIWindow>();
        }

        protected virtual void InitializeDropdown()
        {
            if (!accountDropdown) return;
            accountDropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            if (accounts != null)
            {
                foreach (var acc in accounts)
                    options.Add(new Dropdown.OptionData(acc.name));
            }
            accountDropdown.AddOptions(options);
            accountDropdown.onValueChanged.AddListener(_ => UpdateTime());
        }

        protected virtual void InitializeActions()
        {
            if (depositButton) depositButton.onClick.AddListener(OnDeposit);
            if (withdrawButton) withdrawButton.onClick.AddListener(OnWithdraw);
            if (claimButton) claimButton.onClick.AddListener(OnClaim);
        }

        protected virtual int ParseFields(InputField sol, InputField lun, InputField amb)
        {
            int total = 0;
            if (sol && int.TryParse(sol.text, out int s) && s > 0)
                total += Currency.ConvertToAmberlings(s, CurrencyType.Solmire);
            if (lun && int.TryParse(lun.text, out int l) && l > 0)
                total += Currency.ConvertToAmberlings(l, CurrencyType.Lunaris);
            if (amb && int.TryParse(amb.text, out int a) && a > 0)
                total += a;
            return total;
        }

        protected virtual void ClearFields(InputField sol, InputField lun, InputField amb)
        {
            if (sol) sol.text = "0";
            if (lun) lun.text = "0";
            if (amb) amb.text = "0";
        }

        protected virtual void OnDeposit()
        {
            var account = currentAccount;
            if (account == null) return;
            int amount = ParseFields(depositSolmireField, depositLunarisField, depositAmberlingsField);
            if (amount <= 0) return;
            if (m_playerInventory.instance.currency.GetTotalAmberlings() < amount) return;

            m_playerInventory.instance.SpendMoney(amount);
            account.currency.AddAmberlings(amount);

            ClearFields(depositSolmireField, depositLunarisField, depositAmberlingsField);
        }

        protected virtual void OnWithdraw()
        {
            var account = currentAccount;
            if (account == null) return;
            int amount = ParseFields(withdrawSolmireField, withdrawLunarisField, withdrawAmberlingsField);
            if (amount <= 0) return;
            if (account.currency.GetTotalAmberlings() < amount) return;

            account.currency.RemoveAmberlings(amount);
            m_playerInventory.instance.AddMoney(amount);

            ClearFields(withdrawSolmireField, withdrawLunarisField, withdrawAmberlingsField);
        }

        protected virtual void UpdateTime()
        {
            var account = currentAccount;
            if (!timeRemainingText || account == null) return;
            float remaining = account.RemainingTime;
            timeRemainingText.text = remaining > 0f ? $"{remaining:0}s" : "Ready!";
            if (claimButton) claimButton.interactable = account.CanClaim;
        }

        protected virtual void Update()
        {
            UpdateTime();
        }

        protected virtual void OnClaim()
        {
            var account = currentAccount;
            if (account == null || !account.CanClaim) return;
            int reward = account.ClaimInterest();
            if (reward > 0)
                m_playerInventory.instance.AddMoney(reward);
            UpdateTime();
        }
    }
}
