public enum ESkillRarity
{
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5
}

public static class ESkillRarityExtensions
{
    public static ESkillRarity FromLevel(int level) => level switch
    {
        1 => ESkillRarity.Common,
        2 => ESkillRarity.Uncommon,
        3 => ESkillRarity.Rare,
        4 => ESkillRarity.Epic,
        5 => ESkillRarity.Legendary,
        _ => ESkillRarity.Common
    };

    public static string DisplayName(this ESkillRarity rarity) => rarity switch
    {
        ESkillRarity.Common => "Comum",
        ESkillRarity.Uncommon => "Incomum",
        ESkillRarity.Rare => "Rara",
        ESkillRarity.Epic => "Épica",
        ESkillRarity.Legendary => "Lendária",
        _ => "?"
    };
}

public static class WeightTable
{
    public static float GetWeight(int targetLevel, float luckPercent)
    {
        float w1 = 60f, w2 = 25f, w3 = 10f, w4 = 4f, w5 = 1f;

        float luck = UnityEngine.Mathf.Clamp(luckPercent, 0f, 100f);
        w1 = UnityEngine.Mathf.Max(0f, w1 - luck * 0.30f);
        w2 = UnityEngine.Mathf.Max(0f, w2 - luck * 0.10f);
        w3 += luck * 0.15f;
        w4 += luck * 0.15f;
        w5 += luck * 0.10f;

        return targetLevel switch { 1 => w1, 2 => w2, 3 => w3, 4 => w4, 5 => w5, _ => 0f };
    }
}
