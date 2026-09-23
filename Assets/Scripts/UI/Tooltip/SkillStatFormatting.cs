using System.Collections.Generic;

public static class SkillStatFormatting
{
    static readonly Dictionary<ESkillStatTarget, string> Formats = new()
    {
        { ESkillStatTarget.Damage, "0" },
        { ESkillStatTarget.Cooldown, "0.##s" },
        { ESkillStatTarget.Range, "0.#m" },
        { ESkillStatTarget.Speed, "0.#" },
        { ESkillStatTarget.ProjectileCount, "0" },
        { ESkillStatTarget.Spread, "0°" },
    };

    public static string Format(ESkillStatTarget target, float value)
        => value.ToString(Formats.TryGetValue(target, out var format) ? format : "0.##");

    public static bool LowerIsBetter(ESkillStatTarget target) => target == ESkillStatTarget.Cooldown;

    public static bool IsImprovement(ESkillStatTarget target, float current, float next)
        => LowerIsBetter(target) ? next < current - 0.0001f : next > current + 0.0001f;
}
