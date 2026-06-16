// InventoryEntry.cs
using Assets.Scripts.Systems.Rarity;
using UnityEngine;

public class InventoryEntry
{
    public readonly IDrawable Drawable;
    public readonly int RarityIdx;

    public string RarityDisplay => RarityHelper.DisplayName(RarityIdx);
    public Color RarityColor => RarityHelper.Color(RarityIdx);
    public string DisplayName => Drawable?.DisplayName ?? string.Empty;
    public Sprite Icon => Drawable?.Icon;

    public InventoryEntry(IDrawable drawable, int RarityHelper)
    {
        Drawable = drawable;
        RarityIdx = RarityHelper;
    }
}