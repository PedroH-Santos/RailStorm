using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject, IDrawable
{
    public string skillName = "Nova Skill";
    public Sprite icon;
    [TextArea] public string description = "";
    public EStatTarget statTarget;

    [SerializeField] private int currentRarity = -1;

    public List<SkillLevelData> levels = new()
    {
        new SkillLevelData { statValue = 5f,  isMultiplier = false },
        new SkillLevelData { statValue = 10f, isMultiplier = false },
        new SkillLevelData { statValue = 15f, isMultiplier = false },
        new SkillLevelData { statValue = 20f, isMultiplier = false },
        new SkillLevelData { statValue = 25f, isMultiplier = false },
    };

    public string DisplayName => skillName;
    public Sprite Icon => icon;
    public int CurrentRarity => currentRarity;

    public bool IsAcquired => currentRarity >= 0;
    public bool CanLevelUp => currentRarity < levels.Count - 1;
    public int MaxRarity => levels.Count - 1;
    public int NextRarity => currentRarity + 1;

    public SkillLevelData GetLevelForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return new SkillLevelData();
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public void Acquire(int rarityIndex)
    {
        currentRarity = Mathf.Clamp(rarityIndex, 0, levels.Count - 1);
    }

    public bool Upgrade(int targetRarity)
    {
        if (!CanLevelUp) return false;
        if (targetRarity <= currentRarity || targetRarity >= levels.Count) return false;
        currentRarity = targetRarity;
        return true;
    }

    public void ResetForNewRun()
    {
        currentRarity = -1;
    }
}
