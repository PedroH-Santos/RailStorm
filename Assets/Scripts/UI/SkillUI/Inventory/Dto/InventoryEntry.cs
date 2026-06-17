using UnityEngine;

public class InventoryEntry
{
    public readonly IDrawable Drawable;
    public readonly int CurrentRarity;

    public string RarityDisplay => RarityHelper.DisplayName(CurrentRarity);
    public Color RarityColor => RarityHelper.Color(CurrentRarity);
    public string DisplayName => Drawable?.DisplayName ?? string.Empty;
    public Sprite Icon => Drawable?.Icon;

    public InventoryEntry(IDrawable drawable)
    {
        Drawable = drawable;
        CurrentRarity = drawable.CurrentRarity;
    }
}
