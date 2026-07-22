using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDefinition))]
public class ItemDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rarity"));

        var effectTypeProp = serializedObject.FindProperty("effectType");
        EditorGUILayout.PropertyField(effectTypeProp);

        var effectType = (EItemEffectType)effectTypeProp.enumValueIndex;

        EditorGUILayout.Space(6);

        if (effectType == EItemEffectType.StatChange)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statTarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isMultiplier"));
        }
        else if (effectType == EItemEffectType.Ability)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityScript"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawScriptField()
    {
        var scriptProp = serializedObject.FindProperty("m_Script");
        if (scriptProp == null) return;

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(scriptProp);
    }
}