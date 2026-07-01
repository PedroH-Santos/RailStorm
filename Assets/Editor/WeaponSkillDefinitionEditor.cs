using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponSkillDefinition))]
public class WeaponSkillDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        var weaponTypeProp = serializedObject.FindProperty("weaponType");
        EditorGUILayout.PropertyField(weaponTypeProp);

        DrawStatTargetPopup(weaponTypeProp);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentRarity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levels"), true);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawScriptField()
    {
        var scriptProp = serializedObject.FindProperty("m_Script");
        if (scriptProp == null) return;

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(scriptProp);
    }

    void DrawStatTargetPopup(SerializedProperty weaponTypeProp)
    {
        var statTargetProp = serializedObject.FindProperty("statTarget");

        var weaponType = (EWeaponType)weaponTypeProp.enumValueIndex;
        var allowed = WeaponStatTargetMap.GetAllowed(weaponType);

        if (allowed.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Selecione um Weapon Type válido para ver os atributos disponíveis.",
                MessageType.Info);
            return;
        }

        var currentValue = (EWeaponStatTarget)statTargetProp.enumValueIndex;
        int currentIndex = System.Array.IndexOf(allowed, currentValue);
        if (currentIndex < 0) currentIndex = 0;

        string[] options = allowed.Select(a => a.ToString()).ToArray();
        int newIndex = EditorGUILayout.Popup("Stat Target", currentIndex, options);

        statTargetProp.enumValueIndex = (int)allowed[newIndex];
    }
}