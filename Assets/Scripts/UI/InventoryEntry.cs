using UnityEngine;

public class InventoryEntry
{
    public readonly IDrawable Drawable;
    public readonly int Level;
    public readonly bool IsLocked;

    public string DisplayName => Drawable?.DisplayName ?? string.Empty;
    public Sprite Icon => Drawable?.Icon;

    public InventoryEntry(IDrawable drawable, int level = 0)
    {
        Drawable = drawable;
        Level = level;
        IsLocked = false;
    }

    private InventoryEntry() { IsLocked = true; }

    public static InventoryEntry Locked() => new InventoryEntry();
}