using UnityEditor;
using UnityEngine;
using AI_DDA.Assets.Scripts;

[CustomEditor(typeof(ZoneTrigger))]
[CanEditMultipleObjects]
public class ZoneTriggerEditor : Editor
{
    SerializedProperty showInHUD;
    SerializedProperty giveReward;
    SerializedProperty zoneStatus;
    SerializedProperty zoneDescription;

    SerializedProperty achieverExpReward;
    SerializedProperty achieverGoldReward;
    SerializedProperty killerExpReward;
    SerializedProperty killerGoldReward;
    SerializedProperty socializerExpReward;
    SerializedProperty socializerGoldReward;
    SerializedProperty explorerExpReward;
    SerializedProperty explorerGoldReward;

    private void OnEnable()
    {
        showInHUD = serializedObject.FindProperty("showInHUD");
        giveReward = serializedObject.FindProperty("giveReward");
        zoneStatus = serializedObject.FindProperty("zoneStatus");
        zoneDescription = serializedObject.FindProperty("zoneDescription");

        achieverExpReward = serializedObject.FindProperty("achieverExpReward");
        achieverGoldReward = serializedObject.FindProperty("achieverGoldReward");
        killerExpReward = serializedObject.FindProperty("killerExpReward");
        killerGoldReward = serializedObject.FindProperty("killerGoldReward");
        socializerExpReward = serializedObject.FindProperty("socializerExpReward");
        socializerGoldReward = serializedObject.FindProperty("socializerGoldReward");
        explorerExpReward = serializedObject.FindProperty("explorerExpReward");
        explorerGoldReward = serializedObject.FindProperty("explorerGoldReward");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject,
            "zoneStatus", "zoneDescription",
            "achieverExpReward", "achieverGoldReward",
            "killerExpReward", "killerGoldReward",
            "socializerExpReward", "socializerGoldReward",
            "explorerExpReward", "explorerGoldReward"
        );

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(!showInHUD.boolValue);
        EditorGUILayout.PropertyField(zoneStatus);
        EditorGUILayout.PropertyField(zoneDescription);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(!giveReward.boolValue);

        EditorGUILayout.LabelField("Achiever Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(achieverExpReward);
        EditorGUILayout.PropertyField(achieverGoldReward);

        EditorGUILayout.LabelField("Killer Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(killerExpReward);
        EditorGUILayout.PropertyField(killerGoldReward);

        EditorGUILayout.LabelField("Socializer Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(socializerExpReward);
        EditorGUILayout.PropertyField(socializerGoldReward);

        EditorGUILayout.LabelField("Explorer Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(explorerExpReward);
        EditorGUILayout.PropertyField(explorerGoldReward);

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
