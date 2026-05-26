using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineCollision : MonoBehaviour
{
    [SerializeField] private float switchCooldown = 0.25f;
    [SerializeField] private float inputThreshold = 0.15f;
    [SerializeField] private float sampleStep = 0.05f;
    [SerializeField] private float minDotToSwitch = 0.4f;

    [SerializeField] private SplineContainer _splineContainer;
    PlayerController _playerInside;
    float _cooldownTimer;

    void Awake()
    {
        _splineContainer = GetComponent<SplineContainer>();
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        _cooldownTimer -= Time.deltaTime;
        if (_playerInside == null) return;
        if (_cooldownTimer > 0f) return;

        Vector2 rawInput = _playerInside.RawInput;
        if (rawInput.sqrMagnitude < inputThreshold * inputThreshold) return;

        Vector3 inputDir = new Vector3(rawInput.x, 0f, rawInput.y).normalized;

        if (TryFindBest(inputDir, out int bestSpline, out float bestDir))
        {
            if (bestSpline != _playerInside.CurrentSplineIndex)
            {
                _playerInside.SwitchToSplineIndex(bestSpline, bestDir, _playerInside.CurrentSpeed);
                _cooldownTimer = switchCooldown;
            }
        }
    }

    bool TryFindBest(Vector3 inputDir, out int bestSplineIndex, out float bestDir)
    {
        bestSplineIndex = -1;
        bestDir = 1f;
        float bestDot = minDotToSwitch;

        KnotLinkCollection links = _splineContainer.KnotLinkCollection;

        int currentIdx = _playerInside.CurrentSplineIndex;
        Spline currentSpline = _splineContainer.Splines[currentIdx];

        int closestKnot = GetClosestKnotIndex(currentSpline, _splineContainer);
        var currentKnotIdx = new SplineKnotIndex(currentIdx, closestKnot);

        IReadOnlyList<SplineKnotIndex> linked = links.GetKnotLinks(currentKnotIdx);

        foreach (SplineKnotIndex ski in linked)
        {
            if (ski.Spline == currentIdx) continue;
            if (SplineRuntimeState.Instance != null && SplineRuntimeState.Instance.IsBlocked(ski.Spline)) continue;

            Spline spline = _splineContainer.Splines[ski.Spline];

            float knotT = SplineUtility.GetNormalizedInterpolation(
                              spline, ski.Knot, PathIndexUnit.Knot);

            float tFwd = spline.Closed
                       ? Mathf.Repeat(knotT + sampleStep, 1f)
                       : Mathf.Clamp01(knotT + sampleStep);
            float tBwd = spline.Closed
                       ? Mathf.Repeat(knotT - sampleStep + 1f, 1f)
                       : Mathf.Clamp01(knotT - sampleStep);

            Vector3 origin = _splineContainer.transform.TransformPoint(spline.EvaluatePosition(knotT));
            Vector3 pFwd = _splineContainer.transform.TransformPoint(spline.EvaluatePosition(tFwd));
            Vector3 pBwd = _splineContainer.transform.TransformPoint(spline.EvaluatePosition(tBwd));

            Vector3 dirFwd = pFwd - origin; dirFwd.y = 0f;
            Vector3 dirBwd = pBwd - origin; dirBwd.y = 0f;

            TryScore(inputDir, dirFwd, 1f, ski.Spline,
                     ref bestDot, ref bestSplineIndex, ref bestDir);
            TryScore(inputDir, dirBwd, -1f, ski.Spline,
                     ref bestDot, ref bestSplineIndex, ref bestDir);
        }

        return bestSplineIndex >= 0;
    }

    int GetClosestKnotIndex(Spline spline, SplineContainer container)
    {
        Vector3 jPos = transform.position;
        int closest = 0;
        float closestDist = float.MaxValue;

        for (int k = 0; k < spline.Count; k++)
        {
            Vector3 kw = container.transform.TransformPoint(spline[k].Position);
            float dist = Vector3.Distance(
                new Vector3(jPos.x, 0f, jPos.z),
                new Vector3(kw.x, 0f, kw.z));
            if (dist < closestDist) { closestDist = dist; closest = k; }
        }
        return closest;
    }

    void TryScore(Vector3 inputDir, Vector3 exitDir, float dir, int splineIdx,
                  ref float bestDot, ref int bestSpline, ref float bestDir)
    {
        if (exitDir.magnitude < 0.01f) return;
        float dot = Vector3.Dot(inputDir, exitDir.normalized);
        if (dot > bestDot) { bestDot = dot; bestSpline = splineIdx; bestDir = dir; }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = other.GetComponent<PlayerController>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = null;
    }
}