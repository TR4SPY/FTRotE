using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Stash")]
    public class GUIStash : GUIInventory
    {
        [Header("Stash Settings")]
        public int stashIndex;

        [Header("Deposit Settings")]
        public Button depositButton;
        public InputField depositField;
        public GUIWindow depositWindow;
        public Dropdown depositCurrencyDropdown;

        [Header("Withdraw Settings")]
        public Button withdrawButton;
        public InputField withdrawField;
        public GUIWindow withdrawWindow;
        public Dropdown withdrawCurrencyDropdown;

        [Header("Audio Settings")]
        public AudioClip showClip;
        public AudioClip depositClip;
        public AudioClip withdrawClip;

        protected GUIWindow m_window;

        protected EntityInventory m_playerInventory => Level.instance.player.inventory;
        protected GUIInventory m_playerInventoryGUI => GUIWindowsManager.instance.GetInventory();

        protected virtual void InitializeWindow()
        {
            m_window = GetComponentInParent<GUIWindow>();
        }

        protected virtual void InitializeActions()
        {
            depositButton.onClick.AddListener(OnDeposit);
            withdrawButton.onClick.AddListener(OnWithdraw);
        }

        protected virtual void OnDeposit()
        {
            if (!int.TryParse(depositField.text, out int amount) || amount <= 0)
                return;

            var selectedText = depositCurrencyDropdown.options[depositCurrencyDropdown.value].text;

            if (!System.Enum.TryParse(selectedText, out CurrencyType selectedCurrencyType))
                return;

            int totalAmberlings = Currency.ConvertToAmberlings(amount, selectedCurrencyType);

            if (m_playerInventory.instance.currency.GetTotalAmberlings() < totalAmberlings)
                return;

            m_playerInventory.instance.SpendMoney(totalAmberlings);
            m_inventory.AddMoney(totalAmberlings);

            depositWindow.Hide();
            PlayAudio(depositClip);
        }

        protected virtual void OnWithdraw()
        {
            if (!int.TryParse(withdrawField.text, out int amount) || amount <= 0)
                return;

            var selectedText = withdrawCurrencyDropdown.options[withdrawCurrencyDropdown.value].text;

            if (!System.Enum.TryParse(selectedText, out CurrencyType selectedCurrencyType))
                return;

            int totalAmberlings = Currency.ConvertToAmberlings(amount, selectedCurrencyType);

            if (m_inventory.currency.GetTotalAmberlings() < totalAmberlings)
                return;

            m_inventory.SpendMoney(totalAmberlings);
            m_playerInventory.instance.AddMoney(totalAmberlings);

            withdrawWindow.Hide();
            PlayAudio(withdrawClip);
        }

        protected virtual void Start()
        {
            SetInventory(GameStash.instance.GetInventory(stashIndex));
            InitializeWindow();
            InitializeInventory();
            InitializeActions();
        }

        protected virtual void OnEnable()
        {
            m_audio.PlayUiEffect(showClip);
            m_playerInventoryGUI.GetComponent<GUIWindow>()?.Show();
        }

        protected virtual void OnDisable()
        {
            depositWindow.Hide();
            withdrawWindow.Hide();

            if (!m_window.isOpen)
                m_playerInventoryGUI.GetComponent<GUIWindow>()?.Hide();
        }
    }
}
