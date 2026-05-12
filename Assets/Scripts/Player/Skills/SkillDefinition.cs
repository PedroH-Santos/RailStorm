using UnityEngine;
using System.Collections.Generic;

public enum SkillType
{
    Stat,
    Mechanic
}

public enum StatTarget
{
    MoveSpeed,
    Coins,
}

public enum MechanicType
{
    Dash,
    DoubleJump,
    Shield,
}

[System.Serializable]
public class SkillLevel
{
    [TextArea] public string description;
    public float statValue;        
    public bool isMultiplier;      
}

public enum SkillRarity
{
    Common,
    Uncommon,
    Rare
}


[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Info")]
    public string skillName = "Nova Skill";
    public Sprite icon;

    [Header("Rarity")]
    public SkillRarity rarity;

    [Header("Type")]
    public SkillType skillType;

    [Header("Stat (se SkillType = Stat)")]
    public StatTarget statTarget;

    [Header("Mechanic (se SkillType = Mechanic)")]
    public MechanicType mechanicType;

    [Header("Levels")]
    public List<SkillLevel> levels = new(); 

    public int MaxLevel => levels.Count;

    [Header("Probabilidades Base (%)")]
    private static float WeightCommon = 70f;
    private static float WeightUncommon = 20f;
    private static float WeightRare = 10f;

    public float BaseWeight => rarity switch
    {
        SkillRarity.Common => WeightCommon,
        SkillRarity.Uncommon => WeightUncommon,
        SkillRarity.Rare => WeightRare,
        _ => 0f
    };

    public float GetWeight(float luckPercent)
    {
        float luck = luckPercent;

        float common = Mathf.Max(0f, WeightCommon - luck * 0.6f);
        float uncommon = WeightUncommon + luck * 0.3f;
        float rare = WeightRare + luck * 0.3f;

        return rarity switch
        {
            SkillRarity.Common => common,
            SkillRarity.Uncommon => uncommon,
            SkillRarity.Rare => rare,
            _ => 0f
        };
    }

    public SkillLevel GetLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }
}