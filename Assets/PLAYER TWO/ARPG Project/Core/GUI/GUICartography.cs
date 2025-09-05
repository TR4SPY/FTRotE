using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Cartography")]
    public class GUICartography : MonoBehaviour
    {
        public CartographyManager cartographyManager;
        public Transform recipeContainer;
        public Button recipeButtonPrefab;
        public GUICartographySlot[] fragmentSlots;
        public GUIItemSlot resultSlot;
        public Button createButton;

        private CartographyRecipe m_selectedRecipe;
        private Inventory m_playerInventory => Level.instance.player.inventory.instance;

        void OnEnable()
        {
            GUIWindowsManager.instance.inventoryWindow.Show();
            foreach (var slot in fragmentSlots)
            {
                slot.onEquip.AddListener(OnSlotChanged);
                slot.onUnequip.AddListener(OnSlotChanged);
            }
            RefreshRecipes();
            RefreshState();
        }

        void OnDisable()
        {
            foreach (var slot in fragmentSlots)
            {
                slot.onEquip.RemoveListener(OnSlotChanged);
                slot.onUnequip.RemoveListener(OnSlotChanged);
            }
            ReturnItems();
            GUIWindowsManager.instance.inventoryWindow.Hide();
        }

        public void SetManager(CartographyManager manager)
        {
            cartographyManager = manager;
            RefreshRecipes();
        }

        void RefreshRecipes()
        {
            if (!recipeContainer || recipeButtonPrefab == null || cartographyManager == null)
                return;

            foreach (Transform child in recipeContainer)
                Destroy(child.gameObject);

            foreach (var recipe in cartographyManager.recipes)
            {
                var btn = Instantiate(recipeButtonPrefab, recipeContainer);
                var text = btn.GetComponentInChildren<Text>();
                if (text) text.text = recipe.resultMap != null ? recipe.resultMap.name : "Recipe";
                btn.onClick.AddListener(() => SelectRecipe(recipe));
            }
        }

        void SelectRecipe(CartographyRecipe recipe)
        {
            m_selectedRecipe = recipe;
            ReturnItems();
            RefreshState();
        }

        void OnSlotChanged(GUIItem item)
        {
            RefreshState();
        }

        void RefreshState()
        {
            if (createButton != null)
                createButton.interactable = CanAssemble();
        }

        bool CanAssemble()
        {
            if (m_selectedRecipe == null)
                return false;

            int count = 0;
            foreach (var slot in fragmentSlots)
            {
                if (slot.item == null)
                    return false;
                if (slot.item.item.data != m_selectedRecipe.fragmentItem)
                    return false;
                count++;
            }
            return count >= m_selectedRecipe.requiredFragments;
        }

        public void OnCreateMap()
        {
            if (!CanAssemble())
            {
                GameAudio.instance?.PlayDeniedSound();
                return;
            }

            var fragments = fragmentSlots.Where(s => s.item != null).Select(s => s.item.item).ToList();
            ItemInstance result = null;
            if (!cartographyManager.TryAssemble(fragments, ref result))
            {
                GameAudio.instance?.PlayDeniedSound();
                return;
            }

            foreach (var slot in fragmentSlots)
            {
                if (slot.item != null)
                {
                    Destroy(slot.item.gameObject);
                    slot.Unequip();
                }
            }

            if (resultSlot.item != null)
                resultSlot.Unequip();

            if (result != null)
            {
                var guiResult = GUI.instance.CreateGUIItem(result, resultSlot.transform as RectTransform);
                resultSlot.Equip(guiResult);
            }

            RefreshState();
        }

        void ReturnItems()
        {
            foreach (var slot in fragmentSlots)
            {
                if (slot.item == null) continue;
                var inst = slot.item.item;
                bool added = m_playerInventory.TryAddItem(inst);
                if (!added)
                    GUI.instance.DropItem(inst);
                Destroy(slot.item.gameObject);
                slot.Unequip();
            }

            if (resultSlot != null && resultSlot.item != null)
            {
                var inst = resultSlot.item.item;
                m_playerInventory.TryAddItem(inst);
                Destroy(resultSlot.item.gameObject);
                resultSlot.Unequip();
            }
        }
    }
}
