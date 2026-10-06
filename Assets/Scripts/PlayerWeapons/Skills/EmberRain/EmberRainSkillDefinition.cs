using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmberRainSkill", menuName = "Player Weapons/Skills/Ember Rain")]
public class EmberRainSkillDefinition : SkillDefinition, IHoldToAimSkill, IMultiChargeSkill
{
    static readonly ESkillStatTarget[] Targets =
    {
        ESkillStatTarget.Damage,
        ESkillStatTarget.Cooldown,
        ESkillStatTarget.Radius,
        ESkillStatTarget.Duration,
        ESkillStatTarget.Range,
    };

    public EmberRainArea areaPrefab;
    [Min(0.05f)] public float pulseInterval = 0.5f;
    [Min(0)] public int burnStacksPerPulse = 1;

    [Header("Mira segurando a tecla")]
    public SkillAimMarker aimMarkerPrefab;
    [Tooltip("Segundos para o marcador ir do vagão até o alcance máximo.")]
    [Min(0.05f)] public float aimTravelSeconds = 0.8f;

    [Header("Cargas (variante Dupla)")]
    [Tooltip("Segundos para lançar as cargas restantes antes de perdê-las e a recarga começar.")]
    [Min(0f)] public float chargeWindowSeconds = 4f;

    public List<EmberRainLevelData> levels = new()
    {
        new EmberRainLevelData(),
    };

    public override int LevelCount => levels.Count;
    public override IReadOnlyList<ESkillStatTarget> DisplayStats => Targets;

    public float AimTravelSeconds => aimTravelSeconds;
    public SkillAimMarker AimMarkerPrefab => aimMarkerPrefab;
    public float ChargeWindowSeconds => chargeWindowSeconds;

    public EmberRainLevelData GetLevel(int level)
    {
        if (levels.Count == 0) return new EmberRainLevelData();
        return levels[Mathf.Clamp(level, 0, levels.Count - 1)];
    }

    public override int GetUpgradeCost(int level) => GetLevel(level).upgradeCost;
    public override float GetCooldown(int level) => GetLevel(level).cooldown;

    public override float GetStatValue(int level, ESkillStatTarget target)
    {
        var data = GetLevel(level);
        switch (target)
        {
            case ESkillStatTarget.Damage: return data.damagePerPulse;
            case ESkillStatTarget.Cooldown: return data.cooldown;
            case ESkillStatTarget.Radius: return data.radius;
            case ESkillStatTarget.Duration: return data.duration;
            case ESkillStatTarget.Range: return data.range;
            default: return 0f;
        }
    }

    public float GetAimMaxDistance(int level) => GetLevel(level).range;
    public float GetAimAreaRadius(int level) => GetLevel(level).radius;

    public int GetChargeCount(int level, SkillVariantDefinition variant)
        => variant is EmberRainDoubleVariant doubleVariant ? doubleVariant.chargeCount : 1;

    public override void Cast(SkillCastContext context, int level, SkillVariantDefinition variant)
    {
        if (areaPrefab == null || !context.HasAimPoint) return;

        var data = GetLevel(level);
        float duration = variant is EmberRainDoubleVariant doubleVariant
            ? doubleVariant.DurationPerCharge(data.duration)
            : data.duration;

        var area = Instantiate(areaPrefab, context.AimPoint, Quaternion.identity);
        area.Init(data.damagePerPulse, data.radius, duration, pulseInterval, context.Burn, burnStacksPerPulse);
    }
}
