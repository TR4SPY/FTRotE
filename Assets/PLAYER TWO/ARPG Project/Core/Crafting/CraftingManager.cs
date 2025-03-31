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

            foreach (var rule in customRules)
            {
                if (!rule.Matches(inputItems))
                    continue;

                result = rule.Craft(inputItems);
                failReason = "";
                return true;
            }

            failReason = "No matching recipe or upgrade rule";
            return false;
        }

        public bool Matches(CraftingRecipe recipe, List<ItemInstance> input)
        {
            var inputDict = new Dictionary<PLAYERTWO.ARPGProject.Item, int>();

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