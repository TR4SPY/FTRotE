using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "PLAYER TWO/ARPG Project/Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public PLAYERTWO.ARPGProject.Item item;
        public int minQuantity;
        public int maxQuantity;
    }

    [System.Serializable]
    public class BonusIngredient
    {
        public PLAYERTWO.ARPGProject.Item item;
        public int quantity = 1;
    }

    public List<Ingredient> ingredients = new();
    public List<BonusIngredient> additionalChanceItems = new();

    public PLAYERTWO.ARPGProject.Item resultItem;
    public int resultQuantity = 1;

    public int goldCost = 0;

    [Range(0f, 1f)]
    public float baseSuccessChance = 0.5f; 
    [Range(0f, 1f)]
    public float maxSuccessChance = 0.95f;
}
