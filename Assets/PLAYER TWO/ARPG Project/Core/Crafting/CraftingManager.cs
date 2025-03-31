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

            if (ShouldUseCustomRules(inputItems))
            {
                foreach (var rule in customRules)
                {
                    if (!rule.Matches(inputItems))
                        continue;

                    int barUsed, barReturned;
                    float successRate = GetSuccessRateFromJewels(inputItems, out barUsed, out barReturned);

                    var bars = inputItems.Where(i => i.data.name == "Bar of Solmire").ToList();

                    if (successRate <= 0f)
                    {
                        failReason = "Insert a jewel or crystal to enhance the item.";
                        return false;
                    }

                    if (Random.value > successRate)
                    {
                        var usedJewel = inputItems.FirstOrDefault(i =>
                            i.data is ItemJewel && i.data.name != "Bar of Solmire");

                        if (usedJewel != null)
                            usedJewel.stack--;

                        RemoveBars(bars, barUsed);

                        var enhancedItem = inputItems.FirstOrDefault(i => i.IsEquippable());
                        rule.OnFail(enhancedItem);

                        failReason = "<color=#cc4444>Enhancement failed!</color>";
                        return false;
                    }

                    RemoveBars(bars, barUsed);

                    for (int i = 0; i < barReturned; i++)
                    {
                        var barItem = new ItemInstance(bars[0].data, false);
                        bool added = playerInventory.TryAddItem(barItem);
                        if (!added)
                        {
                            Debug.LogWarning("[CraftingManager] Couldn't return Bar of Solmire (no space).");
                        }
                    }

                    result = rule.Craft(inputItems);
                    failReason = "";
                    return true;
                }
            }

            foreach (var recipe in availableRecipes)
            {
                if (!Matches(recipe, inputItems))
                    continue;

                if (inventory.GetGold() < recipe.goldCost)
                {
                    failReason = "Not enough gold";
                    return false;
                }

                float successChance = CalculateSuccessChance(recipe, inputItems);
                bool isGuaranteed = successChance >= 1f;

                if (!isGuaranteed && Random.value > successChance)
                {
                    inventory.SpendGold(recipe.goldCost);
                    failReason = "Crafting failed";
                    return false;
                }

                inventory.SpendGold(recipe.goldCost);
                ConsumeIngredients(inputItems, recipe);
                result = new ItemInstance(recipe.resultItem, false);
                return true;
            }

            failReason = "No matching recipe or upgrade rule";
            return false;
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
                if (!inputDict.ContainsKey(ingredient.item) || inputDict[ingredient.item] < ingredient.minQuantity)
                    return false;
            }

            return true;
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

            if (!input.Any(i => i.IsEquippable()) || jewels.Count == 0)
                return 0f;

            var primaryJewel = jewels.FirstOrDefault(j => j.name != "Bar of Solmire");
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
