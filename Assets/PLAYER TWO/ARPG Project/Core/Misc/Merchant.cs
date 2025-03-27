//  ZMODYFIKOWANO 31 GRUDNIA 2024
using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Misc/Merchant")]
    public class Merchant : Interactive
    {
        [System.Serializable]
        public class MerchantItem
        {
            [Tooltip("The scriptable object representing the Item.")]
            public Item data;

            [Tooltip("The amount of additional attributes on this Item.")]
            public int attributes;

            [Tooltip("Upgrade level of the item (only for equippable items).")]
            [Range(0, 25)]
            public int itemLevel;
        }

        [System.Serializable]
        public class Section
        {
            [Tooltip("The title of the section.")]
            public string title;

            [Tooltip("The items available to purchase from this section.")]
            public MerchantItem[] items;
        }

        [Header("Merchant Settings")]
        [Tooltip("The amount of rows in the Merchant's Inventory.")]
        public int rows;

        [Tooltip("The amount of columns in the Merchant's Inventory.")]
        public int columns;

        [Tooltip("The title of the section to buy back items.")]
        public string buyBackTitle = "BUY BACK";

        [Tooltip("The shopping sections/categories available on the Merchant.")]
        public Section[] sections;
        public Dialog assignedDialog;

        /// <summary>
        /// Returns a dictionary of inventories using their section title as key.
        /// </summary>
        public Dictionary<string, Inventory> inventories { get; protected set; }

        protected GUIMerchant m_guiMerchant => GUIWindowsManager.instance.GetMerchant();

        protected virtual void InitializeInventories()
        {
            inventories = new Dictionary<string, Inventory>();

            foreach (var section in sections)
            {
                if (section.items == null) continue;

                var inventory = new Inventory(rows, columns);
                inventories.Add(section.title, inventory);

                foreach (var item in section.items)
                {
                    if (item.data == null) continue;

                    var instance = (item.attributes > 0)
                        ? new ItemInstance(item.data, true, item.attributes, item.attributes)
                        : new ItemInstance(item.data, false);

                    if (instance.IsEquippable())
                    {
                        int level = Mathf.Clamp(item.itemLevel, 0, instance.GetEquippable().maxUpgradeLevel);
                        for (int i = 0; i < level; i++)
                            instance.UpgradeLevel();
                    }

                    inventory.TryAddItem(instance);
                }
            }

            inventories.Add(buyBackTitle, new Inventory(rows, columns));
        }

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity)) return;

            if (assignedDialog != null)
            {
                if (GUIWindowsManager.instance.dialogWindow == null)
                {
                    Debug.LogError("dialogWindow is NULL in GUIWindowsManager! Ensure it is assigned.");
                    return;
                }

                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }
            
            var interactionLogger = GetComponent<NpcInteractionLogger>();
            if (interactionLogger != null)
            {
                var collider = entity.GetComponent<Collider>();
                if (collider != null)
                {
                    interactionLogger.LogInteraction(collider);
                }
                else
                {
                    Debug.LogWarning("Collider for interacting entity is null. Cannot log interaction.");
                }
            }
            else
            {
                Debug.LogWarning("NpcInteractionLogger not found on Merchant. Cannot log interaction.");
            }
        }
        
        public void OpenQuestDialog()
        {
            var questGiver = GetComponent<QuestGiver>();
            if (questGiver != null)
            {
                questGiver.OpenQuestDialog();
            }
            else
            {
                return;
            }
        }

       public void OpenExclusiveQuestDialog()
        {
            var questGiver = GetComponent<QuestGiver>();
            if (questGiver == null) return;

            if (GUIWindowsManager.instance.dialogWindow != null)
            {
                GUIWindowsManager.instance.dialogWindow.OpenExclusiveWindow();
            }
        }

        public void OpenMerchantShop()
        {

                            GUIWindowsManager.instance.merchantWindow.Show();
                            GUIWindowsManager.instance.inventoryWindow.Show();
                            m_guiMerchant.SetMerchant(this);

                            Debug.Log("Merchant interacted with by player. UI opened.");
        }

        protected override void Start()
        {
            base.Start();
            InitializeInventories();
        }
    }
}