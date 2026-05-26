using System.Collections.Generic;
using UnityEngine;

public class SplineRuntimeState : MonoBehaviour
{
    public static SplineRuntimeState Instance { get; private set; }

    public SplineManifest manifest;

    readonly HashSet<int> _unlocked = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsBlocked(int splineIndex)
    {
        if (_unlocked.Contains(splineIndex)) return false;
        var entry = manifest?.GetEntry(splineIndex);
        return entry != null && entry.isBlockedByDefault;
    }

    public void Unblock(int splineIndex)
    {
        _unlocked.Add(splineIndex);
    }

    public int GetUnlockCost(int splineIndex)
    {
        var entry = manifest?.GetEntry(splineIndex);
        return entry != null ? entry.unlockCost : 0;
    }

    public string GetDisplayName(int splineIndex)
    {
        var entry = manifest?.GetEntry(splineIndex);
        return entry != null ? entry.displayName : $"Spline {splineIndex}";
    }

    public List<int> GetBlockedSplines()
    {
        var result = new List<int>();
        if (manifest == null) return result;
        foreach (var entry in manifest.entries)
            if (IsBlocked(entry.index)) result.Add(entry.index);
        return result;
    }

    public List<int> GetBlockedSplinesFromList(List<int> candidates)
    {
        var result = new List<int>();
        foreach (var index in candidates)
            if (IsBlocked(index)) result.Add(index);
        return result;
    }
}