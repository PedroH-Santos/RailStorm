using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineJunction : MonoBehaviour
{
    public SplineContainer splineContainer;

    [SerializeField]
    private List<SplineInfo> _splineInfos = new List<SplineInfo>();


    void Awake()
    {
        if (SplineBlockRegistry.Instance == null)
        {
            Debug.LogWarning("[SplineJunction] SplineBlockRegistry não encontrado na cena. " +
                             "Adicione um GameObject com o componente SplineBlockRegistry.");
            return;
        }

        foreach (var info in _splineInfos)
        {
            if (info.isBlocked)
                SplineBlockRegistry.Instance.Block(info.splineIndex, info.unlockCost);
        }
    }

    public bool IsBlocked(int splineIndex)
        => SplineBlockRegistry.Instance != null && SplineBlockRegistry.Instance.IsBlocked(splineIndex);

    public void Unblock(int splineIndex)
        => SplineBlockRegistry.Instance?.Unblock(splineIndex);

    public int GetUnlockCost(int splineIndex)
        => SplineBlockRegistry.Instance != null ? SplineBlockRegistry.Instance.GetUnlockCost(splineIndex) : 0;

    public List<int> GetBlockedSplines()
    {
        var result = new List<int>();
        foreach (var info in _splineInfos)
            if (IsBlocked(info.splineIndex))
                result.Add(info.splineIndex);
        return result;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}