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

    public void BuildNormal() => Build(plankPrefab, normalRoot);
    public void BuildBroken() => Build(brokenPlankPrefab, brokenRoot);

    void Build(GameObject prefab, Transform root)
    {
        if (splineContainer == null || prefab == null || root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
#else
            Destroy(root.GetChild(i).gameObject);
#endif

        float pieceLength = GetPieceLength(prefab);
        if (pieceLength <= 0.01f)
        {
            Debug.LogError("[SplineTrackBuilder] Não consegui medir o tamanho da peça.");
            return;
        }

        Spline spline = splineContainer.Splines[splineIndex];
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        int count = Mathf.Max(1, Mathf.RoundToInt(totalLength / pieceLength));

        for (int i = 0; i < count; i++)
        {
            // posiciona no MEIO de cada segmento, não na borda,
            // pra peça ficar centralizada no trecho que ela cobre
            float distAtCenter = (i + 0.5f) * pieceLength;
            float t = distAtCenter / totalLength;
            t = Mathf.Clamp01(t);

            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

            Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t));
            tangent.y = 0f;
            if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;

            Quaternion rot = Quaternion.LookRotation(tangent.normalized);

#if UNITY_EDITOR
            GameObject piece = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, root);
            piece.transform.SetPositionAndRotation(worldPos, rot);
            UnityEditor.Undo.RegisterCreatedObjectUndo(piece, "Build Track Piece");
#else
            Instantiate(prefab, worldPos, rot, root);
#endif
        }
    }

    float GetPieceLength(GameObject prefab)
    {
        var renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer == null) return 0f;

        // assume que o "comprimento" da peça é o eixo Z do bounds local
        return renderer.bounds.size.z > 0f ? renderer.bounds.size.z : prefab.transform.localScale.z;
    }
}