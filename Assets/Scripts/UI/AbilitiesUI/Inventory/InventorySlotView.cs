using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView
{
    public GameObject Root { get; }

    readonly Image _icon;
    readonly TextMeshProUGUI _levelLabel;
    readonly Image _rarityBorder;
    readonly TooltipTrigger _tooltip;

    public InventorySlotView(GameObject root)
    {
        Root = root;
        _tooltip = root.GetComponent<TooltipTrigger>();
        _icon = FindDeep(root.transform, "Icon")?.GetComponent<Image>();
        _levelLabel = FindDeep(root.transform, "LevelLabel")?.GetComponent<TextMeshProUGUI>();
        _rarityBorder = FindDeep(root.transform, "BackGround")?.GetComponent<Image>();

        UIThemeConfig.Instance?.ApplyBody(_levelLabel);
    }

    public void Apply(InventoryEntry entry)
    {
        Root.SetActive(true);

        if (_icon != null)
            _icon.sprite = entry.Icon;

        if (_levelLabel != null)
        {
            _levelLabel.enabled = true;
            _levelLabel.text = entry.LevelDisplay;
        }

        if (_rarityBorder != null)
            _rarityBorder.color = entry.RarityColor;

        if (_tooltip != null)
            _tooltip.SetSource(entry.Drawable);
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
