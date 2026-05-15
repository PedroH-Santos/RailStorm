using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotView
{
    public GameObject Root { get; }

    readonly Image _icon;
    readonly TextMeshProUGUI _levelLabel;

    public InventorySlotView(GameObject root)
    {
        Root = root;
        _icon = root.transform.Find("Icon")?.GetComponent<Image>();
        _levelLabel = root.transform.Find("LevelLabel")?.GetComponent<TextMeshProUGUI>();
    }

    public void Apply(InventoryEntry entry)
    {
        Root.SetActive(true);

        if (_icon != null)
            _icon.sprite = entry.Icon;

        if (_levelLabel != null)
        {
            bool hasLevel = entry.Level > 0;
            _levelLabel.enabled = hasLevel;
            _levelLabel.text = hasLevel ? $"LVL {entry.Level}" : string.Empty;
        }
    }

    public void Hide() => Root.SetActive(false);
}
