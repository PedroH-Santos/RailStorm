using System.Collections.Generic;
using UnityEngine;

public class SplineBlock : MonoBehaviour
{
    [SerializeField]
    private List<int> _blockedSplines = new List<int>();
    public void Block(int splineIndex) => _blockedSplines.Add(splineIndex);
    public void Unblock(int splineIndex) => _blockedSplines.Remove(splineIndex);
    public bool IsBlocked(int splineIndex) => _blockedSplines.Contains(splineIndex);
}
