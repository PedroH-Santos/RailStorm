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

        JunctionUIManager.Instance?.UpdateMenu(_blockedHere, _stats.Coins);

    }


    void OpenMenu()
    {
        _menuOpen = true;
        _player.SetMovementLocked(true);
        JunctionUIManager.Instance?.ShowMenu(_blockedHere, _stats.Coins);
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _player?.SetMovementLocked(false);
        JunctionUIManager.Instance?.HideMenu();
    }


    List<SplineEntry> ResolveRelevantEntries(int currentSplineIndex)
    {
        var result = new List<SplineEntry>();
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
                result.Add(entry);
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
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, unlockRadius);
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, unlockRadius);
    }
}