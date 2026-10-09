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

    [Header("Chama sobre o inimigo")]
    public LowPolyFlame flamePrefab;
    public Vector3 flameOffset = new Vector3(0f, 2f, 0f);

    [Header("Indicador de acúmulos")]
    public BurnIndicator indicatorPrefab;
    [Tooltip("Altura do indicador acima dos pés do inimigo, em unidades do mundo.")]
    public float indicatorHeight = 1.7f;
    [Tooltip("Deslocamento para a direita da tela, em unidades do mundo.")]
    public float indicatorSide = 0.7f;

    [Header("Corpo avermelhado")]
    [Tooltip("Brilho somado ao inimigo enquanto queima. Manter abaixo de 1 para não florescer no Bloom.")]
    public Color burnTint = new Color(0.5f, 0.08f, 0.02f, 1f);
    [Tooltip("Cor que multiplica o corpo do inimigo enquanto queima.")]
    public Color burnBodyTint = new Color(1f, 0.4f, 0.3f, 1f);
    [Min(0f)] public float tintPulseSpeed = 7f;

    public int StacksToIgnite(bool isBoss) => isBoss ? bossStacksToIgnite : stacksToIgnite;

    public int DamagePerTick => Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * tickInterval));
}
