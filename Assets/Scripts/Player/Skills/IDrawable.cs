
public interface IDrawable
{
    string DisplayName { get; }

    UnityEngine.Sprite Icon { get; }

    SkillRarity Rarity { get; }

    float GetWeight(float luckPercent);
}