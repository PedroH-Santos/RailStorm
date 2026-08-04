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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("price"));

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
            DrawAbilityScriptField();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawAbilityScriptField()
    {
        var typeNameProp = serializedObject.FindProperty("abilityTypeName");

        MonoScript currentScript = null;
        if (!string.IsNullOrEmpty(typeNameProp.stringValue))
        {
            var type = System.Type.GetType(typeNameProp.stringValue);
            if (type != null)
            {
                foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
                {
                    if (script.GetClass() == type)
                    {
                        currentScript = script;
                        break;
                    }
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        var newScript = (MonoScript)EditorGUILayout.ObjectField(
            "Ability Script", currentScript, typeof(MonoScript), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (newScript == null)
            {
                typeNameProp.stringValue = "";
            }
            else
            {
                var cls = newScript.GetClass();
                if (cls != null && typeof(MonoBehaviour).IsAssignableFrom(cls))
                {
                    typeNameProp.stringValue = cls.AssemblyQualifiedName;
                }
                else
                {
                    Debug.LogWarning($"'{newScript.name}' não é um MonoBehaviour válido para Ability.");
                }
            }
        }
    }

    void DrawScriptField()
    {
        var scriptProp = serializedObject.FindProperty("m_Script");
        if (scriptProp == null) return;

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(scriptProp);
    }
}