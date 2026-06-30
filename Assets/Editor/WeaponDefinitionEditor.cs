using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(WeaponDefinition))]
public class WeaponDefinitionEditor : Editor
{
    static readonly Dictionary<EWeaponType, Type> _statsTypeMap = new()
    {
        { EWeaponType.Arrow, typeof(ArrowLevelData) },
        { EWeaponType.Magic, typeof(MagicLevelData) },
    };

    bool _rebuildPending;

    //public override void OnInspectorGUI()
    //{
    //    serializedObject.Update();

    //    var def = (WeaponDefinition)target;

    //    DrawPropertiesExcluding(serializedObject, "levels");

    //    EditorGUILayout.Space(6);

    //    _statsTypeMap.TryGetValue(def.weaponType, out Type expectedType);

    //    bool needsRebuild = false;
    //    if (expectedType != null)
    //    {
    //        needsRebuild = def.levels.Count == 0;
    //        if (!needsRebuild)
    //        {
    //            foreach (var level in def.levels)
    //            {
    //                if (level == null || level.GetType() != expectedType)
    //                { needsRebuild = true; break; }
    //            }
    //        }
    //    }

    //    if (needsRebuild && !_rebuildPending)
    //    {
    //        _rebuildPending = true;
    //        EditorApplication.delayCall += () => RebuildLevels(def, expectedType);
    //    }

    //    if (needsRebuild)
    //    {
    //        EditorGUILayout.HelpBox("Atualizando níveis da arma...", MessageType.Info);
    //    }
    //    else
    //    {
    //        var levelsProp = serializedObject.FindProperty("levels");
    //        EditorGUILayout.PropertyField(levelsProp, new GUIContent("Levels"), true);
    //    }

    //    serializedObject.ApplyModifiedProperties();
    //}

    //void RebuildLevels(WeaponDefinition def, Type expectedType)
    //{
    //    _rebuildPending = false;
    //    if (def == null || expectedType == null) return;

    //    Undo.RecordObject(def, "Rebuild weapon levels");

    //    int count = def.levels.Count > 0 ? def.levels.Count : 5;
    //    var newLevels = new List<WeaponLevelData>(count);
    //    for (int i = 0; i < count; i++)
    //    {
    //        var old = i < def.levels.Count ? def.levels[i] : null;
    //        var instance = (WeaponLevelData)Activator.CreateInstance(expectedType);
    //        if (old != null)
    //        {
    //            instance.damage = old.damage;
    //            instance.attackRate = old.attackRate;
    //            instance.range = old.range;
    //        }
    //        newLevels.Add(instance);
    //    }
    //    def.levels = newLevels;

    //    EditorUtility.SetDirty(def);

    //    var so = new SerializedObject(def);
    //    so.Update();

    //    Repaint();
    //}
}