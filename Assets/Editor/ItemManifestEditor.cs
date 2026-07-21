using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemManifest))]
public class ItemManifestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Assign IDs to New Items"))
        {
            ((ItemManifest)target).AssignIdsToNewItems();
        }
    }
}