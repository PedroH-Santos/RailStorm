using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineJunction))]
public class SplineCollision : MonoBehaviour
{
    [SerializeField] private float switchCooldown = 0.3f;
    [SerializeField] private float inputThreshold = 0.09f;
    [SerializeField] private float sampleStep = 0.05f;

    SplineJunction _junction;
    PlayerController _playerInside;
    float _cooldownTimer;

    SplineKnotIndex[] _linkedKnots;


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
        if (_linkedKnots == null || _linkedKnots.Length == 0) return;

        Vector2 rawInput = _playerInside.RawInput;
        if (rawInput.sqrMagnitude < inputThreshold) return;

        Vector3 inputDir = new Vector3(rawInput.x, 0f, rawInput.y).normalized;

        if (TryFindBest(inputDir, out int bestSpline, out float bestDir))
        {

            bool differentSpline = bestSpline != _playerInside.CurrentSplineIndex;
            bool differentDir = !Mathf.Approximately(bestDir, _playerInside.LastDirection);

            if (differentSpline || differentDir)
            {
                //Debug.Log($"[Junction] → spline={bestSpline} dir={bestDir}");
                _playerInside.SwitchToSplineIndex(bestSpline, bestDir);
                _cooldownTimer = switchCooldown;
            }
        }
    }

    // ---------------------------------------------------------------
    // Core: iterate every linked knot at this junction, score each
    // exit direction against the player's input, return the winner.
    // ---------------------------------------------------------------
    bool TryFindBest(Vector3 inputDir,
                     out int bestSplineIndex, out float bestLastDir)
    {
        bestSplineIndex = -1;
        bestLastDir = 1f;
        float bestDot = 0.25f;          // minimum threshold to qualify

        SplineContainer container = _junction.splineContainer;

        foreach (SplineKnotIndex ski in _linkedKnots)
        {
           // Debug.Log($"Checking knot {ski} on spline {ski.Spline}");
            //Debug.Log($"BLOCKED: {_junction.IsBlocked(ski.Spline)}");

            if (_junction.IsBlocked(ski.Spline)) continue;

            //Debug.Log("PASSOU");

            Spline spline = container.Splines[ski.Spline];

            float knotT = SplineUtility.GetNormalizedInterpolation(
                              spline, ski.Knot, PathIndexUnit.Knot);

            // Sample a tiny step forward and backward from this knot
            float tFwd = spline.Closed
                       ? Mathf.Repeat(knotT + sampleStep, 1f)
                       : Mathf.Clamp01(knotT + sampleStep);
            float tBwd = spline.Closed
                       ? Mathf.Repeat(knotT - sampleStep + 1f, 1f)
                       : Mathf.Clamp01(knotT - sampleStep);

            Vector3 origin = container.transform.TransformPoint(
                                 spline.EvaluatePosition(knotT));
            Vector3 pFwd = container.transform.TransformPoint(
                                 spline.EvaluatePosition(tFwd));
            Vector3 pBwd = container.transform.TransformPoint(
                                 spline.EvaluatePosition(tBwd));

            Vector3 dirFwd = pFwd - origin; dirFwd.y = 0f;
            Vector3 dirBwd = pBwd - origin; dirBwd.y = 0f;

            TryScore(inputDir, dirFwd, 1f, ski.Spline,
                     ref bestDot, ref bestSplineIndex, ref bestLastDir);
            TryScore(inputDir, dirBwd, -1f, ski.Spline,
                     ref bestDot, ref bestSplineIndex, ref bestLastDir);
        }

        return bestSplineIndex >= 0;
    }

    void TryScore(Vector3 inputDir, Vector3 exitDir, float dir, int splineIdx,
                  ref float bestDot, ref int bestSpline, ref float bestDir)
    {
        if (exitDir.magnitude < 0.01f) return;
        float dot = Vector3.Dot(inputDir, exitDir.normalized);
        //Debug.Log($"  spline={splineIdx} dir={dir} exitDir={exitDir.normalized} dot={dot:F2}");
        if (dot > bestDot)
        {
            bestDot = dot;
            bestSpline = splineIdx;
            bestDir = dir;
        }
    }

    // ---------------------------------------------------------------
    // On enter: resolve which linked knots belong to this junction
    // using KnotLinkCollection — no distance search needed.
    // ---------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = other.GetComponent<PlayerController>();
        _linkedKnots = ResolveLinkedKnots();

       // Debug.Log($"[Junction] Player entered. Found {_linkedKnots.Length} linked knots.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = null;
        _linkedKnots = null;
        //Debug.Log("[Junction] Player exited");
    }

    // Walk every spline/knot in the container; find the one closest to
    // THIS junction's world position, then pull its full link group.
    SplineKnotIndex[] ResolveLinkedKnots()
    {
        SplineContainer container = _junction.splineContainer;
        var links = container.KnotLinkCollection;
        Vector3 junctionPos = transform.position;

        SplineKnotIndex closestKnot = default;
        float closestDist = float.MaxValue;
        bool found = false;

        for (int s = 0; s < container.Splines.Count; s++)
        {
            Spline spline = container.Splines[s];
            for (int k = 0; k < spline.Count; k++)
            {
                Vector3 knotWorld = container.transform.TransformPoint(
                                        spline[k].Position);
                float dist = Vector3.Distance(
                    new Vector3(junctionPos.x, 0f, junctionPos.z),
                    new Vector3(knotWorld.x, 0f, knotWorld.z));

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestKnot = new SplineKnotIndex(s, k);
                    found = true;
                }
            }
        }

        if (!found) return System.Array.Empty<SplineKnotIndex>();

        // Get the full group of knots linked at this junction
        if (links.TryGetKnotLinks(closestKnot, out var group))
        {
            // Convert IReadOnlyList to array
            var result = new SplineKnotIndex[group.Count];
            for (int i = 0; i < group.Count; i++) result[i] = group[i];
            return result;
        }

        // Fallback: junction knot exists but has no links — return just itself
        return new[] { closestKnot };
    }
}