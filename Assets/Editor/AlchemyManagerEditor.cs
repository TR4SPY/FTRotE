using System.Linq;
using UnityEditor;
using UnityEngine;
using PLAYERTWO.ARPGProject;

[CustomEditor(typeof(AlchemyManager))]
public class AlchemyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (AlchemyManager)target;

        if (GUILayout.Button("Refresh Recipes"))
        {
            string[] guids = AssetDatabase.FindAssets("t:AlchemyRecipe");
            manager.availableRecipes = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(AssetDatabase.GUIDToAssetPath(g)))
                .Cast<CraftingRecipe>()
                .ToList();

            EditorUtility.SetDirty(manager);
        }
    }
}
