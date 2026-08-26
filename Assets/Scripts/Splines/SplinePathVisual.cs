using System.Collections.Generic;
using UnityEngine;

public class SplinePathVisual : MonoBehaviour
{
    static readonly Dictionary<int, SplinePathVisual> VisualsBySplineIndex = new();

    [SerializeField] private int splineIndex;
    [SerializeField] private GameObject normalRoot;
    [SerializeField] private GameObject brokenRoot;

    public GameObject ActiveRoot => brokenRoot != null && brokenRoot.activeSelf ? brokenRoot : normalRoot;

    public static bool TryGet(int forSplineIndex, out SplinePathVisual visual) =>
        VisualsBySplineIndex.TryGetValue(forSplineIndex, out visual);

    void OnEnable()
    {
        SplineRuntimeState.OnSplineUnblocked += HandleUnblocked;
        VisualsBySplineIndex[splineIndex] = this;
    }

    void OnDisable()
    {
        SplineRuntimeState.OnSplineUnblocked -= HandleUnblocked;
        if (VisualsBySplineIndex.TryGetValue(splineIndex, out var current) && current == this)
            VisualsBySplineIndex.Remove(splineIndex);
    }

    void Start() => Refresh();

    void HandleUnblocked(int index) { if (index == splineIndex) Refresh(); }

    void Refresh()
    {
        bool blocked = SplineRuntimeState.Instance.IsBlocked(splineIndex);
        normalRoot.SetActive(!blocked);
        brokenRoot.SetActive(blocked);
    }
}