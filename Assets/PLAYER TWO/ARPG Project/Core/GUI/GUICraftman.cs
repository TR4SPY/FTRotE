using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Craftman")]
    public class GUICraftman : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The prefab to use as the section, which corresponds to the inventory.")]
        public GUIInventory sectionPrefab;

        [Tooltip("A reference to the container for the section.")]
        public RectTransform sectionContainer;

        public CraftingManager craftingManager;
        public Text craftingText;

        public GUIItemSlot resultSlot;

        public AudioClip successSound;

        [Tooltip("Crafting cost Text reference.")]
        public Text priceText;

        protected Craftman m_craftman;
        protected GUIInventory m_section;

        protected Inventory m_playerInventory => Level.instance.player.inventory.instance;

        protected GUIInventory m_playerGUIInventory => GUIWindowsManager.instance.GetInventory();
        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void Awake()
        {
           // Debug.Log("[GUICraftman] Awake()");
        }

        protected virtual void OnDisable()
        {
            ReturnItemsToPlayerOrDrop();

            if (resultSlot != null && resultSlot.item != null)
            {
                var resultInstance = resultSlot.item.item;

                m_playerInventory.TryAddItem(resultInstance);

                Destroy(resultSlot.item.gameObject);
                resultSlot.Unequip();
            }

            DestroySection();
            craftingText.text = "";
            if (priceText) priceText.text = "";

            GUIWindowsManager.instance.inventoryWindow.Hide();
        }

        protected virtual void Start()
        {
           // Debug.Log("[GUICraftman] Start()");
        }

        protected virtual void InitializeSection()
        {
            if (m_craftman == null)
            {
                Debug.LogError("[GUICraftman] m_craftman is NULL!");
                return;
            }

            if (m_craftman.inventories == null)
            {
                Debug.LogError("[GUICraftman] m_craftman.inventories is NULL!");
                return;
            }

            if (string.IsNullOrEmpty(m_craftman.section.title))
            {
                Debug.LogWarning("[GUICraftman] Section title is empty.");
            }

            if (m_craftman.inventories.TryGetValue(m_craftman.section.title, out var inventory))
            {
                m_section = Instantiate(sectionPrefab, sectionContainer);
                m_section.SetInventory(inventory);
                m_section.InitializeInventory();
                m_section.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"[GUICraftman] Could not find inventory for section: {m_craftman.section.title}");
            }
        }

        public void UpdateCraftingPreview(List<ItemInstance> inputItems)
        {
            if (craftingManager == null || craftingText == null)
                return;

           // Debug.Log("[DEBUG - Preview] Items in input:");
            if (inputItems != null && inputItems.Count > 0)
            {
                foreach (var i in inputItems)
                {
                   // Debug.Log($"    => {i.GetName()} (stack={i.stack}, ID={i.GetHashCode()})");
                }
            }
            else
            {
               // Debug.Log("    => (No items or null)");
            }

            if (inputItems == null || inputItems.Count == 0)
            {
                craftingText.text = "";
                if (priceText) 
                    priceText.text = "";
                return;
            }

            if (craftingManager.availableRecipes == null || craftingManager.availableRecipes.Count == 0)
            {
                craftingText.text = "No recipes configured!";
                if (priceText) 
                    priceText.text = "";
                return;
            }

            CraftingRecipe matchedRecipe = null;
            foreach (var recipe in craftingManager.availableRecipes)
            {
                if (PartialMatches(recipe, inputItems))
                {
                    matchedRecipe = recipe;
                    break;
                }
            }

            if (matchedRecipe == null)
            {
                craftingText.text = "No matching recipe";
                if (priceText)
                    priceText.text = "";
                return;
            }

            float success = craftingManager.CalculateSuccessChance(matchedRecipe, inputItems);
            bool guaranteed = (success >= 1f);

            if (priceText)
                priceText.text = matchedRecipe.goldCost.ToString();

            string result = "";
            result += StringUtils.StringWithColorAndStyle(
                $"{matchedRecipe.resultQuantity} {matchedRecipe.resultItem.name}",
                GameColors.Lime,
                bold: true
            );
            result += "\n";

            string chanceText = StringUtils.StringWithColor(
                $"Success Rate: {(int)(success * 100)}%",
                guaranteed ? GameColors.Green : GameColors.LightBlue
            );

            result += chanceText + "\n\n";

            foreach (var ingredient in matchedRecipe.ingredients)
            {
                int available = inputItems
                    .Where(i => i.data == ingredient.item)
                    .Sum(i => i.stack);

                Color color;
                if (available == 0)
                    color = GameColors.Crimson; // none
                else if (available < ingredient.minQuantity)
                    color = GameColors.Orange; // partial
                else if (available >= ingredient.maxQuantity)
                    color = GameColors.Gold;  // full
                else
                    color = GameColors.Green; // minQuantity <= available < max

                string line = $"{ingredient.item.name} {available}/{ingredient.minQuantity}";
                result += StringUtils.StringWithColor(line, color) + "\n";
            }
/*
            if (matchedRecipe.additionalChanceItems != null && matchedRecipe.additionalChanceItems.Count > 0)
            {
                result += "\n" + StringUtils.StringWithColorAndStyle(
                    "Bonus Items:",
                    GameColors.Lime,
                    bold: true
                ) + "\n";

                foreach (var bonus in matchedRecipe.additionalChanceItems)
                {
                    int count = inputItems
                        .Where(i => i.data == bonus.item)
                        .Sum(i => i.stack);

                    Color color = (count >= bonus.quantity)
                        ? GameColors.Green
                        : GameColors.Gray;

                    string line = $"{bonus.item.name} {count}/{bonus.quantity}";
                    result += StringUtils.StringWithColor(line, color) + "\n";
                }
            }
*/

            craftingText.text = result;
        }

        private bool PartialMatches(CraftingRecipe recipe, List<ItemInstance> inputItems)
        {
            foreach (var ing in recipe.ingredients)
            {
                int count = inputItems
                    .Where(i => i.data == ing.item)
                    .Sum(i => i.stack);

                if (count > 0)
                    return true;
            }
            return false; 
        }

        private void ReturnItemsToPlayerOrDrop()
        {
            if (m_section is GUICraftmanInventory craftSection)
            {
                var leftoverItems = craftSection.inventory.items.Keys.ToList();
                foreach (var item in leftoverItems)
                {
                    bool added = m_playerInventory.TryAddItem(item);
                    if (!added)
                    {
                        Debug.Log($"[Craftman] No space in player eq => dropping {item.GetName()} on ground");
                    }

                    craftSection.inventory.TryRemoveItem(item);
                }
            }
        }
        public void OnCraftButtonClicked()
        {
            var craftSection = m_section as GUICraftmanInventory;
            if (craftSection == null)
            {
                craftingText.text = "No crafting section found.";
                return;
            }

            var craftItems = craftSection.inventory.items.Keys.ToList();
            if (craftItems.Count == 0)
            {
                craftingText.text = "You haven't placed any items for crafting.";
                return;
            }

            CraftingRecipe matchedRecipe = null;
            foreach (var recipe in craftingManager.availableRecipes)
            {
                if (craftingManager.Matches(recipe, craftItems))
                {
                    matchedRecipe = recipe;
                    break;
                }
            }

            if (matchedRecipe == null)
            {
                craftingText.text = "No matching recipe for these items.";
                return;
            }

            ItemInstance resultItem = null;
            string failReason = "";
            bool success = craftingManager.TryCraft(craftItems, ref resultItem, ref failReason);

            if (!success)
            {
                craftingText.text = failReason;
                GameAudio.instance.PlayDeniedSound();
                return;
            }

            var recipeItems = matchedRecipe.ingredients.Select(i => i.item).ToList();
            if (matchedRecipe.additionalChanceItems != null)
            {
                recipeItems.AddRange(matchedRecipe.additionalChanceItems.Select(i => i.item));
            }

            var guiItems = craftSection.GetComponentsInChildren<GUIItem>().ToList();
            foreach (var guiItem in guiItems)
            {
                var item = guiItem.item;

                if (recipeItems.Contains(item.data))
                {
                    craftSection.inventory.TryRemoveItem(item);
                    Destroy(guiItem.gameObject);
                }
                else
                {
                    if (item.stack > 0)
                    {
                        bool added = m_playerInventory.TryAddItem(item);
                        if (!added)
                        {
                            Debug.Log($"[Craftman] No space in inventory for {item.GetName()}");
                        }
                    }

                    craftSection.inventory.TryRemoveItem(item);
                    Destroy(guiItem.gameObject);
                }
            }

            craftSection.UpdateSlots();

            var guiResult = GUI.instance.CreateGUIItem(resultItem, resultSlot.transform as RectTransform);
            resultSlot.Equip(guiResult);

            if (successSound != null)
                GameAudio.instance.PlayUiEffect(successSound);

            craftingText.text = "Craft successful!";
        }

        protected virtual void DestroySection()
        {
            foreach (Transform child in sectionContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public virtual void SetCraftman(Craftman craftman)
        {
            m_craftman = craftman;
            craftingManager = m_craftman.GetComponent<CraftingManager>();

            DestroySection();
            InitializeSection();
        }

    }
}
