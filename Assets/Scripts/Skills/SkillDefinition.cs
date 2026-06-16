using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject, IDrawable
{
    [Header("Informações")]
    public string skillName = "Nova Skill";
    public Sprite icon;

    [Header("Atributo afetado")]
    public EStatTarget statTarget;

    [Header("Níveis (um por raridade — ordem segue o RarityConfig)")]
    public List<SkillLevel> levels = new();

    public string DisplayName => skillName;
    public Sprite Icon => icon;

    public int RarityHelper => 0;

    public int MaxLevelIndex => levels.Count - 1;

    public SkillLevel GetLevelForRarity(int RarityHelper)
    {
        if (levels.Count == 0) return new SkillLevel();
        return levels[Mathf.Clamp(RarityHelper, 0, levels.Count - 1)];
    }

    public bool CanLevelUp(int currentRarityHelper) => currentRarityHelper < MaxLevelIndex;
}