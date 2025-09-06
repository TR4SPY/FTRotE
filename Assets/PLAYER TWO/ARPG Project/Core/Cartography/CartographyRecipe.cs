using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "NewCartographyRecipe", menuName = "PLAYER TWO/ARPG Project/Cartography/Recipe")]
    public class CartographyRecipe : ScriptableObject
    {
        public Item fragmentItem;
        public int requiredFragments = 5;
        public Item resultMap;
    }
}
