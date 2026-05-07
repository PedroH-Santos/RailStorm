using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineJunction : MonoBehaviour
{
    public SplineContainer splineContainer;

    [SerializeField]
    private List<SplineInfo> _splineInfos = new List<SplineInfo>();

    SplineInfo GetInfo(int splineIndex)
    {
        foreach (var info in _splineInfos)
            if (info.splineIndex == splineIndex) return info;
        return null;
    }

    public bool IsBlocked(int splineIndex)
    {
        var info = GetInfo(splineIndex);
        return info != null && info.isBlocked;
    }

    public void Unblock(int splineIndex)
    {
        var info = GetInfo(splineIndex);
        if (info != null) info.isBlocked = false;
    }

    public int GetUnlockCost(int splineIndex)
    {
        var info = GetInfo(splineIndex);
        return info != null ? info.unlockCost : 0;
    }

    public List<int> GetBlockedSplines()
    {
        var result = new List<int>();
        foreach (var info in _splineInfos)
            if (info.isBlocked) result.Add(info.splineIndex);
        return result;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}