using UnityEngine;

[System.Serializable]
public class HordeEnemyEntry
{
    public GameObject enemyPrefab;
    public int minCount = 1;
    [Range(0f, 1f)]
    public float weight = 0.5f;
}
