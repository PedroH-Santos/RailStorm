using UnityEngine;
using System.Collections.Generic;

public enum StatTarget
{
    MoveSpeed,
    MaxHP,
    HP,
    LuckPercent,
    Coins,
    FireballDamage,
    FireballSpeed,
    FireballRange,
    AttackRate,

    CarWeaponDamage,
    CarFireRate,
    CarRange,
    CarMaxWeapons,
    CarSpeed,

    EnemyDamage,
    EnemySpeed,
    EnemyHP,
    EnemyAttackRate,

    SpawnRate,
    WaveSize,
    CoinDropRate,
    XpMultiplier,
}

public enum SkillRarity
{
    Common,
    Uncommon,
    Rare
}

[System.Serializable]
public class SkillLevel
{
    [TextArea] public string description;
    public float statValue;
    public bool isMultiplier;
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject, IDrawable
{
    [Header("Informações")]
    public string skillName = "Nova Skill";
    public Sprite icon;

    [Header("Raridade")]
    public SkillRarity rarity;

    [Header("Atributo afetado")]
    public StatTarget statTarget;

    [Header("Níveis")]
    public List<SkillLevel> levels = new();

    public string DisplayName => skillName;
    public Sprite Icon => icon;
    public SkillRarity Rarity => rarity;

    public int MaxLevel => levels.Count;

    public SkillLevel GetLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }

    static readonly float WeightCommon = 70f;
    static readonly float WeightUncommon = 20f;
    static readonly float WeightRare = 10f;

    public float GetWeight(float luckPercent)
    {
        float common = Mathf.Max(0f, WeightCommon - luckPercent * 0.6f);
        float uncommon = WeightUncommon + luckPercent * 0.3f;
        float rare = WeightRare + luckPercent * 0.3f;

        return rarity switch
        {
            SkillRarity.Common => common,
            SkillRarity.Uncommon => uncommon,
            SkillRarity.Rare => rare,
            _ => 0f
        };
    }
}