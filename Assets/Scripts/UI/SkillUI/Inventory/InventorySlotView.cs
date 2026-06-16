// InventorySlotView.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView
{
    public GameObject Root { get; }

    readonly Image _icon;
    readonly TextMeshProUGUI _levelLabel;
    readonly Image _rarityBorder; // opcional

    public InventorySlotView(GameObject root)
    {
        Root = root;
        _icon = root.transform.Find("Icon")?.GetComponent<Image>();
        _levelLabel = root.transform.Find("LevelLabel")?.GetComponent<TextMeshProUGUI>();
        _rarityBorder = root.transform.Find("RarityBorder")?.GetComponent<Image>();
    }

    public void Apply(InventoryEntry entry)
    {
        Root.SetActive(true);

        if (_icon != null)
            _icon.sprite = entry.Icon;

        if (_levelLabel != null)
        {
            _levelLabel.enabled = true;
            _levelLabel.text = entry.RarityDisplay;
        }

        if (_rarityBorder != null)
            _rarityBorder.color = entry.RarityColor;
    }

    public void Hide() => Root.SetActive(false);
}