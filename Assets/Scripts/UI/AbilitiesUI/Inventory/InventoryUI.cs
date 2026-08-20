using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform entitiesContainer;
    public GameObject slotPrefab;
    public PlayerCartWeaponHandler weaponHandler;
    public PlayerItemHandler itemHandler;
    public StarterAssets.PlayerSkillHandler skillHandler;

    const string SectionWeapons = "Weapons";
    const string SectionSkills = "Skills";
    const string SectionItems = "Items";
    readonly Dictionary<string, InventorySection> _sections = new();

    void Awake()
    {
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<PlayerCartWeaponHandler>();
        if (itemHandler == null)
            itemHandler = FindFirstObjectByType<PlayerItemHandler>();
        if (skillHandler == null)
            skillHandler = FindFirstObjectByType<StarterAssets.PlayerSkillHandler>();

        RegisterSection(SectionWeapons);
        RegisterSection(SectionSkills);
        RegisterSection(SectionItems);
    }

    void OnEnable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged += RefreshWeapons;
        if (itemHandler != null) itemHandler.OnItemsChanged += RefreshItems;
        if (skillHandler != null) skillHandler.OnSkillsChanged += RefreshSkills;

        RefreshWeapons();
        RefreshSkills();
        RefreshItems();
    }

    void OnDisable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged -= RefreshWeapons;
        if (itemHandler != null) itemHandler.OnItemsChanged -= RefreshItems;
        if (skillHandler != null) skillHandler.OnSkillsChanged -= RefreshSkills;
    }

    void RefreshWeapons()
    {
        if (weaponHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var w in weaponHandler.AcquiredWeapons)
            entries.Add(new InventoryEntry(w));

        SetSection(SectionWeapons, entries);
    }

    void RefreshSkills()
    {
        if (skillHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var s in skillHandler.AcquiredSkills)
            entries.Add(new InventoryEntry(s));

        SetSection(SectionSkills, entries);
    }

    void RefreshItems()
    {
        if (itemHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var i in itemHandler.AcquiredItems)
            entries.Add(new InventoryEntry(i));

        SetSection(SectionItems, entries);
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
