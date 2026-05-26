using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class JunctionInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;

    PlayerController _playerInside;
    PlayerStatsAggregator _stats;
    bool _menuOpen = false;
    List<int> _blockedHere = new();
    List<int> _relevantSplines = new();

    SphereCollider _interactionCollider;

    void Awake()
    {
        _interactionCollider = gameObject.AddComponent<SphereCollider>();
        _interactionCollider.radius = interactionRadius;
        _interactionCollider.isTrigger = true;
    }

    void Update()
    {
        if (_playerInside == null) return;

        bool hasBlocked = GetLocalBlockedSplines().Count > 0;

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

        int splineIndex = _blockedHere[menuIndex];
        int cost = SplineRuntimeState.Instance.GetUnlockCost(splineIndex);

        if (_stats.Coins < cost)
        {
            JunctionUIManager.Instance.ShowInsufficientFunds();
            return;
        }

        _stats.Coins -= cost;
        SplineRuntimeState.Instance.Unblock(splineIndex);
        _blockedHere.Remove(splineIndex);

        JunctionUIManager.Instance.UpdateMenu(_blockedHere, splineIndex, _stats.Coins);

        if (_blockedHere.Count == 0)
            CloseMenu();
    }

    void OpenMenu()
    {
        _menuOpen = true;
        _blockedHere = GetLocalBlockedSplines();
        _playerInside.SetMovementLocked(true);
        JunctionUIManager.Instance.ShowMenu(_blockedHere, _stats.Coins);
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _playerInside.SetMovementLocked(false);
        JunctionUIManager.Instance.HideMenu();
    }

    List<int> GetLocalBlockedSplines()
    {
        if (SplineRuntimeState.Instance == null) return new List<int>();
        return SplineRuntimeState.Instance.GetBlockedSplinesFromList(_relevantSplines);
    }

    List<int> ResolveRelevantSplines(int currentSplineIndex)
    {
        var result = new List<int>();

        SplineContainer container = FindFirstObjectByType<SplineContainer>();
        if (container == null) return result;

        KnotLinkCollection links = container.KnotLinkCollection;
        Spline currentSpline = container.Splines[currentSplineIndex];

        int closestKnot = GetClosestKnotIndex(currentSpline, container);
        var currentKnotIdx = new SplineKnotIndex(currentSplineIndex, closestKnot);

        IReadOnlyList<SplineKnotIndex> linked = links.GetKnotLinks(currentKnotIdx);

        foreach (var ski in linked)
        {
            if (ski.Spline == currentSplineIndex) continue;
            if (!result.Contains(ski.Spline))
                result.Add(ski.Spline);
        }

        return result;
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = other.GetComponent<PlayerController>();
        _stats = other.GetComponent<PlayerStatsAggregator>();
        _relevantSplines = ResolveRelevantSplines(_playerInside.CurrentSplineIndex);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_menuOpen) CloseMenu();
        _playerInside = null;
        _stats = null;
        _relevantSplines.Clear();
    }
}