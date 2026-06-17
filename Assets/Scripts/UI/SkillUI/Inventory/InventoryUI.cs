using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform entitiesContainer;
    public GameObject slotPrefab;
    public CarWeaponHandler weaponHandler;

    const string SectionWeapons = "Weapons";
    readonly Dictionary<string, InventorySection> _sections = new();

    void Awake()
    {
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<CarWeaponHandler>();
        RegisterSection(SectionWeapons);
    }

    void OnEnable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged += RefreshWeapons;
        RefreshWeapons();
    }

    void OnDisable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged -= RefreshWeapons;
    }

    void RefreshWeapons()
    {
        if (weaponHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var w in weaponHandler.AcquiredWeapons)
            entries.Add(new InventoryEntry(w));

        SetSection(SectionWeapons, entries);
    }

    public void SetSection(string name, IReadOnlyList<InventoryEntry> entries)
    {
        if (_sections.TryGetValue(name, out var s)) s.SetEntries(entries);
    }

    public void HideSection(string name)
    {
        if (_sections.TryGetValue(name, out var s)) s.Root.SetActive(false);
    }

    public void ShowSection(string name)
    {
        if (_sections.TryGetValue(name, out var s)) s.Root.SetActive(true);
    }

    void RegisterSection(string sectionName)
    {
        if (_sections.ContainsKey(sectionName)) return;
        var root = entitiesContainer.Find(sectionName);
        if (root == null) { Debug.LogError($"[InventoryUI] '{sectionName}' não encontrado."); return; }
        var container = root.Find("Container");
        if (container == null) { Debug.LogError($"[InventoryUI] 'Container' não encontrado."); return; }
        _sections[sectionName] = new InventorySection(sectionName, container, slotPrefab, root.gameObject);
    }
}
