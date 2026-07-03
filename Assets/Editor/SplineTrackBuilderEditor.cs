using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineTrackBuilder))]
public class SplineTrackBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var builder = (SplineTrackBuilder)target;

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Gerar Trilho Normal"))
            builder.BuildNormal();

        if (GUILayout.Button("Gerar Trilho Quebrado"))
            builder.BuildBroken();
    }
}