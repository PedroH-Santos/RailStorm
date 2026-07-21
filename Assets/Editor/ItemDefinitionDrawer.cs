using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemDefinition))]
public class ItemDefinitionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var idProp = property.FindPropertyRelative("id");
        var nameProp = property.FindPropertyRelative("itemName");
        var descProp = property.FindPropertyRelative("description");
        var iconProp = property.FindPropertyRelative("icon");
        var rarityProp = property.FindPropertyRelative("rarity");
        var effectTypeProp = property.FindPropertyRelative("effectType");
        var statTargetProp = property.FindPropertyRelative("statTarget");
        var statValueProp = property.FindPropertyRelative("statValue");
        var isMultiplierProp = property.FindPropertyRelative("isMultiplier");
        var abilityScriptProp = property.FindPropertyRelative("abilityScript");

        float lh = EditorGUIUtility.singleLineHeight;
        float sp = EditorGUIUtility.standardVerticalSpacing;
        float y = position.y;

        Rect NextLine()
        {
            var r = new Rect(position.x, y, position.width, lh);
            y += lh + sp;
            return r;
        }

        EditorGUI.PropertyField(NextLine(), idProp);
        EditorGUI.PropertyField(NextLine(), nameProp);
        EditorGUI.PropertyField(NextLine(), descProp);
        EditorGUI.PropertyField(NextLine(), iconProp);
        EditorGUI.PropertyField(NextLine(), rarityProp);
        EditorGUI.PropertyField(NextLine(), effectTypeProp);

        var effectType = (EItemEffectType)effectTypeProp.enumValueIndex;

        if (effectType == EItemEffectType.StatChange)
        {
            EditorGUI.PropertyField(NextLine(), statTargetProp);
            EditorGUI.PropertyField(NextLine(), statValueProp);
            EditorGUI.PropertyField(NextLine(), isMultiplierProp);
        }
        else if (effectType == EItemEffectType.Ability)
        {
            EditorGUI.PropertyField(NextLine(), abilityScriptProp);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lh = EditorGUIUtility.singleLineHeight;
        float sp = EditorGUIUtility.standardVerticalSpacing;

        int lines = 6; // id, name, desc, icon, rarity, effectType

        var effectTypeProp = property.FindPropertyRelative("effectType");
        var effectType = (EItemEffectType)effectTypeProp.enumValueIndex;

        lines += effectType == EItemEffectType.StatChange ? 3 : 1;

        return lines * (lh + sp);
    }
}