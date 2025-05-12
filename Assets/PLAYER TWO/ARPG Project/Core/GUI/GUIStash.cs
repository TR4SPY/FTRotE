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
        public InputField depositSolmireField;
        public InputField depositLunarisField;
        public InputField depositAmberlingsField;
        public GUIWindow depositWindow;

        [Header("Withdraw Settings")]
        public Button withdrawButton;
        public InputField withdrawSolmireField;
        public InputField withdrawLunarisField;
        public InputField withdrawAmberlingsField;
        public GUIWindow withdrawWindow;

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

        private void ClearDepositFields()
        {
            depositSolmireField.text = "0";
            depositLunarisField.text = "0";
            depositAmberlingsField.text = "0";
        }

        private void ClearWithdrawFields()
        {
            withdrawSolmireField.text = "0";
            withdrawLunarisField.text = "0";
            withdrawAmberlingsField.text = "0";
        }

        protected virtual void OnDeposit()
        {

            int totalAmberlings = 0;

            if (int.TryParse(depositSolmireField.text, out int solmire) && solmire > 0)
                totalAmberlings += Currency.ConvertToAmberlings(solmire, CurrencyType.Solmire);

            if (int.TryParse(depositLunarisField.text, out int lunaris) && lunaris > 0)
                totalAmberlings += Currency.ConvertToAmberlings(lunaris, CurrencyType.Lunaris);

            if (int.TryParse(depositAmberlingsField.text, out int amberlings) && amberlings > 0)
                totalAmberlings += amberlings;

            if (totalAmberlings <= 0)
                return;

            if (m_playerInventory.instance.currency.GetTotalAmberlings() < totalAmberlings)
                return;

            m_playerInventory.instance.SpendMoney(totalAmberlings);
            m_inventory.AddMoney(totalAmberlings);

            depositWindow.Hide();
            PlayAudio(depositClip);

        }

        protected virtual void OnWithdraw()
        {
            int totalAmberlings = 0;

            if (int.TryParse(withdrawSolmireField.text, out int solmire) && solmire > 0)
                totalAmberlings += Currency.ConvertToAmberlings(solmire, CurrencyType.Solmire);

            if (int.TryParse(withdrawLunarisField.text, out int lunaris) && lunaris > 0)
                totalAmberlings += Currency.ConvertToAmberlings(lunaris, CurrencyType.Lunaris);

            if (int.TryParse(withdrawAmberlingsField.text, out int amberlings) && amberlings > 0)
                totalAmberlings += amberlings;

            if (totalAmberlings <= 0)
                return;

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

            if (depositWindow != null)
            {
                depositWindow.onClose.AddListener(ClearDepositFields);
            }

            if (withdrawWindow != null)
            {
                withdrawWindow.onClose.AddListener(ClearWithdrawFields);
            }
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
