using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineJunction))]
public class SplineCollision : MonoBehaviour
{
    [SerializeField] private float switchCooldown = 0.2f;
    [SerializeField] private float inputThreshold = 0.15f;
    [SerializeField] private float sampleStep = 0.05f;
    [SerializeField] private float minDotToSwitch = 0.4f;

    SplineJunction _junction;
    PlayerController _playerInside;
    float _cooldownTimer;

    void Awake()
    {
        _junction = GetComponent<SplineJunction>();
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

        if (TryFindBest(inputDir, out int bestSpline, out float bestDir,
                        out float bestKnotT, out float bestDotScore))
        {
            if (bestSpline != _playerInside.CurrentSplineIndex)
            {
                // Passa a velocidade atual para o novo spline não desacelerar
                // — e multiplica pelo dot para escalar levemente com a
                // correspondência entre input e direção da nova spline.
                float speedToCarry = _playerInside.CurrentSpeed
                                     * Mathf.Clamp01(bestDotScore);

                _playerInside.SwitchToSplineIndex(bestSpline, bestDir,
                                                  bestKnotT, speedToCarry);
                _cooldownTimer = switchCooldown;
            }
        }
    }

    bool TryFindBest(Vector3 inputDir,
                     out int bestSplineIndex,
                     out float bestDir,
                     out float bestKnotT,
                     out float bestDotOut)
    {
        bestSplineIndex = -1;
        bestDir = 1f;
        bestKnotT = 0f;
        bestDotOut = 0f;
        float bestDot = minDotToSwitch;

        SplineContainer container = _junction.splineContainer;
        KnotLinkCollection links = container.KnotLinkCollection;

        int currentIdx = _playerInside.CurrentSplineIndex;
        Spline currentSpline = container.Splines[currentIdx];
        int closestKnot = GetClosestKnotIndex(currentSpline, container);

        var currentKnotIdx = new SplineKnotIndex(currentIdx, closestKnot);
        IReadOnlyList<SplineKnotIndex> linked = links.GetKnotLinks(currentKnotIdx);

        foreach (SplineKnotIndex ski in linked)
        {
            if (ski.Spline == currentIdx) continue;   // ignora spline atual
            if (_junction.IsBlocked(ski.Spline)) continue;

            Spline spline = container.Splines[ski.Spline];

            // T exato do knot linkado nesta junção — sem GetNearestPoint
            float knotT = SplineUtility.GetNormalizedInterpolation(
                              spline, ski.Knot, PathIndexUnit.Knot);

            float tFwd = spline.Closed
                       ? Mathf.Repeat(knotT + sampleStep, 1f)
                       : Mathf.Clamp01(knotT + sampleStep);
            float tBwd = spline.Closed
                       ? Mathf.Repeat(knotT - sampleStep + 1f, 1f)
                       : Mathf.Clamp01(knotT - sampleStep);

            Vector3 origin = container.transform.TransformPoint(spline.EvaluatePosition(knotT));
            Vector3 pFwd = container.transform.TransformPoint(spline.EvaluatePosition(tFwd));
            Vector3 pBwd = container.transform.TransformPoint(spline.EvaluatePosition(tBwd));

            Vector3 dirFwd = pFwd - origin; dirFwd.y = 0f;
            Vector3 dirBwd = pBwd - origin; dirBwd.y = 0f;

            TryScore(inputDir, dirFwd, 1f, ski.Spline, knotT,
                     ref bestDot, ref bestSplineIndex, ref bestDir,
                     ref bestKnotT, ref bestDotOut);
            TryScore(inputDir, dirBwd, -1f, ski.Spline, knotT,
                     ref bestDot, ref bestSplineIndex, ref bestDir,
                     ref bestKnotT, ref bestDotOut);
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

    void TryScore(Vector3 inputDir, Vector3 exitDir, float dir,
                  int splineIdx, float knotT,
                  ref float bestDot, ref int bestSpline,
                  ref float bestDir, ref float bestKnotT, ref float bestDotOut)
    {
        if (exitDir.magnitude < 0.01f) return;
        float dot = Vector3.Dot(inputDir, exitDir.normalized);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestSpline = splineIdx;
            bestDir = dir;
            bestKnotT = knotT;
            bestDotOut = dot;
        }
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