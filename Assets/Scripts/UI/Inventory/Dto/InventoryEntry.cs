using UnityEngine;

public class InventoryEntry
{
    public readonly IDrawable Drawable;
    public readonly int CurrentRarity;
    public readonly int CurrentLevel;

    public string RarityDisplay => RarityHelper.DisplayName(CurrentRarity);
    public string LevelDisplay => $"LVL {CurrentLevel + 1}";
    public Color RarityColor => RarityHelper.Color(CurrentRarity);
    public string DisplayName => Drawable?.DisplayName ?? string.Empty;
    public Sprite Icon => Drawable?.Icon;

    public InventoryEntry(IDrawable drawable, int currentRarity)
        : this(drawable, currentRarity, currentRarity)
    {
    }

    public InventoryEntry(IDrawable drawable, int currentRarity, int currentLevel)
    {
        Drawable = drawable;
        CurrentRarity = Mathf.Max(currentRarity, 0);
        CurrentLevel = Mathf.Max(currentLevel, 0);
    }
}
