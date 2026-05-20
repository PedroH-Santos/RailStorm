using StarterAssets;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineJunction))]
public class SplineCollision : MonoBehaviour
{
    [SerializeField] private float switchCooldown = 0.3f;
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

        if (TryFindBest(inputDir, out int bestSpline, out float bestDir, out float bestKnotT))
        {
            if (bestSpline != _playerInside.CurrentSplineIndex)
            {
                _playerInside.SwitchToSplineIndex(bestSpline, bestDir, bestKnotT);
                _cooldownTimer = switchCooldown;
            }
        }
    }

    bool TryFindBest(Vector3 inputDir,
                     out int bestSplineIndex, out float bestDir, out float bestKnotT)
    {
        bestSplineIndex = -1;
        bestDir = 1f;
        bestKnotT = 0f;
        float bestDot = minDotToSwitch;

        SplineContainer container = _junction.splineContainer;

        foreach (int splineIdx in _junction.GetAvailableSplines())
        {
            if (splineIdx == _playerInside.CurrentSplineIndex) continue;

            Spline spline = container.Splines[splineIdx];

            // Acha o knot deste spline mais próximo desta junção no mundo
            float knotT = GetJunctionKnotT(spline, container);

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

            TryScore(inputDir, dirFwd, 1f, splineIdx, knotT,
                     ref bestDot, ref bestSplineIndex, ref bestDir, ref bestKnotT);
            TryScore(inputDir, dirBwd, -1f, splineIdx, knotT,
                     ref bestDot, ref bestSplineIndex, ref bestDir, ref bestKnotT);
        }

        return bestSplineIndex >= 0;
    }

    // Retorna o T normalizado do knot deste spline que está mais perto desta junção
    float GetJunctionKnotT(Spline spline, SplineContainer container)
    {
        Vector3 jPos = transform.position;
        int closestKnot = 0;
        float closestDist = float.MaxValue;

        for (int k = 0; k < spline.Count; k++)
        {
            Vector3 kw = container.transform.TransformPoint(spline[k].Position);
            float dist = Vector3.Distance(
                new Vector3(jPos.x, 0f, jPos.z),
                new Vector3(kw.x, 0f, kw.z));

            if (dist < closestDist)
            {
                closestDist = dist;
                closestKnot = k;
            }
        }

        return SplineUtility.GetNormalizedInterpolation(
                   spline, closestKnot, PathIndexUnit.Knot);
    }

    void TryScore(Vector3 inputDir, Vector3 exitDir, float dir, int splineIdx, float knotT,
                  ref float bestDot, ref int bestSpline, ref float bestDir, ref float bestKnotT)
    {
        if (exitDir.magnitude < 0.01f) return;
        float dot = Vector3.Dot(inputDir, exitDir.normalized);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestSpline = splineIdx;
            bestDir = dir;
            bestKnotT = knotT;
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