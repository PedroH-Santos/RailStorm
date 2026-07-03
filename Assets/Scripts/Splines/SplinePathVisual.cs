using UnityEngine;

public class SplinePathVisual : MonoBehaviour
{
    [SerializeField] private int splineIndex;
    [SerializeField] private GameObject normalRoot;
    [SerializeField] private GameObject brokenRoot;

    void OnEnable() => SplineRuntimeState.OnSplineUnblocked += HandleUnblocked;
    void OnDisable() => SplineRuntimeState.OnSplineUnblocked -= HandleUnblocked;

    void Start() => Refresh();

    void HandleUnblocked(int index) { if (index == splineIndex) Refresh(); }

    void Refresh()
    {
        bool blocked = SplineRuntimeState.Instance.IsBlocked(splineIndex);
        normalRoot.SetActive(!blocked);
        brokenRoot.SetActive(blocked);
    }
}