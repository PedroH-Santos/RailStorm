using UnityEngine;

[CreateAssetMenu(fileName = "NewBurn", menuName = "Combat/Burn Definition")]
public class BurnDefinition : ScriptableObject
{
    [Header("Acúmulos")]
    [Min(1)] public int stacksToIgnite = 3;
    [Min(1)] public int bossStacksToIgnite = 5;
    [Tooltip("Segundos sem ser atingido até os acúmulos sumirem.")]
    [Min(0f)] public float stackDecaySeconds = 4f;

    [Header("Queimadura")]
    [Min(0.1f)] public float burnDuration = 3f;
    [Min(0f)] public float damagePerSecond = 5f;
    [Min(0.1f)] public float tickInterval = 1f;

    [Header("Visual")]
    public GameObject flamePrefab;
    public Vector3 flameOffset = new Vector3(0f, 2f, 0f);

    public int StacksToIgnite(bool isBoss) => isBoss ? bossStacksToIgnite : stacksToIgnite;

    public int DamagePerTick => Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * tickInterval));
}
