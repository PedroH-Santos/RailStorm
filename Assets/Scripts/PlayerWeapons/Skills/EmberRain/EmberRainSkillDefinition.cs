using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmberRainSkill", menuName = "Player Weapons/Skills/Ember Rain")]
public class EmberRainSkillDefinition : LeveledSkillDefinition<EmberRainLevelData>, IHoldToAimSkill, IMultiChargeSkill
{
    static readonly ESkillStatTarget[] StatsShownToPlayer =
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

    public override IReadOnlyList<ESkillStatTarget> DisplayStats => StatsShownToPlayer;

    public float AimTravelSeconds => aimTravelSeconds;
    public SkillAimMarker AimMarkerPrefab => aimMarkerPrefab;
    public float GetAimMaxDistance(SkillLevel level) => GetLevelStats(level).range;
    public float GetAimAreaRadius(SkillLevel level) => GetLevelStats(level).radius;

    public float ChargeWindowSeconds => chargeWindowSeconds;
    public int GetChargeCount(SkillLevel level, SkillVariantDefinition variant)
        => variant is EmberRainDoubleVariant doubleRain ? doubleRain.chargeCount : 1;

    public override void Cast(SkillCastContext context, SkillLevel level, SkillVariantDefinition variant)
    {
        if (areaPrefab == null || !context.HasAimPoint) return;

        var stats = GetLevelStats(level);
        var storm = new EmberRainStorm(stats.radius, DurationOfOneCast(stats, variant), pulseInterval);
        var hitPerPulse = new SkillHit(stats.damagePerPulse, context.Burn, burnStacksPerPulse);

        var area = Instantiate(areaPrefab, context.AimPoint, Quaternion.identity);
        area.Begin(storm, hitPerPulse);
    }

    static float DurationOfOneCast(EmberRainLevelData stats, SkillVariantDefinition variant)
        => variant is EmberRainDoubleVariant doubleRain ? doubleRain.DurationPerCharge(stats.duration) : stats.duration;
}
