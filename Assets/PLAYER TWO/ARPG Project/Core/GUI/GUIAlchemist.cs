using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/GUI/GUI Alchemist")]
    public class GUIAlchemist : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The prefab to use as the section, which corresponds to the inventory.")]
        public GUIInventory sectionPrefab;

        [Tooltip("A reference to the container for the section.")]
        public RectTransform sectionContainer;

        public CraftingManager craftingManager;
        public Text craftingText;
        public GUIItemSlot resultSlot;
        public Button craftButton;

        [Header("Dynamic Crafting Cost")]
        [Tooltip("Where we place the dynamic price tags for the crafting cost.")]
        public Transform craftingCostContainer;

        [Tooltip("Prefab with 'Name'(Text) and 'Icon'(Image).")]
        public GameObject priceTagPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

        protected Alchemist m_alchemist;
        protected GUIInventory m_section;
        protected Inventory m_playerInventory => Level.instance.player.inventory.instance;

        private bool canCraft = false;

        protected virtual void InitializeSection()
        {
            if (m_alchemist == null)
                return;

            if (m_alchemist.inventories == null)
                return;

            if (m_alchemist.inventories.TryGetValue(m_alchemist.alchemySection.title, out var inventory))
            {
                m_section = Instantiate(sectionPrefab, sectionContainer);
                m_section.SetInventory(inventory);
                m_section.InitializeInventory();
                m_section.gameObject.SetActive(true);
            }
        }

        private string FormatCurrency(int totalAmberlings)
        {
            var currency = new Currency();
            currency.SetFromTotalAmberlings(totalAmberlings);
            return currency.ToString();
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

        protected virtual void DestroySection()
        {
            foreach (Transform child in sectionContainer)
                Destroy(child.gameObject);
        }

        public virtual void SetAlchemist(Alchemist alchemist)
        {
            m_alchemist = alchemist;
            craftingManager = m_alchemist.GetComponent<CraftingManager>();

            DestroySection();
            InitializeSection();
        }

        public void UpdateAlchemyPreview(List<ItemInstance> inputItems)
        {
            canCraft = false;

            if (craftingManager == null || craftingText == null)
                return;

            craftingText.text = "";
            ClearPriceTags(craftingCostContainer);

            if (inputItems == null || inputItems.Count == 0)
            {
                craftingText.text = "You haven't placed any items for alchemy.";
                if (craftButton != null) craftButton.interactable = false;
                return;
            }

            var exactRecipe = craftingManager.GetBestRecipeForDisplay(inputItems);
            if (exactRecipe != null && exactRecipe.resultItem is ItemPotion)
            {
                DisplayRecipe(exactRecipe, inputItems);
                canCraft = true;
                if (craftButton != null) craftButton.interactable = true;
                return;
            }

            var potionRecipes = craftingManager.availableRecipes
                .Where(r => r.resultItem is ItemPotion)
                .Select(recipe =>
                {
                    int matched = 0;
                    int total = recipe.ingredients.Count;

                    foreach (var ing in recipe.ingredients)
                    {
                        int count = inputItems.Where(i => i.data == ing.item).Sum(i => i.stack);
                        if (count >= ing.minQuantity)
                            matched++;
                    }

                    return (recipe, matched, total);
                })
                .Where(x => x.matched >= 2)
                .OrderByDescending(x => (float)x.matched / x.total)
                .ToList();

            var bestPartial = potionRecipes.FirstOrDefault();
            if (bestPartial.recipe != null)
            {
                DisplayRecipe(bestPartial.recipe, inputItems);
                canCraft = true;
                if (craftButton != null) craftButton.interactable = true;
                return;
            }

            craftingText.text = "No matching potion recipe found.";
            if (craftButton != null) craftButton.interactable = false;
        }

        private void DisplayRecipe(CraftingRecipe recipe, List<ItemInstance> inputItems)
        {
            float success = craftingManager.CalculateSuccessChance(recipe, inputItems);
            float failure = 1f - success;
            bool guaranteed = (success >= 1f);

            ShowPriceTags(craftingCostContainer, recipe.goldCost);

            string result = StringUtils.StringWithColorAndStyle(
                $"{recipe.resultQuantity} {recipe.resultItem.name}",
                GameColors.Lime,
                bold: true
            ) + "\n";

            string chanceText = StringUtils.StringWithColor(
                $"Success Rate: {Mathf.RoundToInt(success * 100)}% / Failure Rate: {Mathf.RoundToInt(failure * 100)}%",
                guaranteed ? GameColors.Green : GameColors.LightBlue
            );

            result += chanceText + "\n\n";

            foreach (var ingredient in recipe.ingredients)
            {
                int available = inputItems
                    .Where(i => i.data == ingredient.item)
                    .Sum(i => i.stack);

                Color color;
                if (available == 0)
                    color = GameColors.Crimson;
                else if (available < ingredient.minQuantity)
                    color = GameColors.Orange;
                else if (available >= ingredient.maxQuantity)
                    color = GameColors.Gold;
                else
                    color = GameColors.Green;

                string line = $"{ingredient.item.name} ({available} of {ingredient.minQuantity}–{ingredient.maxQuantity})";
                result += StringUtils.StringWithColor(line, color) + "\n";
            }

            craftingText.text = result;
        }

        public void Craft()
        {
            if (m_section == null)
            {
                craftingText.text = "No alchemy section found.";
                GameAudio.instance?.PlayDeniedSound();
                return;
            }

            var guiItems = m_section.GetComponentsInChildren<GUIItem>().ToList();
            var craftItems = guiItems.Select(i => i.item).Where(i => i != null).ToList();

            if (!craftItems.Any())
            {
                craftingText.text = "You haven't placed any items for alchemy.";
                GameAudio.instance?.PlayDeniedSound();
                return;
            }

            if (!canCraft)
            {
                craftingText.text = "Crafting is not available with current combination.";
                GameAudio.instance?.PlayDeniedSound();
                return;
            }

            ItemInstance resultItem = null;
            string failReason = "";
            bool success = craftingManager.TryCraft(craftItems, ref resultItem, ref failReason);

            foreach (var guiItem in guiItems)
            {
                m_section.TryRemove(guiItem);
                Destroy(guiItem.gameObject);
            }

            m_section.UpdateSlots();

            if (resultSlot.item != null)
                resultSlot.Unequip();

            if (resultItem != null)
            {
                var guiResult = GUI.instance.CreateGUIItem(resultItem, resultSlot.transform as RectTransform);
                resultSlot.Equip(guiResult);
            }

            if (!success)
            {
                craftingText.text = failReason;
                GameAudio.instance?.PlayDeniedSound();
            }
            else
            {
                craftingText.text = "Craft successful!";
            }
        }
    }
}

