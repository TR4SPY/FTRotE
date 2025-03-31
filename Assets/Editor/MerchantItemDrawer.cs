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
            bool showQuantity = false;

            if (dataProp != null && dataProp.objectReferenceValue is PLAYERTWO.ARPGProject.Item item)
            {
                if (item is PLAYERTWO.ARPGProject.ItemEquippable)
                    showLevel = true;

                if (item.canStack)
                    showQuantity = true;
            }

            int lines = 2;
            if (showLevel) lines++;
            if (showQuantity) lines++;

            return EditorGUIUtility.singleLineHeight * lines + 6f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dataProp       = property.FindPropertyRelative("data");
            var attributesProp = property.FindPropertyRelative("attributes");
            var itemLevelProp  = property.FindPropertyRelative("itemLevel");
            var quantityProp   = property.FindPropertyRelative("quantity");

            Rect rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(rect, dataProp, new GUIContent("Item"));
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.PropertyField(rect, attributesProp, new GUIContent("Attributes"));
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            PLAYERTWO.ARPGProject.Item soItem = null;
            if (dataProp != null && dataProp.objectReferenceValue is PLAYERTWO.ARPGProject.Item castItem)
            {
                soItem = castItem;
            }

            if (soItem != null && soItem is PLAYERTWO.ARPGProject.ItemEquippable)
            {
                EditorGUI.PropertyField(rect, itemLevelProp, new GUIContent("Item Level"));
                rect.y += EditorGUIUtility.singleLineHeight + 2;
            }

            if (soItem != null && soItem.canStack)
            {
                EditorGUI.PropertyField(rect, quantityProp, new GUIContent("Quantity"));
                rect.y += EditorGUIUtility.singleLineHeight + 2;
            }
        }
    }
}
