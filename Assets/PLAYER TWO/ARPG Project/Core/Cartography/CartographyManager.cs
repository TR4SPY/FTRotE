using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class CartographyManager : MonoBehaviour
    {
        public List<CartographyRecipe> recipes = new();

        public bool TryAssemble(List<ItemInstance> fragments, ref ItemInstance result)
        {
            foreach (var recipe in recipes)
            {
                if (recipe.fragmentItem == null || recipe.resultMap == null)
                    continue;

                int count = fragments.Sum(f => f.data == recipe.fragmentItem ? f.stack : 0);
                if (count < recipe.requiredFragments)
                    continue;

                int remaining = recipe.requiredFragments;
                for (int i = fragments.Count - 1; i >= 0 && remaining > 0; i--)
                {
                    var frag = fragments[i];
                    if (frag.data != recipe.fragmentItem)
                        continue;

                    int remove = Mathf.Min(frag.stack, remaining);
                    frag.stack -= remove;
                    remaining -= remove;
                    if (frag.stack <= 0)
                        fragments.RemoveAt(i);
                }

                result = new ItemInstance(recipe.resultMap, false);
                return true;
            }

            return false;
        }
    }
}
