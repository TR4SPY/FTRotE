using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    public class CraftingManager : MonoBehaviour
    {
        public List<CraftingRecipe> availableRecipes;
        public List<CraftingRules> customRules = new();
        private void Start()
        {
            customRules = new List<CraftingRules>
            {
                new JewelOfEmbersRule(),
                new JewelOfLightRule(),
                new JewelOfClarityRule(),
                new OceanCrystalRule(),
                new CrystalOfRefractionRule(),
                new DuskCrystalRule(),
                new HeartlyCrystalRule(),
                new JewelOfVerdancyRule()
            };
        }

        public bool TryCraft(List<ItemInstance> inputItems, ref ItemInstance result, ref string failReason)
        {
            var character = Game.instance.currentCharacter;
            var inventory = character.inventory;
            var playerInventory = Level.instance.player.inventory.instance;

            CraftingRules bestRule = null;
            float bestRuleScore = 0f;

            if (ShouldUseCustomRules(inputItems))
            {
                foreach (var rule in customRules)
                {
                    if (!rule.Matches(inputItems))
                        continue;

                    float successRate = GetSuccessRateFromJewels(inputItems, out _, out _);
                    if (successRate <= 0f)
                        continue;

                    float score = GetRuleScore(rule, inputItems);
                    if (score > bestRuleScore)
                    {
                        bestRuleScore = score;
                        bestRule = rule;
                    }
                }
            }

            CraftingRecipe bestRecipe = null;
            float bestRecipeScore = 0f;

            foreach (var recipe in availableRecipes)
            {
                if (!Matches(recipe, inputItems))
                    continue;

                if (inventory.GetGold() < recipe.goldCost)
                    continue;

                float recipeScore = GetRecipeScore(recipe, inputItems);
                if (recipeScore > bestRecipeScore)
                {
                    bestRecipeScore = recipeScore;
                    bestRecipe = recipe;
                }
            }

            if (bestRule != null && bestRuleScore >= bestRecipeScore)
            {
                var bars = inputItems.Where(i => i.data.name == "Bar of Solmire").ToList();
                float successRate = GetSuccessRateFromJewels(inputItems, out int barUsed, out int barReturned);

                if (successRate <= 0f)
                {
                    failReason = "Insert a jewel or crystal to enhance the item.";
                    return false;
                }

                // Debug.Log($"[DEBUG:TryCraft] Using rule: {bestRule.GetType().Name}");
                var eqItem = inputItems.FirstOrDefault(i => i.IsEquippable());
                var crystals = inputItems.Where(i => i.data.name == "Crystal of Refraction").Sum(i => i.stack);
                // Debug.Log($"[DEBUG:TryCraft] eqItem = {eqItem.GetName()}, itemLevel={eqItem.itemLevel}, crystals={crystals}");

                if (Random.value > successRate)
                {
                    var originalItemsOnFail = new List<ItemInstance>(inputItems);
                    var enhancedItem = inputItems.FirstOrDefault(i => i.IsEquippable());

                    bestRule.OnFail(enhancedItem);
                    result = enhancedItem;

                    RemoveBars(bars, barUsed);

                    foreach (var it in originalItemsOnFail)
                    {
                        // Debug.Log($"[DEBUG:TryCraft] originalItemsOnFail => {it.GetName()} (stack={it.stack})");
                    }

                    bestRule.ConsumeUsedItems(originalItemsOnFail, inputItems);

                    foreach (var it in originalItemsOnFail)
                    {
                        // Debug.Log($"[DEBUG:TryCraft] leftover => {it.GetName()} (stack={it.stack})");
                    }

                    foreach (var leftover in originalItemsOnFail)
                    {
                        // Debug.Log($"[DEBUG:TryCraft] returning leftover => {leftover.GetName()} (stack={leftover.stack}) to inventory");

                        playerInventory.TryAddItem(leftover);
                    }

                    failReason = StringUtils.StringWithColor("Enhancement failed!", GameColors.Crimson);
                    return false;
                }

                RemoveBars(bars, barUsed);

                for (int i = 0; i < barReturned; i++)
                {
                    var barItem = new ItemInstance(bars[0].data, false);
                    bool added = playerInventory.TryAddItem(barItem);
                    if (!added)
                    {
                        GUI.instance.DropItem(barItem);
                    }
                }

                var originalItemsSuccess = new List<ItemInstance>(inputItems);
                result = bestRule.Craft(inputItems);

                if (result == null)
                {
                    var enhancedItem = inputItems.FirstOrDefault(i => i.IsEquippable());
                    bestRule.OnFail(enhancedItem);

                    bestRule.ConsumeUsedItems(originalItemsSuccess, inputItems);

                    foreach (var leftover in originalItemsSuccess)
                        playerInventory.TryAddItem(leftover);

                    failReason = StringUtils.StringWithColor(
                        "Enhancement failed!", GameColors.Crimson);
                    return false;
                }

                bestRule.ConsumeUsedItems(originalItemsSuccess, inputItems);
                foreach (var leftover in originalItemsSuccess)
                    playerInventory.TryAddItem(leftover);

                failReason = "";
                return true;
            }
            else if (bestRecipe != null)
            {
                float successChance = CalculateSuccessChance(bestRecipe, inputItems);
                bool isGuaranteed = successChance >= 1f;

                if (!isGuaranteed && Random.value > successChance)
                {
                    inventory.SpendGold(bestRecipe.goldCost);
                    failReason = "Crafting failed";
                    return false;
                }

                inventory.SpendGold(bestRecipe.goldCost);
                ConsumeIngredients(inputItems, bestRecipe);

                var unusedItems = inputItems.Where(i => i.stack > 0).ToList();
                foreach (var leftover in unusedItems)
                {
                    playerInventory.TryAddItem(leftover);
                }

                inputItems.Clear();
                result = new ItemInstance(bestRecipe.resultItem, false);

                failReason = "";
                return true;
            }

            failReason = "No matching recipe or upgrade rule";
            return false;
        }

        public float GetRecipeScore(CraftingRecipe recipe, List<ItemInstance> input)
        {
            float score = 0f;

            foreach (var ing in recipe.ingredients)
            {
                int available = input
                    .Where(i => i.data == ing.item)
                    .Sum(i => i.stack);

                if (available < ing.minQuantity)
                    return 0f;

                int used = Mathf.Min(available, ing.maxQuantity);
                score += (float)(used - ing.minQuantity + 1) / (ing.maxQuantity - ing.minQuantity + 1);
            }

            return score;
        }
        public float GetRuleScore(CraftingRules rule, List<ItemInstance> input)
        {
            if (!rule.Matches(input))
                return 0f;

            int jewelCount = input
                .Where(i => i.data is ItemJewel && i.data.name != "Bar of Solmire")
                .Sum(i => i.stack);

            return Mathf.Clamp01(jewelCount / 3f);
        }
        public bool ShouldUseCustomRules(List<ItemInstance> inputItems)
        {
            var jewelTypes = inputItems
                .Where(i => i.data is ItemJewel && i.data.name != "Bar of Solmire")
                .Select(i => i.data.name)
                .Distinct()
                .ToList();

            return jewelTypes.Count <= 1;
        }
        public bool Matches(CraftingRecipe recipe, List<ItemInstance> input)
        {
            var inputDict = new Dictionary<Item, int>();

            foreach (var item in input)
            {
                if (!inputDict.ContainsKey(item.data))
                    inputDict[item.data] = 0;

                inputDict[item.data] += item.stack;
            }

            foreach (var ingredient in recipe.ingredients)
            {
                if (!inputDict.ContainsKey(ingredient.item))
                {
                    // Debug.Log($"[DEBUG] Missing ingredient: {ingredient.item.name} for recipe {recipe.resultItem.name}");
                    return false;
                }

                if (inputDict[ingredient.item] < ingredient.minQuantity)
                {
                    // Debug.Log($"[DEBUG] Not enough of ingredient: {ingredient.item.name}. Have {inputDict[ingredient.item]}, need {ingredient.minQuantity}");
                    return false;
                }
            }

            // Debug.Log($"[DEBUG] Recipe {recipe.resultItem.name} matches all ingredients.");
            return true;
        }
        public bool LooseMatches(CraftingRecipe recipe, List<ItemInstance> input)
        {
            bool hasAnyMatchingIngredient = false;

            foreach (var ingredient in recipe.ingredients)
            {
                int available = input
                    .Where(i => i.data == ingredient.item)
                    .Sum(i => i.stack);

                if (available > 0)
                {
                    hasAnyMatchingIngredient = true;
                }
            }

            return hasAnyMatchingIngredient;
        }
        public CraftingRecipe GetBestRecipeForDisplay(List<ItemInstance> inputItems)
        {
            CraftingRecipe bestRecipe = null;
            float bestScore = 0f;

            foreach (var recipe in availableRecipes)
            {
                if (!Matches(recipe, inputItems))
                    continue;

                float score = GetRecipeScore(recipe, inputItems);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRecipe = recipe;
                }
            }

            return bestRecipe;
        }
        public float CalculateSuccessChance(CraftingRecipe recipe, List<ItemInstance> input)
        {
            float totalRatio = 0f;
            int validIngredients = 0;

            foreach (var ing in recipe.ingredients)
            {
                int available = input
                    .Where(i => i.data == ing.item)
                    .Sum(i => i.stack);

                if (available < ing.minQuantity)
                    return 0f;

                float ratio = Mathf.Clamp01(
                    (float)(available - ing.minQuantity) / Mathf.Max(1, (ing.maxQuantity - ing.minQuantity))
                );

                totalRatio += ratio;
                validIngredients++;
            }

            float baseChance = Mathf.Lerp(
                recipe.baseSuccessChance,
                recipe.maxSuccessChance,
                (validIngredients > 0) ? (totalRatio / validIngredients) : 0f
            );

            foreach (var bonus in recipe.additionalChanceItems)
            {
                int count = input
                    .Where(i => i.data == bonus.item)
                    .Sum(i => i.stack);

                if (count >= bonus.quantity)
                    return 1f;
            }

            return baseChance;
        }
        private void RemoveBars(List<ItemInstance> bars, int amountToRemove)
        {
            int remaining = amountToRemove;
            foreach (var bar in bars)
            {
                int toTake = Mathf.Min(remaining, bar.stack);
                bar.stack -= toTake;
                remaining -= toTake;
                if (remaining <= 0) break;
            }
        }
        public float GetSuccessRateFromJewels(List<ItemInstance> input, out int barOfSolmireUsed, out int barOfSolmireReturned)
        {
            barOfSolmireUsed = 0;
            barOfSolmireReturned = 0;

            var jewels = input.Where(i => i.data is ItemJewel).Select(i => i.data as ItemJewel).ToList();

            bool hasEquippable = input.Any(i => i.IsEquippable());
            bool hasValidJewel = jewels.Any(j => j.name != "Bar of Solmire");

            if (!hasEquippable || !hasValidJewel)
                return 0f;

            var distinctJewels = jewels
                .Where(j => j.name != "Bar of Solmire")
                .Select(j => j.name)
                .Distinct()
                .ToList();

            if (distinctJewels.Count != 1)
                return 0f;

            var primaryJewel = jewels.First(j => j.name == distinctJewels[0]);
            var barsOfSolmire = input.Where(i => i.data.name == "Bar of Solmire").ToList();
            int barsCount = barsOfSolmire.Sum(b => b.stack);

            float baseRate = primaryJewel != null ? primaryJewel.successRate : 0f;
            float bonusRate = barsCount * 10f;

            float totalRate = baseRate + bonusRate;

            if (totalRate >= 110f)
            {
                barOfSolmireReturned = Mathf.FloorToInt((Mathf.Floor(totalRate) - 100f) / 10f);
            }

            barOfSolmireUsed = Mathf.Clamp(barsCount - barOfSolmireReturned, 0, barsCount);

            return Mathf.Clamp01(totalRate / 100f);
        }
        public CraftingRules GetMatchedRule(List<ItemInstance> inputItems)
        {
            return customRules.FirstOrDefault(rule => rule.Matches(inputItems));
        }
        private void ConsumeIngredients(List<ItemInstance> input, CraftingRecipe recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                int remaining = ingredient.minQuantity;

                for (int i = 0; i < input.Count && remaining > 0; i++)
                {
                    if (input[i].data != ingredient.item) continue;

                    int toRemove = Mathf.Min(input[i].stack, remaining);
                    input[i].stack -= toRemove;
                    remaining -= toRemove;
                }
            }
            input.RemoveAll(i => i.stack <= 0);
        }
    }
}
