using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject sectionPrefab;
    public GameObject slotPrefab;

    [Header("Layout")]
    public Transform sectionsParent;

    [Header("Armas")]
    public CarWeaponHandler weaponHandler;

    readonly Dictionary<string, (InventorySection section, GameObject root)> _sections = new();

    void Awake()
    {
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<CarWeaponHandler>();

        RegisterSection("Armas");
    }

    void OnEnable()
    {
        if (weaponHandler != null)
            weaponHandler.OnWeaponsChanged += RefreshWeapons;

        RefreshWeapons();
    }

    void OnDisable()
    {
        if (weaponHandler != null)
            weaponHandler.OnWeaponsChanged -= RefreshWeapons;
    }


    public void RegisterSection(string sectionName)
    {
        if (_sections.ContainsKey(sectionName)) return;

        var go = Instantiate(sectionPrefab, sectionsParent);
        var label = go.transform.Find("SectionLabel")?.GetComponent<TextMeshProUGUI>();
        var slotsParent = go.transform.Find("SlotsContainer");

        if (label != null) label.text = sectionName;

        var section = new InventorySection(sectionName, slotsParent, slotPrefab);
        _sections[sectionName] = (section, go);
    }

    public void SetSection(string sectionName, IReadOnlyList<InventoryEntry> entries)
    {
        if (!_sections.ContainsKey(sectionName))
            RegisterSection(sectionName);

        _sections[sectionName].section.SetEntries(entries);
    }

    public void HideSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var e)) e.root.SetActive(false);
    }

    public void ShowSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var e)) e.root.SetActive(true);
    }


    void RefreshWeapons()
    {
        if (weaponHandler == null) return;

        var entries = new List<InventoryEntry>();

        foreach (var weapon in weaponHandler.AcquiredWeapons)
            entries.Add(new InventoryEntry(weapon)); 

        while (entries.Count < weaponHandler.maxWeapons)
            entries.Add(InventoryEntry.Locked());

        SetSection("Armas", entries);
    }
}