using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SplineJunctionSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private float detectionRadius = 1f;

    [ContextMenu("Auto Find Connected Splines")]
    public void AutoFindConnectedSplines()
    {
        SplineJunction junction = GetComponent<SplineJunction>();
        if (junction == null)
        {
            Debug.LogError("SplineJunction component not found!");
            return;
        }

        SplineContainer[] allSplines = FindObjectsOfType<SplineContainer>();
        System.Collections.Generic.List<SplineContainer> connected = new System.Collections.Generic.List<SplineContainer>();

        foreach (SplineContainer spline in allSplines)
        {
            // Verifica se algum knot está próximo desta junção
            for (int i = 0; i < spline.Spline.Count; i++)
            {
                Vector3 knotPosition = spline.transform.TransformPoint(spline.Spline[i].Position);
                float distance = Vector3.Distance(transform.position, knotPosition);

                if (distance < detectionRadius)
                {
                    connected.Add(spline);
                    break;
                }
            }
        }

        Debug.Log($"Found {connected.Count} connected splines");

#if UNITY_EDITOR
        SerializedObject so = new SerializedObject(junction);
        SerializedProperty prop = so.FindProperty("connectedSplines");
        prop.arraySize = connected.Count;
        for (int i = 0; i < connected.Count; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = connected[i];
        }
        so.ApplyModifiedProperties();
#endif
    }
}