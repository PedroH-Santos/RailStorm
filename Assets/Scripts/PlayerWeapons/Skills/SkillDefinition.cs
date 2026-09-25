using System.Collections.Generic;
using UnityEngine;

public abstract class SkillDefinition : ScriptableObject, IDrawable
{
    public string skillName = "Nova Skill";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Loja do ferreiro")]
    [Min(0)] public int purchaseCost = 50;

    public string DisplayName => skillName;
    public Sprite Icon => icon;

    public abstract int LevelCount { get; }
    public int MaxLevel => Mathf.Max(0, LevelCount - 1);

    public int RarityForLevel(int level) => Mathf.Clamp(level, 0, Mathf.Max(0, RarityHelper.Count - 1));

    public abstract int GetUpgradeCost(int level);
    public abstract float GetCooldown(int level);
    public bool HasCooldown(int level) => GetCooldown(level) > 0f;

    public abstract IReadOnlyList<ESkillStatTarget> DisplayStats { get; }
    public abstract float GetStatValue(int level, ESkillStatTarget target);

    public abstract void Cast(SkillCastContext context, int level);

    public IEnumerable<ESkillStatTarget> VisibleStats(int level)
    {
        foreach (var target in DisplayStats)
        {
            if (target == ESkillStatTarget.Cooldown && !HasCooldown(level)) continue;
            yield return target;
        }
    }
}
