using System.Collections.Generic;

public static class CarWeaponStatFormatting
{
    static readonly Dictionary<ECarWeaponStatTarget, string> Formats = new()
    {
        { ECarWeaponStatTarget.Damage, "0" },
        { ECarWeaponStatTarget.AttackRate, "0.##/s" },
        { ECarWeaponStatTarget.Range, "0.#m" },
        { ECarWeaponStatTarget.Speed, "0.#" },
        { ECarWeaponStatTarget.ArrowCount, "0" },
        { ECarWeaponStatTarget.Area, "0.#m" },
        { ECarWeaponStatTarget.CastTime, "0.##s" },
    };

    public static string Format(ECarWeaponStatTarget target, float value)
        => value.ToString(Formats.TryGetValue(target, out var format) ? format : "0.##");

    public static string BuildSummary(CarWeaponLevelData stats)
    {
        if (stats == null) return string.Empty;

        var parts = new List<string>(stats.DisplayStats.Count);
        foreach (var target in stats.DisplayStats)
            parts.Add($"{StatLabels.Of(target)} {Format(target, stats.GetStatValue(target))}");

        return string.Join("  |  ", parts);
    }

    public static string BuildTransitionSummary(CarWeaponLevelData previous, CarWeaponLevelData next)
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
