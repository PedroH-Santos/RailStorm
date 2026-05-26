using System.Collections.Generic;
using UnityEngine;

public class SplineBlockRegistry : MonoBehaviour
{
    public static SplineBlockRegistry Instance { get; private set; }

    readonly Dictionary<int, int> _blockedSplines = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Block(int splineIndex, int unlockCost)
    {
        _blockedSplines[splineIndex] = unlockCost;
    }

    public void Unblock(int splineIndex)
    {
        _blockedSplines.Remove(splineIndex);
    }

    public bool IsBlocked(int splineIndex)
    {
        return _blockedSplines.ContainsKey(splineIndex);
    }

    public int GetUnlockCost(int splineIndex)
    {
        return _blockedSplines.TryGetValue(splineIndex, out int cost) ? cost : 0;
    }

    public List<int> GetAllBlockedSplines()
    {
        return new List<int>(_blockedSplines.Keys);
    }
}