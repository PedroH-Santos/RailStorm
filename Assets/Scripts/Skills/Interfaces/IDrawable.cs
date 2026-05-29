
public interface IDrawable
{
    string DisplayName { get; }

    UnityEngine.Sprite Icon { get; }

    ESkillRarity Rarity { get; }

    float GetWeight(float luckPercent);
}