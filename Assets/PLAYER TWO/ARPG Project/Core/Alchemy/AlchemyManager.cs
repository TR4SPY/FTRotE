using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class AlchemyManager : CraftingManager
    {
        private void Awake()
        {
            if (availableRecipes != null)
            {
                availableRecipes = availableRecipes
                    .Where(r => r != null && r.resultItem is ItemPotion)
                    .ToList();
            }
        }

        public override bool TryCraft(List<ItemInstance> inputItems, ref ItemInstance result, ref string failReason)
        {
            var character = Game.instance?.currentCharacter;
            if (character == null || Level.instance?.player == null)
            {
                Debug.LogWarning("[AlchemyManager] No current character available. Crafting aborted.");
                failReason = "No character";
                return false;
            }

            var inventory = character.inventory;
            var playerInventory = Level.instance.player.inventory.instance;

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

            if (bestRecipe != null)
            {
                float successChance = CalculateSuccessChance(bestRecipe, inputItems);
                successChance = ApplyCatalystBonus(inputItems, successChance, bestRecipe);

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

            failReason = "No matching recipe";
            return false;
        }

        private float ApplyCatalystBonus(List<ItemInstance> inputItems, float baseChance, CraftingRecipe recipe)
        {
            int catalystCount = inputItems
                .Where(i => i.data.name.ToLower().Contains("catalyst"))
                .Sum(i => i.stack);

            if (catalystCount > 0)
            {
                baseChance += 0.05f * catalystCount;
                baseChance = Mathf.Min(baseChance, recipe.maxSuccessChance);
            }

            return baseChance;
        }
    }
}
