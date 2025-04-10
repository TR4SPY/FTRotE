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

        [Header("Dynamic Crafting Cost")]
        [Tooltip("Where we place the dynamic price tags for the crafting cost.")]
        public Transform craftingCostContainer;

        [Tooltip("Prefab with 'Name'(Text) and 'Icon'(Image).")]
        public GameObject priceTagPrefab;

        [Header("Currency Icons")]
        public Sprite solmireIcon;
        public Sprite lunarisIcon;
        public Sprite amberlingsIcon;

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
            ClearPriceTags(craftingCostContainer); 

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
               // Debug.LogWarning("[GUICraftman] Section title is empty.");
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

        public void UpdateCraftingPreview(List<ItemInstance> inputItems)
        {
            if (craftingManager == null || craftingText == null)
                return;

            if (inputItems == null || inputItems.Count == 0)
            {
                craftingText.text = "";
                ClearPriceTags(craftingCostContainer); 
                return;
            }

            if (craftingManager.ShouldUseCustomRules(inputItems))
            {
                foreach (var rule in craftingManager.customRules)
                {
                    if (rule.Matches(inputItems))
                    {
                        string preview = rule.GetPreview(inputItems);
                        int usedBars, returnedBars;
                        float successRate = craftingManager.GetSuccessRateFromJewels(
                            inputItems, 
                            out usedBars, 
                            out returnedBars
                        );

                        if (successRate <= 0f)
                        {
                            craftingText.text = "Insert a jewel or crystal to enhance the item.";
                            ClearPriceTags(craftingCostContainer); 
                            return;
                        }

                        string chanceText = StringUtils.StringWithColor(
                            $"Success Rate: {(int)(successRate * 100)}%",
                            successRate >= 1f ? GameColors.Green : GameColors.LightBlue
                        );

                        craftingText.text =
                            StringUtils.StringWithColorAndStyle(preview, GameColors.Lime, bold: true)
                            + "\n" + chanceText;

                        if (returnedBars > 0)
                            craftingText.text += $"\nUnused Bars returned: {returnedBars}";

                        ClearPriceTags(craftingCostContainer); 
                        return;
                    }
                }
            }

            CraftingRecipe matchedRecipe = null;
            int bestIngredientMatchCount = 0;

            foreach (var recipe in craftingManager.availableRecipes)
            {
                if (!PartialMatches(recipe, inputItems))
                    continue;

                int matchCount = recipe.ingredients.Count(i =>
                    inputItems.Any(input => input.data == i.item));

                if (matchCount > bestIngredientMatchCount)
                {
                    bestIngredientMatchCount = matchCount;
                    matchedRecipe = recipe;
                }
            }

            if (matchedRecipe != null)
            {
                float success = craftingManager.CalculateSuccessChance(matchedRecipe, inputItems);
                bool guaranteed = (success >= 1f);

                ShowPriceTags(craftingCostContainer, matchedRecipe.goldCost);

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
                        color = GameColors.Crimson;
                    else if (available < ingredient.minQuantity)
                        color = GameColors.Orange;
                    else if (available >= ingredient.maxQuantity)
                        color = GameColors.Gold;
                    else
                        color = GameColors.Green;

                    string line = $"{ingredient.item.name} {available}/{ingredient.minQuantity}";
                    result += StringUtils.StringWithColor(line, color) + "\n";
                }

                craftingText.text = result;
                return;
            }

            if (inputItems.Any(i => i.IsEquippable()))
            {
                craftingText.text = "Insert a jewel or crystal to enhance the item.";
            }
            else
            {
                craftingText.text = "No matching recipe or enhancement found.";
            }

            ClearPriceTags(craftingCostContainer);
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

            var matchedRule = craftingManager.GetMatchedRule(craftItems);

            ItemInstance resultItem = null;
            string failReason = "";
            bool success = craftingManager.TryCraft(craftItems, ref resultItem, ref failReason);

            if (!success)
            {
                craftingText.text = failReason;
                GameAudio.instance.PlayDeniedSound();
                return;
            }

            var guiItems = craftSection.GetComponentsInChildren<GUIItem>().ToList();

            foreach (var guiItem in guiItems)
            {
                var item = guiItem.item;

                if (matchedRule != null)
                {
                    craftSection.inventory.TryRemoveItem(item);
                    Destroy(guiItem.gameObject);
                    continue;
                }

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
