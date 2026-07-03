using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

[CustomEditor(typeof(SplineManifest))]
public class SplineManifestEditor : Editor
{
    SplineContainer _container;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Auto Populate", EditorStyles.boldLabel);

        _container = (SplineContainer)EditorGUILayout.ObjectField(
            "SplineContainer", _container, typeof(SplineContainer), true);

        EditorGUI.BeginDisabledGroup(_container == null);
        if (GUILayout.Button("Populate from SplineContainer"))
        {
            ((SplineManifest)target).PopulateFromContainer(_container);
        }
        EditorGUI.EndDisabledGroup();
    }
}