using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    [Header("Referências da hierarquia")]
    public Transform entitiesContainer;  

    [Header("Slot prefab")]
    public GameObject slotPrefab;

    [Header("Armas")]
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
        if (weaponHandler != null)
            weaponHandler.OnWeaponsChanged += RefreshWeapons;

        RefreshWeapons();
    }

    void OnDisable()
    {
        if (weaponHandler != null)
            weaponHandler.OnWeaponsChanged -= RefreshWeapons;
    }


    public void SetSection(string sectionName, IReadOnlyList<InventoryEntry> entries)
    {
        if (_sections.TryGetValue(sectionName, out var section))
            section.SetEntries(entries);
        else
            Debug.LogWarning($"[InventoryUI] Seção '{sectionName}' não registrada. Declare-a no Awake.");
    }

    public void HideSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var s)) s.Root.SetActive(false);
    }

    public void ShowSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var s)) s.Root.SetActive(true);
    }

    void RegisterSection(string sectionName)
    {
        if (_sections.ContainsKey(sectionName)) return;

        var sectionRoot = entitiesContainer.Find(sectionName);
        if (sectionRoot == null)
        {
            Debug.LogError($"[InventoryUI] Nó '{sectionName}' não encontrado em EntitiesContainer.");
            return;
        }

        var container = sectionRoot.Find("Container");
        if (container == null)
        {
            Debug.LogError($"[InventoryUI] Nó 'Container' não encontrado dentro de '{sectionName}'.");
            return;
        }

        _sections[sectionName] = new InventorySection(sectionName, container, slotPrefab, sectionRoot.gameObject);
    }

    void RefreshWeapons()
    {
        if (weaponHandler == null) return;

        var entries = new List<InventoryEntry>();

        foreach (var weapon in weaponHandler.AcquiredWeapons)
            entries.Add(new InventoryEntry(weapon));

        SetSection(SectionWeapons, entries);
    }
}