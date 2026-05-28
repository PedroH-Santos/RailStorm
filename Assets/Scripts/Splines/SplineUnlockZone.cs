using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[RequireComponent(typeof(SphereCollider))]
public class SplineUnlockZone : MonoBehaviour
{
    [Header("Collider desta zona (independente do SplineCollision)")]
    [SerializeField] private float unlockRadius = 3f;

    [Header("Referência ao SplineContainer da cena")]
    [SerializeField] private SplineContainer splineContainer;

    PlayerController _player;
    PlayerStatsAggregator _stats;
    bool _menuOpen;

    List<SplineEntry> _relevantEntries = new();
    List<SplineEntry> _blockedHere = new();

    readonly Dictionary<int, string> _entryDirections = new();

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = unlockRadius;
        col.isTrigger = true;
    }

    void Update()
    {
        if (_player == null) return;

        _blockedHere = SplineRuntimeState.Instance
            .GetBlockedEntriesFrom(_relevantEntries)
            .ToList();

        bool hasBlocked = _blockedHere.Count > 0;

        if (hasBlocked && !_menuOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenMenu();
            return;
        }

        if (_menuOpen && (Keyboard.current.eKey.wasPressedThisFrame
                       || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseMenu();
            return;
        }

        if (_menuOpen)
            HandleUnlockInput();
    }

    void HandleUnlockInput()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryUnlock(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) TryUnlock(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) TryUnlock(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) TryUnlock(3);
    }

    void TryUnlock(int menuIndex)
    {
        if (menuIndex >= _blockedHere.Count) return;

        SplineEntry entry = _blockedHere[menuIndex];

        if (_stats.Coins < entry.unlockCost)
        {
            JunctionUIManager.Instance?.ShowInsufficientFunds();
            return;
        }

        _stats.Coins -= entry.unlockCost;
        SplineRuntimeState.Instance.Unblock(entry.index);

        _blockedHere = SplineRuntimeState.Instance
            .GetBlockedEntriesFrom(_relevantEntries)
            .ToList();

        if (_blockedHere.Count == 0)
            CloseMenu();
        else
            JunctionUIManager.Instance?.UpdateMenu(BuildMenuEntries(), _stats.Coins);
    }

    void OpenMenu()
    {
        _menuOpen = true;
        _player.SetMovementLocked(true);
        Time.timeScale = 0f;
        JunctionUIManager.Instance?.ShowMenu(BuildMenuEntries(), _stats.Coins);
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _player?.SetMovementLocked(false);
        Time.timeScale = 1f;
        JunctionUIManager.Instance?.HideMenu();
    }

    List<JunctionMenuEntry> BuildMenuEntries()
    {
        var result = new List<JunctionMenuEntry>();
        foreach (var entry in _blockedHere)
        {
            string dir = _entryDirections.TryGetValue(entry.index, out var d) ? d : "?";
            string dest = !string.IsNullOrEmpty(entry.destinationName)
                          ? entry.destinationName
                          : entry.displayName;

            result.Add(new JunctionMenuEntry
            {
                DirectionArrow = dir,
                DestinationName = dest,
                UnlockCost = entry.unlockCost
            });
        }
        return result;
    }

    string GetDirectionArrow(SplineEntry entry)
    {
        if (splineContainer == null) return "→";

        Spline spline = splineContainer.Splines[entry.index];
        if (spline == null || spline.Count == 0) return "→";

        int closestKnot = GetClosestKnotIndex(spline);
        float knotT = SplineUtility.GetNormalizedInterpolation(spline, closestKnot, PathIndexUnit.Knot);

        Vector3 junctionPos = transform.position;
        Vector3 origin = splineContainer.transform.TransformPoint(spline.EvaluatePosition(knotT));

        float sampleStep = 0.08f;

        float tFwd = spline.Closed
            ? Mathf.Repeat(knotT + sampleStep, 1f)
            : Mathf.Clamp01(knotT + sampleStep);
        float tBwd = spline.Closed
            ? Mathf.Repeat(knotT - sampleStep + 1f, 1f)
            : Mathf.Clamp01(knotT - sampleStep);

        Vector3 pFwd = splineContainer.transform.TransformPoint(spline.EvaluatePosition(tFwd));
        Vector3 pBwd = splineContainer.transform.TransformPoint(spline.EvaluatePosition(tBwd));

        float distFwd = Vector3.Distance(new Vector3(pFwd.x, 0f, pFwd.z),
                                         new Vector3(junctionPos.x, 0f, junctionPos.z));
        float distBwd = Vector3.Distance(new Vector3(pBwd.x, 0f, pBwd.z),
                                         new Vector3(junctionPos.x, 0f, junctionPos.z));

        Vector3 chosenPoint = distFwd >= distBwd ? pFwd : pBwd;
        Vector3 dir = chosenPoint - origin;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return "→";

        dir.Normalize();

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        int sector = Mathf.RoundToInt(angle / 45f) % 8;
        return sector switch
        {
            0 => "↑",
            1 => "↗",
            2 => "→",
            3 => "↘",
            4 => "↓",
            5 => "↙",
            6 => "←",
            7 => "↖",
            _ => "→"
        };
    }

    List<SplineEntry> ResolveRelevantEntries(int currentSplineIndex)
    {
        var result = new List<SplineEntry>();
        _entryDirections.Clear();

        if (splineContainer == null || SplineRuntimeState.Instance?.manifest == null)
            return result;

        KnotLinkCollection links = splineContainer.KnotLinkCollection;
        if (links == null) return result;

        Spline currentSpline = splineContainer.Splines[currentSplineIndex];
        int closestKnot = GetClosestKnotIndex(currentSpline);
        var currentKnotIdx = new SplineKnotIndex(currentSplineIndex, closestKnot);

        IReadOnlyList<SplineKnotIndex> linked = links.GetKnotLinks(currentKnotIdx);
        if (linked == null) return result;

        foreach (var ski in linked)
        {
            if (ski.Spline == currentSplineIndex) continue;

            SplineEntry entry = SplineRuntimeState.Instance.manifest.GetEntry(ski.Spline);
            if (entry != null && !result.Contains(entry))
            {
                result.Add(entry);
                _entryDirections[entry.index] = GetDirectionArrow(entry);
            }
        }

        return result;
    }

    int GetClosestKnotIndex(Spline spline)
    {
        Vector3 jPos = transform.position;
        int closest = 0;
        float closestDist = float.MaxValue;

        for (int k = 0; k < spline.Count; k++)
        {
            Vector3 kw = splineContainer.transform.TransformPoint(spline[k].Position);
            float dist = Vector3.Distance(
                new Vector3(jPos.x, 0f, jPos.z),
                new Vector3(kw.x, 0f, kw.z));
            if (dist < closestDist) { closestDist = dist; closest = k; }
        }
        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _player = other.GetComponent<PlayerController>();
        _stats = other.GetComponent<PlayerStatsAggregator>();

        if (_player != null)
            _relevantEntries = ResolveRelevantEntries(_player.CurrentSplineIndex);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_menuOpen) CloseMenu();

        _player = null;
        _stats = null;
        _relevantEntries.Clear();
        _blockedHere.Clear();
        _entryDirections.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, unlockRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, unlockRadius);
    }
}