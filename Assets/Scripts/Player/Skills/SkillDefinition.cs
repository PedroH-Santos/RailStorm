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

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Info")]
    public string skillName = "Nova Skill";
    public Sprite icon;

    [Header("Rarity")]
    [Range(0f, 1f)] public float weight = 0.5f;

    [Header("Type")]
    public SkillType skillType;

    [Header("Stat (se SkillType = Stat)")]
    public StatTarget statTarget;

    [Header("Mechanic (se SkillType = Mechanic)")]
    public MechanicType mechanicType;

    [Header("Levels")]
    public List<SkillLevel> levels = new(); 

    public int MaxLevel => levels.Count;

    public SkillLevel GetLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }
}