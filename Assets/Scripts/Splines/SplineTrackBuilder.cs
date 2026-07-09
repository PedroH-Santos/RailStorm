using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineTrackBuilder : MonoBehaviour
{
    [Header("Referência")]
    public SplineContainer splineContainer;
    public int splineIndex;

    [Header("Prefabs")]
    public GameObject plankPrefab;
    public GameObject brokenPlankPrefab;

    [Header("Containers gerados")]
    public Transform normalRoot;
    public Transform brokenRoot;

    struct SegmentInfo
    {
        public Vector3 p0, dir;
        public float length;
    }

    public void BuildNormal() => Build(plankPrefab, normalRoot);
    public void BuildBroken() => Build(brokenPlankPrefab, brokenRoot);

    void Build(GameObject prefab, Transform root)
    {
        if (splineContainer == null || prefab == null || root == null) return;

        float pieceLength = GetPieceLength(prefab);
        if (pieceLength <= 0.01f)
        {
            Debug.LogError("[SplineTrackBuilder] pieceLength precisa ser maior que zero.");
            return;
        }

        ClearChildren(root);

        Spline spline = splineContainer.Splines[splineIndex];
        int knotCount = spline.Count;
        int segmentCount = spline.Closed ? knotCount : knotCount - 1;

        // 1. Monta a lista de segmentos e soma o comprimento TOTAL do percurso
        var segments = new List<SegmentInfo>();
        float totalLength = 0f;

        for (int k = 0; k < segmentCount; k++)
        {
            int nextIndex = (k + 1) % knotCount;

            Vector3 p0 = splineContainer.transform.TransformPoint(spline[k].Position);
            Vector3 p1 = splineContainer.transform.TransformPoint(spline[nextIndex].Position);

            Vector3 segDir = p1 - p0;
            float segLength = segDir.magnitude;
            if (segLength < 0.001f) continue;
            segDir /= segLength;

            segments.Add(new SegmentInfo { p0 = p0, dir = segDir, length = segLength });
            totalLength += segLength;
        }

        if (totalLength < 0.001f || segments.Count == 0) return;

        // 2. Decide quantas peças cabem no percurso INTEIRO (não por segmento)
        int count = Mathf.Max(1, Mathf.RoundToInt(totalLength / pieceLength));
        float actualSpacing = totalLength / count; // fecha exato, sem sobra

        Debug.Log($"[SplineTrackBuilder] Total={totalLength:F2}  peças={count}  spacing={actualSpacing:F3}");

        // 3. Caminha de forma CONTÍNUA pelos segmentos, sem resetar nos knots
        int segIdx = 0;
        float segStartDist = 0f;

        for (int i = 0; i < count; i++)
        {
            float distAtCenter = (i + 0.5f) * actualSpacing;

            while (segIdx < segments.Count - 1 &&
                   distAtCenter > segStartDist + segments[segIdx].length)
            {
                segStartDist += segments[segIdx].length;
                segIdx++;
            }

            var seg = segments[segIdx];
            float localDist = distAtCenter - segStartDist;
            Vector3 worldPos = seg.p0 + seg.dir * localDist;

            Vector3 flatDir = seg.dir;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude < 0.0001f) flatDir = Vector3.forward;
            Quaternion rot = Quaternion.LookRotation(flatDir.normalized);

#if UNITY_EDITOR
            GameObject piece = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, root);
            piece.transform.SetPositionAndRotation(worldPos, rot);
            UnityEditor.Undo.RegisterCreatedObjectUndo(piece, "Build Track Piece");
#else
            Instantiate(prefab, worldPos, rot, root);
#endif
        }
    }

    void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
#else
            Destroy(root.GetChild(i).gameObject);
#endif
    }

    float GetPieceLength(GameObject prefab)
    {
        GameObject temp = (GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        temp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            DestroyImmediate(temp);
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        float length = bounds.size.z;
        DestroyImmediate(temp);
        return length;
    }
}