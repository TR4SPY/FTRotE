using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterInventory
    {
        public int initialMoney;
        public Dictionary<ItemInstance, InventoryCell> initialItems;

        public int currentMoney => m_inventory ? m_inventory.instance.money : initialMoney;
        public Dictionary<ItemInstance, InventoryCell> currentItems =>
            m_inventory != null ? m_inventory.instance.items : initialItems;

        protected EntityInventory m_inventory;

        public CharacterInventory(Character data) => Initialize(data.inventory, data.initialMoney);

        public CharacterInventory(CharacterInventoryItem[] items, int money) => Initialize(items, money);
        public event System.Action onInventoryUpdated;

        public virtual void InitializeInventory(EntityInventory inventory)
        {
            m_inventory = inventory;
            m_inventory.instance.currency.SetFromTotalAmberlings(initialMoney);

            foreach (var item in initialItems)
            {
                var row = item.Value.row;
                var column = item.Value.column;
                m_inventory.instance.TryInsertItem(item.Key, row, column);
            }
        }

        protected virtual void Initialize(CharacterInventoryItem[] items, int money)
        {
            initialMoney = money;
            initialItems = new();

            foreach (var inventoryItem in items)
            {
                if (inventoryItem.item.data == null) continue;

                var instance = inventoryItem.item.ToItemInstance();
                initialItems.Add(instance, new(inventoryItem.row, inventoryItem.column));
            }
        }

        public static CharacterInventory CreateFromSerializer(InventorySerializer serializer)
        {
            var items = new List<CharacterInventoryItem>();

            foreach (var item in serializer.items)
            {
                var data = GameDatabase.instance.FindElementById<Item>(item.item.itemId);
                var attributes = CharacterItemAttributes.CreateFromSerializer(item.item.attributes);
                var elements = ItemElements.CreateFromSerializer(item.item.elements);

                var cItem = new CharacterItem(data, attributes, item.item.durability, item.item.stack, elements);
                cItem.itemLevel = item.item.itemLevel;
                cItem.skillEnabled = item.item.skillEnabled;
                cItem.sealType = item.item.sealType;
                cItem.effectiveness = item.item.effectiveness;

                if (data != null)
                {
                    if (data.canStack)
                        cItem.stack = Mathf.Clamp(cItem.stack, 1, data.stackCapacity);
                    else
                        cItem.stack = 1;
                }
                else
                {
                    cItem.stack = 1;
                }

                Debug.Log($"[INVENTORY LOAD] {data.name} => stack={cItem.stack}, itemLevel={cItem.itemLevel}");

                var inventoryItem = new CharacterInventoryItem(cItem, item.row, item.column);
                items.Add(inventoryItem);
            }

            return new CharacterInventory(items.ToArray(), serializer.money);
        }

        public bool HasItem(Item item)
        {
            foreach (var inventoryItem in currentItems.Keys)
            {
                if (inventoryItem.data == item)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetGold() => m_inventory != null ? m_inventory.instance.money : initialMoney;

        public void AddGold(int amount)
        {
            if (m_inventory != null)
                m_inventory.instance.AddMoney(amount);
        }

        public bool SpendGold(int amount)
        {
            if (m_inventory == null)
                return false;

            return m_inventory.instance.SpendMoney(amount);
        }

        public bool RemoveItem(Item item)
        {
            foreach (var inventoryItem in currentItems.Keys)
            {
                if (inventoryItem.data == item)
                {
                    bool removed = m_inventory.instance.TryRemoveItem(inventoryItem);
                    if (removed)
                    {
                        onInventoryUpdated?.Invoke();
                    }
                    return removed;
                }
            }
            return false;
        }
    }
}
