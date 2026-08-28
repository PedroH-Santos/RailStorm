using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathVisual : MonoBehaviour
{
    static readonly Dictionary<int, SplinePathVisual> VisualsBySplineIndex = new();

    [SerializeField] private int splineIndex;
    [SerializeField] private GameObject normalRoot;
    [SerializeField] private GameObject brokenRoot;

    [Header("Onda de construcao")]
    [SerializeField] private float plankPopDuration = 0.18f;
    [SerializeField] private float plankRiseHeight = 0.6f;

    public GameObject ActiveRoot => brokenRoot != null && brokenRoot.activeSelf ? brokenRoot : normalRoot;

    Coroutine _revealRoutine;

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
        if (_revealRoutine != null) return;

        bool blocked = SplineRuntimeState.Instance.IsBlocked(splineIndex);
        normalRoot.SetActive(!blocked);
        brokenRoot.SetActive(blocked);
    }

    public void PlayUnlockReveal(bool reversed, float duration)
    {
        if (normalRoot == null) return;

        if (_revealRoutine != null) StopCoroutine(_revealRoutine);
        _revealRoutine = StartCoroutine(RevealRoutine(reversed, duration));
    }

    IEnumerator RevealRoutine(bool reversed, float duration)
    {
        if (brokenRoot != null) brokenRoot.SetActive(false);
        normalRoot.SetActive(true);

        Transform root = normalRoot.transform;
        int count = root.childCount;
        if (count == 0)
        {
            _revealRoutine = null;
            yield break;
        }

        Vector3 sink = root.InverseTransformVector(Vector3.down * plankRiseHeight);

        var planks = new Transform[count];
        var restScales = new Vector3[count];
        var restPositions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            int childIndex = reversed ? count - 1 - i : i;
            Transform plank = root.GetChild(childIndex);

            planks[i] = plank;
            restScales[i] = plank.localScale;
            restPositions[i] = plank.localPosition;

            plank.localScale = Vector3.zero;
            plank.localPosition = restPositions[i] + sink;
        }

        float stagger = count > 1 ? duration / (count - 1) : 0f;
        float elapsed = 0f;
        float total = duration + plankPopDuration;

        while (elapsed < total)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < count; i++)
            {
                float local = Mathf.Clamp01((elapsed - i * stagger) / plankPopDuration);
                float eased = Easing.BackOut(local);

                planks[i].localScale = restScales[i] * eased;
                planks[i].localPosition = Vector3.Lerp(
                    restPositions[i] + sink,
                    restPositions[i],
                    Easing.CubicOut(local));
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            planks[i].localScale = restScales[i];
            planks[i].localPosition = restPositions[i];
        }

        _revealRoutine = null;
    }
}
