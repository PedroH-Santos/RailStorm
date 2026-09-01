using System.Collections.Generic;

public static class WeaponStatFormatting
{
    static readonly Dictionary<EWeaponStatTarget, string> Formats = new()
    {
        { EWeaponStatTarget.Damage, "0" },
        { EWeaponStatTarget.AttackRate, "0.##/s" },
        { EWeaponStatTarget.Range, "0.#m" },
        { EWeaponStatTarget.Speed, "0.#" },
        { EWeaponStatTarget.ArrowCount, "0" },
        { EWeaponStatTarget.Area, "0.#m" },
        { EWeaponStatTarget.CastTime, "0.##s" },
    };

    public static string Format(EWeaponStatTarget target, float value)
        => value.ToString(Formats.TryGetValue(target, out var format) ? format : "0.##");

    public static string BuildSummary(WeaponLevelData stats)
    {
        if (stats == null) return string.Empty;

        var parts = new List<string>(stats.DisplayStats.Count);
        foreach (var target in stats.DisplayStats)
            parts.Add($"{StatLabels.Of(target)} {Format(target, stats.GetStatValue(target))}");

        return string.Join("  |  ", parts);
    }

    public static string BuildTransitionSummary(WeaponLevelData previous, WeaponLevelData next)
    {
        if (next == null) return string.Empty;

        var parts = new List<string>(next.DisplayStats.Count);
        foreach (var target in next.DisplayStats)
        {
            string prevValue = previous != null ? Format(target, previous.GetStatValue(target)) : "?";
            string nextValue = Format(target, next.GetStatValue(target));
            parts.Add($"{StatLabels.Of(target)} {prevValue}→{nextValue}");
        }

        return string.Join("  |  ", parts);
    }
}
