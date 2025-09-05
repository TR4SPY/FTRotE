using UnityEngine;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/NPC/Alchemist")]
    [RequireComponent(typeof(AlchemyManager))]
    public class Alchemist : Interactive
    {
        [System.Serializable]
        public class AlchemyItem
        {
            public Item data;
            public int attributes;
            public int elements;
        }

        [System.Serializable]
        public class Section
        {
            public string title;
            public AlchemyItem[] items;
        }

        [Header("Alchemist Settings")]
        public int rows;
        public int columns;
        public string sectionTitle = "ALCHEMY";
        public Section alchemySection;
        public Dialog assignedDialog;

        public Dictionary<string, Inventory> inventories { get; protected set; }

        protected GUIAlchemist m_guiAlchemy => GUIWindowsManager.instance.GetAlchemy();

        protected override void Start()
        {
            base.Start();
            InitializeInventory();
        }

        protected virtual void InitializeInventory()
        {
            inventories = new Dictionary<string, Inventory>();

            if (alchemySection.items != null)
            {
                var inventory = new Inventory(rows, columns);
                inventories.Add(alchemySection.title, inventory);

                foreach (var item in alchemySection.items)
                {
                    if (item.data == null)
                    {
                        Debug.LogWarning("[Alchemist] Skipping null item data.");
                        continue;
                    }

                    if (item.attributes > 0)
                        inventory.TryAddItem(new ItemInstance(item.data, true, true, item.attributes, item.attributes, item.elements, item.elements));
                    else
                        inventory.TryAddItem(new ItemInstance(item.data, false, true, 0, 0, item.elements, item.elements));
                }
            }
            else
            {
                Debug.LogWarning("[Alchemist] Section items are null.");
            }
        }

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity))
            {
                Debug.LogWarning("[Alchemist] Interacted object is not Entity.");
                return;
            }

            if (assignedDialog != null)
            {
                if (GUIWindowsManager.instance.dialogWindow == null)
                {
                    Debug.LogError("[Alchemist] dialogWindow is NULL in GUIWindowsManager!");
                    return;
                }

                GUIWindowsManager.instance.dialogWindow.Show(entity, this, assignedDialog);
                return;
            }

            OpenAlchemyService();

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
                    Debug.LogWarning("[Alchemist] Collider for interacting entity is null.");
                }
            }
            else
            {
                Debug.LogWarning("[Alchemist] NpcInteractionLogger not found.");
            }
        }

        public void OpenAlchemyService()
        {
            var alchemyWindow = GUIWindowsManager.instance.alchemyWindow;
            var inventoryWindow = GUIWindowsManager.instance.inventoryWindow;

            if (alchemyWindow == null)
            {
                Debug.LogError("[Alchemist] alchemyWindow is NULL in GUIWindowsManager!");
                return;
            }

            if (inventoryWindow == null)
            {
                Debug.LogError("[Alchemist] inventoryWindow is NULL in GUIWindowsManager!");
                return;
            }

            alchemyWindow.Show();
            inventoryWindow.Show();

            if (m_guiAlchemy == null)
            {
                Debug.LogError("[Alchemist] GUIAlchemy is NULL!");
                return;
            }

            m_guiAlchemy.SetAlchemist(this);
        }
    }
}
