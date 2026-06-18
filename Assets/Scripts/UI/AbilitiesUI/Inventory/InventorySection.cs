using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySection
{
    public readonly string SectionName;

    public readonly GameObject Root;

    readonly Transform _container;
    readonly GameObject _slotPrefab;

    readonly List<InventorySlotView> _slotViews = new();

    public InventorySection(string sectionName, Transform container, GameObject slotPrefab, GameObject root)
    {
        SectionName = sectionName;
        Root = root;
        _container = container;
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
            _slotViews.Add(new InventorySlotView(Object.Instantiate(_slotPrefab, _container)));

        for (int i = 0; i < _slotViews.Count; i++)
            _slotViews[i].Root.SetActive(i < needed);
    }
}