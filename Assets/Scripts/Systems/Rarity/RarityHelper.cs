using Assets.Scripts.Systems.Rarity;
using UnityEngine;

public static class RarityHelper
{
    public static string DisplayName(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.displayName ?? rarityIndex.ToString();

    public static Color Color(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.color ?? UnityEngine.Color.white;

    public static float GetWeight(int rarityIndex, float luckPercent)
    {
        var def = RarityConfig.Instance?.GetRaw(rarityIndex);
        if (def == null) return 0f;
        return Mathf.Max(0f, def.baseWeight + def.weightPerLuck * Mathf.Clamp(luckPercent, 0f, 100f));
    }

    public static int Count
        => RarityConfig.Instance?.Count ?? 0;
}
