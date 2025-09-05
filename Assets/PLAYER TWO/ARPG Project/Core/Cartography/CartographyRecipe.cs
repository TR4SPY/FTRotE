using UnityEngine;

[CreateAssetMenu(fileName = "NewCartographyRecipe", menuName = "PLAYER TWO/ARPG Project/Cartography/Recipe")]
public class CartographyRecipe : ScriptableObject
{
    public PLAYERTWO.ARPGProject.Item fragmentItem;
    public int requiredFragments = 5;
    public PLAYERTWO.ARPGProject.Item resultMap;
}
