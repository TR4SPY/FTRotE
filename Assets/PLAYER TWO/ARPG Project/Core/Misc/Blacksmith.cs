//  ZMODYFIKOWANO 31 GRUDNIA 2024
using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/NPC/Blacksmith")]
    public class Blacksmith : Interactive
    {
        [Header("Item Repair Settings")]
        [Tooltip("Minimum cost of repairing an Item.")]
        public int minPrice;

        [Tooltip("Maximum cost of repairing an Item.")]
        public int maxPrice;

        protected Entity m_entity;

        protected GUIBlacksmith m_blacksmithWindow => GUIWindowsManager.instance.blacksmith;

        /// <summary>
        /// Tries to repair a given Item Instance.
        /// </summary>
        /// <param name="item">The Item Instance you're trying to repair.</param>
        /// <returns>Returns true if the Item Instance was successfully repaired.</returns>
        public virtual bool TryRepair(ItemInstance item)
        {
            var price = GetPriceToRepair(item);

            if (m_entity.inventory.instance.money < price) return false;

            item.Repair();
            m_entity.inventory.instance.money -= price;
            return true;
        }

        /// <summary>
        /// Tries to repair all the items from the Entity inventory.
        /// </summary>
        /// <returns>Returns true if the items were repaired.</returns>
        public virtual bool TryRepairAll()
        {
            var price = GetPriceToRepairAll();

            if (m_entity.inventory.instance.money < price) return false;

            foreach (var item in m_entity.inventory.instance.items)
                item.Key.Repair();

            foreach (var item in m_entity.items.GetEquippedItems())
                item.Repair();

            m_entity.inventory.instance.money -= price;
            return true;
        }

        /// <summary>
        /// Returns the total cost to repair a given Item Instance.
        /// </summary>
        /// <param name="item">The Item Instance you want to get the cost from.</param>
        public virtual int GetPriceToRepair(ItemInstance item)
        {
            if (item == null) return 0;

            var durability = item.GetDurabilityRate();

            if (durability == 1) return 0;

            return (int)Mathf.Lerp(maxPrice, minPrice, durability);
        }

        /// <summary>
        /// Returns the total cost to repair all items from the Entity's inventory.
        /// </summary>
        public virtual int GetPriceToRepairAll()
        {
            if (!m_entity) return 0;

            var total = 0;

            foreach (var item in m_entity.inventory.instance.items)
                total += GetPriceToRepair(item.Key);

            foreach (var item in m_entity.items.GetEquippedItems())
                total += GetPriceToRepair(item);

            return total;
        }

        protected override void OnInteract(object other)
        {
            if (!(other is Entity entity)) return;

            if (m_blacksmithWindow == null)
            {
                Debug.LogWarning("Blacksmith.OnInteract: m_blacksmithWindow is null! Ensure it is assigned in the inspector.");
                return;
            }

            if (entity.inventory == null)
            {
                Debug.LogWarning("Blacksmith.OnInteract: Entity has no inventory! Skipping interaction.");
                return;
            }

            if (entity != m_entity)
            {
                m_entity = entity;

                m_entity.inventory.onItemAdded.AddListener((_) => m_blacksmithWindow.Refresh());
                m_entity.inventory.onItemInserted.AddListener((_) => m_blacksmithWindow.Refresh());
                m_entity.inventory.onItemRemoved.AddListener(m_blacksmithWindow.Refresh);
                m_entity.items.onChanged.AddListener(m_blacksmithWindow.Refresh);
            }

            // Logowanie interakcji przy użyciu Collidera
            var interactionLogger = GetComponent<NpcInteractionLogger>();
            if (interactionLogger != null)
            {
                // Jeśli `other` jest obiektem gracza lub Agenta AI, pobierz jego Collider
                var collider = entity.GetComponent<Collider>();
                if (collider != null)
                {
                    interactionLogger.LogInteraction(collider); // Przekazujemy Collider
                }
                else
                {
                    Debug.LogWarning("Collider for interacting entity is null. Cannot log interaction.");
                }
            }
            else
            {
                Debug.LogWarning("NpcInteractionLogger not found on Blacksmith. Cannot log interaction.");
            }

            // Otwórz UI tylko dla gracza
            if (entity.isPlayer)
            {
                m_blacksmithWindow.Show(this);
                Debug.Log("Blacksmith interacted with by player. UI opened.");
            }
            else
            {
                Debug.Log("Blacksmith interacted with by AI Agent. UI not opened.");
            }
        }

    }
}
