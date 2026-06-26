using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(WeaponDefinition))]
public class WeaponDefinitionEditor : Editor
{
    // Mapeia cada EWeaponType para o tipo concreto de stats
    static readonly Dictionary<EWeaponType, Type> _statsTypeMap = new()
    {
        { EWeaponType.Arrow, typeof(ArrowLevelData) },
        { EWeaponType.Magic, typeof(MagicLevelData) },
        // adicione novas armas aqui
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var def = (WeaponDefinition)target;

        // Desenha todos os campos exceto 'levels'
        DrawPropertiesExcluding(serializedObject, "levels");

        EditorGUILayout.Space(6);

        // Detecta o tipo esperado para esse weaponType
        _statsTypeMap.TryGetValue(def.weaponType, out Type expectedType);

        // Se o tipo mudou, reconstrói a lista com o tipo correto
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
                var newLevels = new List<WeaponLevelData>(count);
                for (int i = 0; i < count; i++)
                {
                    // Tenta preservar os valores base da entrada anterior
                    var old = i < def.levels.Count ? def.levels[i] : null;
                    var instance = (WeaponLevelData)Activator.CreateInstance(expectedType);
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

        // Desenha a lista de levels normalmente
        var levelsProp = serializedObject.FindProperty("levels");
        EditorGUILayout.PropertyField(levelsProp, new GUIContent("Levels"), true);

        serializedObject.ApplyModifiedProperties();
    }
}