using UnityEditor;
using UnityEngine;

namespace PLAYERTWO.ARPGProject.Editors
{
    [CustomPropertyDrawer(typeof(PLAYERTWO.ARPGProject.Merchant.MerchantItem))]
    public class MerchantItemDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var dataProp = property.FindPropertyRelative("data");
            bool showLevel = false;

            if (dataProp != null && dataProp.objectReferenceValue is PLAYERTWO.ARPGProject.Item item)
            {
                showLevel = item is PLAYERTWO.ARPGProject.ItemEquippable;
            }

            // 2 pola domyślnie + 1 jeśli itemLevel ma być pokazany
            int lines = showLevel ? 3 : 2;
            return EditorGUIUtility.singleLineHeight * lines + 6f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dataProp = property.FindPropertyRelative("data");
            var attributesProp = property.FindPropertyRelative("attributes");
            var itemLevelProp = property.FindPropertyRelative("itemLevel");

            Rect rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            // Pole: Item (data)
            EditorGUI.PropertyField(rect, dataProp, new GUIContent("Item"));

            // Pole: Attributes
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, attributesProp, new GUIContent("Attributes"));

            // Pole: Item Level (tylko jeśli to Equippable)
            if (dataProp != null && dataProp.objectReferenceValue is PLAYERTWO.ARPGProject.Item item && item is PLAYERTWO.ARPGProject.ItemEquippable)
            {
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, itemLevelProp, new GUIContent("Item Level"));
            }
        }
    }
}
