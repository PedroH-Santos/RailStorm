using System.Collections.Generic;
using System.Linq;
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

 
    public IEnumerable<SplineEntry> GetBlockedEntries()
    {
        if (manifest == null) yield break;

        foreach (var entry in manifest.entries)
            if (IsBlocked(entry.index))
                yield return entry;
    }

    public IEnumerable<SplineEntry> GetBlockedEntriesFrom(IEnumerable<SplineEntry> candidates)
    {
        foreach (var entry in candidates)
            if (IsBlocked(entry.index))
                yield return entry;
    }


    public void Unblock(int splineIndex) => _unlocked.Add(splineIndex);
}