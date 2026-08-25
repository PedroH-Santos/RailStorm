using Assets.Scripts.Systems.Rarity;
using UnityEngine;

public static class RarityHelper
{
    const float GlowWhiteBlend = 0.3f;

    public static string DisplayName(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.displayName ?? rarityIndex.ToString();

    public static Color Color(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.color ?? UnityEngine.Color.white;

    public static Sprite IconPlate(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.iconPlate;

    public static Sprite IconGlow(int rarityIndex)
        => RarityConfig.Instance?.GetRaw(rarityIndex)?.iconGlow;

    public static Color GlowColor(int rarityIndex)
        => UnityEngine.Color.Lerp(Color(rarityIndex), UnityEngine.Color.white, GlowWhiteBlend);

    public static float GetWeight(int rarityIndex, float luckPercent)
    {
        var def = RarityConfig.Instance?.GetRaw(rarityIndex);
        if (def == null) return 0f;
        return Mathf.Max(0f, def.baseWeight + def.weightPerLuck * Mathf.Clamp(luckPercent, 0f, 100f));
    }

    public static int Count
        => RarityConfig.Instance?.Count ?? 0;
}
