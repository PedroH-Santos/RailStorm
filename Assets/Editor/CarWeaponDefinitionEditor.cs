using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(CarWeaponDefinition))]
public class CarWeaponDefinitionEditor : Editor
{
    static readonly Dictionary<ECarWeaponType, Type> _statsTypeMap = new()
    {
        { ECarWeaponType.Arrow, typeof(ArrowLevelData) },
        { ECarWeaponType.Magic, typeof(MagicLevelData) },
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var def = (CarWeaponDefinition)target;

        DrawPropertiesExcluding(serializedObject, "levels");

        EditorGUILayout.Space(6);

        _statsTypeMap.TryGetValue(def.weaponType, out Type expectedType);

        if (expectedType != null)
        {
            bool needsRebuild = false;
            foreach (var level in def.levels)
            {
                if (level == null || level.GetType() != expectedType)
                { needsRebuild = true; break; }
            }

            if (needsRebuild)
            {
                Undo.RecordObject(def, "Rebuild weapon levels");
                int count = def.levels.Count > 0 ? def.levels.Count : 5;
                var newLevels = new List<CarWeaponLevelData>(count);
                for (int i = 0; i < count; i++)
                {
                    var old = i < def.levels.Count ? def.levels[i] : null;
                    var instance = (CarWeaponLevelData)Activator.CreateInstance(expectedType);
                    if (old != null)
                    {
                        instance.damage = old.damage;
                        instance.attackRate = old.attackRate;
                        instance.range = old.range;
                    }
                    newLevels.Add(instance);
                }
                def.levels = newLevels;
                EditorUtility.SetDirty(def);
            }
        }

        var levelsProp = serializedObject.FindProperty("levels");
        EditorGUILayout.PropertyField(levelsProp, new GUIContent("Levels"), true);

        serializedObject.ApplyModifiedProperties();
    }
}