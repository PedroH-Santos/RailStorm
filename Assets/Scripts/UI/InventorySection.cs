using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySection
{
    public readonly string SectionName;

    readonly Transform _slotsContainer;
    readonly GameObject _slotPrefab;

    readonly List<InventorySlotView> _slotViews = new();

    public InventorySection(string sectionName, Transform slotsContainer, GameObject slotPrefab)
    {
        SectionName = sectionName;
        _slotsContainer = slotsContainer;
        _slotPrefab = slotPrefab;
    }

    public void SetEntries(IReadOnlyList<InventoryEntry> entries)
    {
        EnsureSlots(entries.Count);

        for (int i = 0; i < _slotViews.Count; i++)
        {
            if (i < entries.Count)
                _slotViews[i].Apply(entries[i]);
            else
                _slotViews[i].Hide();
        }
    }

    void EnsureSlots(int needed)
    {
        while (_slotViews.Count < needed)
            _slotViews.Add(new InventorySlotView(Object.Instantiate(_slotPrefab, _slotsContainer)));

        for (int i = 0; i < _slotViews.Count; i++)
            _slotViews[i].Root.SetActive(i < needed);
    }
}

public class InventorySlotView
{
    public GameObject Root { get; }

    readonly Image _icon;
    readonly TextMeshProUGUI _levelLabel;
    readonly GameObject _lockedOverlay;

    public InventorySlotView(GameObject root)
    {
        Root = root;
        _icon = root.transform.Find("Icon")?.GetComponent<Image>();
        _levelLabel = root.transform.Find("LevelLabel")?.GetComponent<TextMeshProUGUI>();
        _lockedOverlay = root.transform.Find("LockedOverlay")?.gameObject;
    }

    public void Apply(InventoryEntry entry)
    {
        Root.SetActive(true);

        if (entry.IsLocked)
        {
            if (_icon != null) _icon.sprite = null;
            if (_levelLabel != null) _levelLabel.enabled = false;
            if (_lockedOverlay != null) _lockedOverlay.SetActive(true);
            return;
        }

        if (_icon != null) _icon.sprite = entry.Icon;
        if (_lockedOverlay != null) _lockedOverlay.SetActive(false);

        if (_levelLabel != null)
        {
            bool hasLevel = entry.Level > 0;
            _levelLabel.enabled = hasLevel;
            _levelLabel.text = hasLevel ? $"LVL {entry.Level}" : string.Empty;
        }
    }

    public void Hide() => Root.SetActive(false);
}