using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Blacksmith")]
    public class GUIBlacksmith : GUIWindow
    {
        [Header("Blacksmith Settings")]
        [Tooltip("The slot to place the items to repair.")]
        public GUIBlacksmithSlot slot;

        [Tooltip("The reference to the 'repair' Button.")]
        public Button repairButton;

        [Tooltip("The reference to the 'repair all' Button.")]
        public Button repairAllButton;

       // [Tooltip("The reference to the 'repair cost' Text.")]
       // public Text repairCostText;

       // [Tooltip("The reference to the 'repair all cost' Text.")]
       // public Text repairAllCostText;

        [Header("Dynamic Repair Cost")]
        [Tooltip("Container for the single-item repair cost.")]
        public Transform repairCostContainer;

        [Tooltip("Container for the 'repair all' cost.")]
        public Transform repairAllCostContainer;

        [Tooltip("Prefab z 'Name'(Text) i 'Icon'(Image).")]
        public GameObject priceTagPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        [Header("Audio Settings")]
        [Tooltip("The Audio Clip that plays when repairing an Item.")]
        public AudioClip repairAudio;

        protected Blacksmith m_blacksmith;
        protected GUIInventory m_inventory;

        protected virtual void UpdateButtons()
        {
            repairButton.interactable = m_blacksmith.GetPriceToRepair(slot.item?.item) > 0;
            repairAllButton.interactable = m_blacksmith.GetPriceToRepairAll() > 0;
        }

        protected virtual void InitializeCallbacks()
        {
            repairButton.onClick.AddListener(OnRepairClicked);
            repairAllButton.onClick.AddListener(OnRepairAllClicked);
            slot.onEquip.AddListener(OnEquip);
            slot.onUnequip.AddListener(OnUnequip);
        }

        protected virtual void OnRepairClicked()
        {
            if (!m_blacksmith || !slot.item) return;

            if (m_blacksmith.TryRepair(slot.item.item))
            {
                ClearRepairCost();
                UpdateButtons();
                m_audio.PlayUiEffect(repairAudio);
            }
        }

        protected virtual void OnRepairAllClicked()
        {
            if (!m_blacksmith) return;

            if (m_blacksmith.TryRepairAll())
            {
                UpdateRepairAllCost();
                UpdateButtons();
                m_audio.PlayUiEffect(repairAudio);
            }
        }

        public virtual void OnEquip(GUIItem item)
        {
            if (item.item.GetDurabilityRate() == 1)
            {
                ClearRepairCost();
                return;
            }

            UpdateRepairCost();
            UpdateRepairAllCost();
            UpdateButtons();
        }

        public virtual void OnUnequip(GUIItem _)
        {
            ClearRepairCost();
            UpdateRepairAllCost();
            UpdateButtons();
        }

        public virtual void Show(Blacksmith blacksmith)
        {
            base.Show();
            m_blacksmith = blacksmith;
            m_inventory = GUIWindowsManager.instance.GetInventory();
            m_inventory.GetComponent<GUIWindow>()?.Show();
            UpdateRepairAllCost();
            UpdateButtons();
        }

        public virtual void Refresh()
        {
            if (!isOpen) return;

            UpdateRepairCost();
            UpdateRepairAllCost();
            UpdateButtons();
        }

        private string FormatCurrency(int totalAmberlings)
        {
            var currency = new Currency();
            currency.SetFromTotalAmberlings(totalAmberlings);
            return currency.ToString();
        }

/*
        protected virtual void UpdateRepairCost() =>
           // repairCostText.text = m_blacksmith.GetPriceToRepair(slot.item?.item).ToString();
            repairCostText.text = FormatCurrency(m_blacksmith.GetPriceToRepair(slot.item?.item));
*/

        protected virtual void UpdateRepairCost()
        {
            int price = m_blacksmith.GetPriceToRepair(slot.item?.item);
            ShowPriceTags(repairCostContainer, price);
        }

        protected virtual void ClearRepairCost()
        {
            ClearPriceTags(repairCostContainer);
        }

/*
        protected virtual void UpdateRepairAllCost() =>
            // repairAllCostText.text = m_blacksmith.GetPriceToRepairAll().ToString();
            repairAllCostText.text = FormatCurrency(m_blacksmith.GetPriceToRepairAll());
*/

        protected virtual void UpdateRepairAllCost()
        {
            int priceAll = m_blacksmith.GetPriceToRepairAll();
            ShowPriceTags(repairAllCostContainer, priceAll);
        }

        private void ClearPriceTags(Transform container)
        {
            if (!container) return;
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void AddPriceTag(Transform container, int amount, Sprite icon)
        {
            var go = Instantiate(priceTagPrefab, container);
            var textObj = go.transform.Find("Name")?.GetComponent<Text>();
            if (textObj) textObj.text = amount.ToString();

            var imageObj = go.transform.Find("Icon")?.GetComponent<Image>();
            if (imageObj && icon)
                imageObj.sprite = icon;
        }

        private void ShowPriceTags(Transform container, int totalAmberlings)
        {
            ClearPriceTags(container);

            if (totalAmberlings <= 0) return;

            var c = new Currency();
            c.SetFromTotalAmberlings(totalAmberlings);

            if (c.solmire > 0)
                AddPriceTag(container, c.solmire, solmireIcon);
            if (c.lunaris > 0)
                AddPriceTag(container, c.lunaris, lunarisIcon);
            if (c.amberlings > 0)
                AddPriceTag(container, c.amberlings, amberlingsIcon);
        }

        protected override void OnClose()
        {
            if (!m_inventory) return;

            m_inventory.GetComponent<GUIWindow>()?.Hide();
        }

        protected override void Start()
        {
            base.Start();
            InitializeCallbacks();
            UpdateButtons();
        }

        protected virtual void OnDisable()
        {
            if (!slot || !slot.item) return;

            if (slot.item.TryMoveToLastPosition())
                slot.Unequip();
        }
    }
}
