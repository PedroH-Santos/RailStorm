namespace Assets.Scripts.Systems.Rarity
{
    public static class RarityHelper
    {
        public static string DisplayName(int RarityHelper) =>
            RarityConfig.Instance?.GetDisplayName(RarityHelper) ?? RarityHelper.ToString();

        public static UnityEngine.Color Color(int RarityHelper) =>
            RarityConfig.Instance?.GetColor(RarityHelper) ?? UnityEngine.Color.white;

        public static float GetWeight(int RarityHelper, float luckPercent) =>
            RarityConfig.Instance?.GetWeight(RarityHelper, luckPercent) ?? 0f;
    }
}