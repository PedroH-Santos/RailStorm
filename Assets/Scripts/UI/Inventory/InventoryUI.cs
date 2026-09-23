using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform entitiesContainer;
    public GameObject slotPrefab;
    public PlayerCarWeaponHandler weaponHandler;
    public PlayerItemHandler itemHandler;
    [FormerlySerializedAs("skillHandler")] public StarterAssets.PlayerPerkHandler perkHandler;
    public PlayerSkillHandler playerSkillHandler;

    const string SectionWeapons = "Weapons";
    const string SectionPerks = "Perks";
    const string SectionSkills = "Skills";
    const string SectionItems = "Items";
    readonly Dictionary<string, InventorySection> _sections = new();

    void Awake()
    {
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<PlayerCarWeaponHandler>();
        if (itemHandler == null)
            itemHandler = FindFirstObjectByType<PlayerItemHandler>();
        if (perkHandler == null)
            perkHandler = FindFirstObjectByType<StarterAssets.PlayerPerkHandler>();
        if (playerSkillHandler == null)
            playerSkillHandler = FindFirstObjectByType<PlayerSkillHandler>();

        RegisterSection(SectionWeapons);
        RegisterSection(SectionPerks);
        RegisterSection(SectionSkills);
        RegisterSection(SectionItems);
    }

    void OnEnable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged += RefreshWeapons;
        if (itemHandler != null) itemHandler.OnItemsChanged += RefreshItems;
        if (perkHandler != null) perkHandler.OnPerksChanged += RefreshPerks;
        if (playerSkillHandler != null) playerSkillHandler.OnSkillsChanged += RefreshSkills;

        RefreshWeapons();
        RefreshPerks();
        RefreshSkills();
        RefreshItems();
    }

    void OnDisable()
    {
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged -= RefreshWeapons;
        if (itemHandler != null) itemHandler.OnItemsChanged -= RefreshItems;
        if (perkHandler != null) perkHandler.OnPerksChanged -= RefreshPerks;
        if (playerSkillHandler != null) playerSkillHandler.OnSkillsChanged -= RefreshSkills;
    }

    void RefreshSkills()
    {
        if (playerSkillHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var s in playerSkillHandler.Owned)
            entries.Add(new InventoryEntry(s, 0, playerSkillHandler.GetLevel(s)));

        SetSection(SectionSkills, entries);
    }

    void RefreshWeapons()
    {
        if (weaponHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var w in weaponHandler.AcquiredWeapons)
            entries.Add(new InventoryEntry(w, weaponHandler.GetRarity(w)));

        SetSection(SectionWeapons, entries);
    }

    void RefreshPerks()
    {
        if (perkHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var s in perkHandler.AcquiredPerks)
            entries.Add(new InventoryEntry(s, perkHandler.GetRarity(s)));

        SetSection(SectionPerks, entries);
    }

    void RefreshItems()
    {
        if (itemHandler == null) return;

        var entries = new List<InventoryEntry>();
        foreach (var i in itemHandler.AcquiredItems)
            entries.Add(new InventoryEntry(i, i.rarity));

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
