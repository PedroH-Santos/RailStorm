using System.Collections.Generic;
using UnityEngine;

public abstract class SkillDefinition : ScriptableObject, IDrawable
{
    public string skillName = "Nova Skill";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Loja do ferreiro")]
    [Min(0)] public int purchaseCost = 50;

    [Header("Animação")]
    [Tooltip("Segundos entre o clique e a saída do ataque, para casar com o golpe da animação.")]
    [Min(0)] public float castDelay;

    [Header("Variantes (ativadas com Runa)")]
    public List<SkillVariantDefinition> variants = new();

    public string DisplayName => skillName;
    public Sprite Icon => icon;

    public abstract int LevelCount { get; }
    public SkillLevel LastLevel => SkillLevel.FromIndex(LevelCount - 1);
    public bool IsLastLevel(SkillLevel level) => level.Index >= LastLevel.Index;
    public SkillLevel NextLevelOrLast(SkillLevel level) => IsLastLevel(level) ? LastLevel : level.Next;

    public int RarityAt(SkillLevel level) => Mathf.Clamp(level.Index, 0, Mathf.Max(0, RarityHelper.Count - 1));

    public abstract int GetUpgradeCost(SkillLevel level);
    public abstract float GetCooldown(SkillLevel level);
    public bool HasCooldown(SkillLevel level) => GetCooldown(level) > 0f;

    public abstract IReadOnlyList<ESkillStatTarget> DisplayStats { get; }
    public abstract float GetStatValue(SkillLevel level, ESkillStatTarget stat);

    public IEnumerable<ESkillStatTarget> VisibleStats(SkillLevel level)
    {
        foreach (var stat in DisplayStats)
        {
            bool isCooldownOfSkillWithoutCooldown = stat == ESkillStatTarget.Cooldown && !HasCooldown(level);
            if (!isCooldownOfSkillWithoutCooldown) yield return stat;
        }
    }

    public bool HasVariant(SkillVariantDefinition variant) => variant != null && variants.Contains(variant);

    public abstract void Cast(SkillCastContext context, SkillLevel level, SkillVariantDefinition variant);
}
