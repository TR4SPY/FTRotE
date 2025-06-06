using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/NPC/Craftman")]
    public class Craftman : Interactive
    {
        [System.Serializable]
        public class CraftmanItem
        {
            public Item data;
            public int attributes;
            public int elements;
        }

        [System.Serializable]
        public class Section
        {
            public string title;
            public CraftmanItem[] items;
        }

        [Header("Craftman Settings")]
        public int rows;
        public int columns;
        public string sectionTitle = "CRAFTING";
        public Section section;
        public Dialog assignedDialog;

        public Dictionary<string, Inventory> inventories { get; protected set; }

        protected GUICraftman m_guiCraftman => GUIWindowsManager.instance.GetCraftman();

        protected override void Start()
        {
            base.Start();
           // Debug.Log("[Craftman] Start()");
            InitializeInventory();
        }

        protected virtual void InitializeInventory()
        {
            // Debug.Log("[Craftman] InitializeInventory()");
            inventories = new Dictionary<string, Inventory>();

            if (section.items != null)
            {
                var inventory = new Inventory(rows, columns);
                inventories.Add(section.title, inventory);

                foreach (var item in section.items)
                {
                    if (item.data == null)
                    {
                        Debug.LogWarning("[Craftman] Skipping null item data.");
                        continue;
                    }

                    // Debug.Log($"[Craftman] Adding item: {item.data.name}, Attributes: {item.attributes}");

                    if (item.attributes > 0)
                        inventory.TryAddItem(new ItemInstance(item.data, true, true, item.attributes, item.attributes, item.elements, item.elements));
                    else
                        inventory.TryAddItem(new ItemInstance(item.data, false, true, 0, 0, item.elements, item.elements));
                }
            }
            else
            {
                Debug.LogWarning("[Craftman] Section items are null.");
            }
        }

        public void OpenQuestDialog()
        {
           // Debug.Log("[Craftman] OpenQuestDialog()");
            var questGiver = GetComponent<QuestGiver>();
            if (questGiver != null)
            {
                questGiver.OpenQuestDialog();
            }
            else
            {
                Debug.LogWarning("[Craftman] No QuestGiver component.");
            }
        }

        public void OpenExclusiveQuestDialog()
        {
           // Debug.Log("[Craftman] OpenExclusiveQuestDialog()");
            var questGiver = GetComponent<QuestGiver>();
            if (questGiver == null)
            {
                Debug.LogWarning("[Craftman] No QuestGiver component.");
                return;
            }

            if (GUIWindowsManager.instance.dialogWindow != null)
            {
                GUIWindowsManager.instance.dialogWindow.OpenExclusiveWindow();
               // Debug.Log("[Craftman] Exclusive dialog window opened.");
            }
            else
            {
                Debug.LogError("[Craftman] dialogWindow is NULL.");
            }
        }

        protected override void OnInteract(object other)
        {
           // Debug.Log("[Craftman] OnInteract()");

            if (!(other is Entity entity))
            {
                Debug.LogWarning("[Craftman] Interacted object is not Entity.");
                return;
            }

           // Debug.Log("[Craftman] Interacted by Entity: " + entity.name);

            if (assignedDialog != null)
            {
               // Debug.Log("[Craftman] assignedDialog found.");

                if (GUIWindowsManager.instance.dialogWindow == null)
                {
                    Debug.LogError("[Craftman] dialogWindow is NULL in GUIWindowsManager!");
                    return;
                }

               // Debug.Log("[Craftman] Opening dialog window.");
                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

           // Debug.Log("[Craftman] No dialog found, opening crafting service.");
            OpenCraftingService();

            var interactionLogger = GetComponent<NpcInteractionLogger>();
            if (interactionLogger != null)
            {
                var collider = entity.GetComponent<Collider>();
                if (collider != null)
                {
                    interactionLogger.LogInteraction(collider);
                   // Debug.Log("[Craftman] Interaction logged.");
                }
                else
                {
                    Debug.LogWarning("[Craftman] Collider for interacting entity is null.");
                }
            }
            else
            {
                Debug.LogWarning("[Craftman] NpcInteractionLogger not found.");
            }
        }

        public void OpenCraftingService()
        {
           // Debug.Log("[Craftman] OpenCraftingService()");

            var craftmanWindow = GUIWindowsManager.instance.craftmanWindow;
            var inventoryWindow = GUIWindowsManager.instance.inventoryWindow;

            if (craftmanWindow == null)
            {
                Debug.LogError("[Craftman] craftmanWindow is NULL in GUIWindowsManager!");
                return;
            }

            if (inventoryWindow == null)
            {
                Debug.LogError("[Craftman] inventoryWindow is NULL in GUIWindowsManager!");
                return;
            }

           // Debug.Log("[Craftman] Showing craftmanWindow and inventoryWindow.");
            craftmanWindow.Show();
            inventoryWindow.Show();

            if (m_guiCraftman == null)
            {
                Debug.LogError("[Craftman] m_guiCraftman is NULL!");
                return;
            }

            m_guiCraftman.SetCraftman(this);
           // Debug.Log("[Craftman] GUI Craftman set and window opened.");
        }
    }
}
