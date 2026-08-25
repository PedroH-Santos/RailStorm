using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView
{
    public GameObject Root { get; }

    readonly Image _icon;
    readonly Image _rarityBorder;
    readonly Image _rarityGlow;
    readonly TextMeshProUGUI _levelLabel;
    readonly TooltipTrigger _tooltip;

    public InventorySlotView(GameObject root)
    {
        Root = root;
        _tooltip = root.GetComponent<TooltipTrigger>();
        _icon = FindDeep(root.transform, "Icon")?.GetComponent<Image>();
        _rarityBorder = FindDeep(root.transform, "BackGround")?.GetComponent<Image>();
        _rarityGlow = FindDeep(root.transform, "Pattern")?.GetComponent<Image>();
        _levelLabel = FindDeep(root.transform, "LevelLabel")?.GetComponent<TextMeshProUGUI>();

        ApplyTheme(root.transform);
    }

    public void Apply(InventoryEntry entry)
    {
        Root.SetActive(true);

        if (_icon != null)
            _icon.sprite = entry.Icon;

        if (_rarityBorder != null)
        {
            var plate = RarityHelper.IconPlate(entry.CurrentRarity);
            if (plate != null) _rarityBorder.sprite = plate;
            _rarityBorder.color = entry.RarityColor;
        }

        if (_rarityGlow != null)
        {
            var glow = RarityHelper.IconGlow(entry.CurrentRarity);
            _rarityGlow.enabled = glow != null;
            if (glow != null) _rarityGlow.sprite = glow;
            _rarityGlow.color = RarityHelper.GlowColor(entry.CurrentRarity);
        }

        if (_levelLabel != null)
            _levelLabel.text = entry.LevelDisplay;

        if (_tooltip != null)
            _tooltip.SetSource(entry.Drawable, entry.CurrentRarity);
    }

    void ApplyTheme(Transform root)
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyBodyHighlight(_levelLabel);
        theme.ApplyIconOutline(_levelLabel != null ? _levelLabel.GetComponent<Outline>() : null);
        theme.ApplyIconOutline(_icon != null ? _icon.GetComponent<Outline>() : null);

        var tray = FindDeep(root, "SlotPlate")?.GetComponent<Image>();
        if (tray != null) tray.color = theme.slotTray;

        var shadow = FindDeep(root, "SlotShadow")?.GetComponent<Image>();
        if (shadow != null) shadow.color = theme.dropShadow;
    }

    public void Hide() => Root.SetActive(false);

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindDeep(root.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }
}
