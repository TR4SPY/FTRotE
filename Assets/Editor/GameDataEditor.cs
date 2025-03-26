using UnityEditor;
using UnityEngine;
using System.Linq;
using PLAYERTWO.ARPGProject;

[CustomEditor(typeof(GameData))]
public class GameDataEditor : Editor
{
    private bool showItems = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameData data = (GameData)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("📦 Przedmioty w grze", EditorStyles.boldLabel);

        if (GUILayout.Button("🔄 Synchronizuj ID z grupami"))
        {
            data.AssignItemIDs();
            data.items = data.items
                .Where(i => i != null)
                .OrderBy(i => (int)i.group)
                .ThenBy(i => i.id)
                .ToList();

            EditorUtility.SetDirty(data);
        }

        if (GUILayout.Button("📤 Zrób backup (eksport do JSON)"))
        {
            string path = EditorUtility.SaveFilePanel("Zapisz backup GameData", "Assets", "GameDataBackup", "json");
            if (!string.IsNullOrEmpty(path))
            {
                var backupData = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(path, backupData);
                Debug.Log($"✅ GameData zapisany do: {path}");
            }
        }

        if (GUILayout.Button("📥 Wczytaj backup (import z JSON)"))
        {
            string path = EditorUtility.OpenFilePanel("Wczytaj backup GameData", "Assets", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, data);
                EditorUtility.SetDirty(data);
                Debug.Log($"✅ GameData wczytany z: {path}");
            }
        }

        showItems = EditorGUILayout.Foldout(showItems, "🧾 Rozwiń listę przedmiotów");

        if (showItems)
        {
            Item.ItemGroup? lastGroup = null;

            foreach (var item in data.items.Where(i => i != null).OrderBy(i => (int)i.group).ThenBy(i => i.id))
            {
                if (lastGroup != item.group)
                {
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField($"📂 {item.group}", EditorStyles.helpBox);
                    lastGroup = item.group;
                }

                GUIStyle headerStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                headerStyle.normal.textColor = Color.cyan;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"🆔 {item.id}", headerStyle, GUILayout.Width(60));
                EditorGUILayout.LabelField($"🏷️ {item.name}", headerStyle);
                if (GUILayout.Button("👁", GUILayout.Width(30)))
                {
                    Selection.activeObject = item;
                    EditorGUIUtility.PingObject(item);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);
                EditorGUI.indentLevel++;

                item.rarity = (Item.Rarity)EditorGUILayout.EnumPopup("⭐ Rarity", item.rarity);
                item.group = (Item.ItemGroup)EditorGUILayout.EnumPopup("🧩 Group", item.group);
                item.price = EditorGUILayout.IntField("💰 Price", item.price);
                item.allowedClasses = (CharacterClassRestrictions)EditorGUILayout.EnumFlagsField("✅ Allowed Classes", item.allowedClasses);
                item.canStack = EditorGUILayout.Toggle("📦 Can Stack", item.canStack);

                if (item.canStack)
                    item.stackCapacity = EditorGUILayout.IntField("🔢 Stack Capacity", item.stackCapacity);

                item.isQuestSpecific = EditorGUILayout.Toggle("📜 Quest Item", item.isQuestSpecific);
                item.cannotBeDropped = EditorGUILayout.Toggle("🚫 Cannot Be Dropped", item.cannotBeDropped);
                item.cannotBeSold = EditorGUILayout.Toggle("🚫 Cannot Be Sold", item.cannotBeSold);

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                GUILayout.Space(4);
            }

            if (UnityEngine.GUI.changed)
            {
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets(); 
            }
        }
    }
}
