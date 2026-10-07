using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PlayerSkillHandler : MonoBehaviour
{
    public const int SlotLimit = 3;

    public static PlayerSkillHandler Instance { get; private set; }

    [Header("Arma")]
    [SerializeField] private PlayerWeaponDefinition weapon;

    [Header("Loadout inicial (escolhido antes da run)")]
    [SerializeField] private List<SkillDefinition> startingSkills = new();
    [SerializeField] private List<SkillDefinition> startingEquipped = new();

    [Header("Slots")]
    [Range(1, SlotLimit)]
    [SerializeField] private int maxSlots = SlotLimit;
    [Range(1, SlotLimit)]
    [SerializeField] private int startingUnlockedSlots = 1;

    [Header("Variantes")]
    [Min(0)]
    [SerializeField] private int runesPerVariant = 1;

    readonly List<SkillDefinition> _catalog = new();
    readonly List<OwnedSkill> _owned = new();
    readonly HashSet<SkillVariantDefinition> _unlockedVariants = new();
    SkillLoadout _loadout;

    public event Action OnSkillsChanged;
    public event Action OnLoadoutChanged;
    public event Action<OwnedSkill> OnSkillAcquired;
    public event Action<OwnedSkill> OnSkillUpgraded;
    public event Action<OwnedSkill> OnVariantChanged;
    public event Action<int> OnSkillCast;

    public PlayerWeaponDefinition Weapon => weapon;
    public IReadOnlyList<SkillDefinition> Catalog => _catalog;
    public IReadOnlyList<OwnedSkill> Owned => _owned;
    public IReadOnlyList<SkillSlot> Slots => _loadout.Slots;
    public int UnlockedSlotCount => _loadout.UnlockedCount;
    public int RunesPerVariant => runesPerVariant;

    void Awake()
    {
        Instance = this;
        StartNewRun();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        _loadout.Tick(Time.deltaTime);
    }

    public void ResetForNewRun()
    {
        StartNewRun();
        OnSkillsChanged?.Invoke();
        OnLoadoutChanged?.Invoke();
    }

    void StartNewRun()
    {
        BuildCatalog();
        _owned.Clear();
        _unlockedVariants.Clear();
        _loadout = new SkillLoadout(Mathf.Clamp(maxSlots, 1, SlotLimit), startingUnlockedSlots);

        foreach (var skill in startingSkills)
            if (CanOwn(skill)) _owned.Add(new OwnedSkill(skill));

        EquipStartingSkills();
    }

    void BuildCatalog()
    {
        _catalog.Clear();

        bool weaponListsItsSkills = weapon != null && weapon.availableSkills.Count > 0;
        IEnumerable<SkillDefinition> source = weaponListsItsSkills
            ? weapon.availableSkills
            : Resources.LoadAll<SkillDefinition>("Skills");

        foreach (var skill in source)
            if (skill != null && !_catalog.Contains(skill) && WeaponSupports(skill))
                _catalog.Add(skill);
    }

    void EquipStartingSkills()
    {
        for (int slot = 0; slot < startingEquipped.Count && slot < _loadout.UnlockedCount; slot++)
        {
            var owned = FindOwned(startingEquipped[slot]);
            bool alreadyEquipped = owned != null && _loadout.SlotHolding(owned.Definition) != null;
            if (owned != null && !alreadyEquipped) _loadout.Get(slot).Equip(owned);
        }

        bool nothingEquipped = _loadout.IsEmpty;
        if (nothingEquipped && _owned.Count > 0) _loadout.Get(0).Equip(_owned[0]);
    }

    public OwnedSkill FindOwned(SkillDefinition skill)
    {
        foreach (var owned in _owned)
            if (owned.Definition == skill) return owned;
        return null;
    }

    public bool Owns(SkillDefinition skill) => skill != null && FindOwned(skill) != null;

    bool WeaponSupports(SkillDefinition skill) => weapon == null || weapon.Supports(skill);

    bool CanOwn(SkillDefinition skill) => skill != null && !Owns(skill) && WeaponSupports(skill);

    public bool CanBuy(SkillDefinition skill, PlayerStatsAggregator stats)
        => CanOwn(skill) && stats != null && stats.Coins >= skill.purchaseCost;

    public bool TryBuy(SkillDefinition skill, PlayerStatsAggregator stats)
    {
        if (!CanBuy(skill, stats)) return false;

        stats.SpendCoins(skill.purchaseCost);
        return AcquireSkill(skill);
    }

    public bool AcquireSkill(SkillDefinition skill)
    {
        if (!CanOwn(skill)) return false;

        var owned = new OwnedSkill(skill);
        _owned.Add(owned);

        var freeSlot = _loadout.FirstEmptyUnlockedSlot();
        if (freeSlot != null) freeSlot.Equip(owned);

        Debug.Log($"[Skills] Nova skill: {skill.skillName}");
        OnSkillAcquired?.Invoke(owned);
        OnSkillsChanged?.Invoke();
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool CanUpgrade(OwnedSkill skill, PlayerStatsAggregator stats)
        => skill != null && !skill.IsAtMaxLevel && stats != null && stats.Coins >= skill.UpgradeCost;

    public bool TryUpgrade(OwnedSkill skill, PlayerStatsAggregator stats)
    {
        if (!CanUpgrade(skill, stats)) return false;

        stats.SpendCoins(skill.UpgradeCost);
        skill.LevelUp();

        Debug.Log($"[Skills] {skill.Definition.skillName} → Nv. {skill.Level.Number}");
        OnSkillUpgraded?.Invoke(skill);
        OnSkillsChanged?.Invoke();
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool IsVariantUnlocked(SkillVariantDefinition variant) => variant != null && _unlockedVariants.Contains(variant);

    public int GetVariantCost(SkillVariantDefinition variant) => IsVariantUnlocked(variant) ? 0 : runesPerVariant;

    public bool CanActivateVariant(OwnedSkill skill, SkillVariantDefinition variant, PlayerStatsAggregator stats)
        => skill != null && skill.Definition.HasVariant(variant) && !skill.IsUsing(variant)
           && stats != null && stats.Runes >= GetVariantCost(variant);

    public bool TryActivateVariant(OwnedSkill skill, SkillVariantDefinition variant, PlayerStatsAggregator stats)
    {
        if (!CanActivateVariant(skill, variant, stats)) return false;

        stats.SpendRunes(GetVariantCost(variant));
        _unlockedVariants.Add(variant);
        skill.UseVariant(variant);
        DropUnusedCharges(skill);

        Debug.Log($"[Skills] {skill.Definition.skillName}: variante {variant.variantName} ativa");
        OnVariantChanged?.Invoke(skill);
        return true;
    }

    public bool TryUseBaseForm(OwnedSkill skill)
    {
        if (skill == null || skill.ActiveVariant == null) return false;

        skill.UseBaseForm();
        DropUnusedCharges(skill);
        OnVariantChanged?.Invoke(skill);
        return true;
    }

    void DropUnusedCharges(OwnedSkill skill)
    {
        var slot = _loadout.SlotHolding(skill.Definition);
        if (slot != null) slot.Charges.Clear();
    }

    public SkillSlot GetSlot(int index) => _loadout.Get(index);

    public SkillSlot SlotHolding(SkillDefinition skill) => _loadout.SlotHolding(skill);

    public bool Equip(int slot, OwnedSkill skill)
    {
        if (!_loadout.Equip(slot, skill)) return false;

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool Unequip(int slot)
    {
        if (!_loadout.Unequip(slot)) return false;

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool UnlockSlot()
    {
        if (!_loadout.UnlockNextSlot()) return false;

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool TryCast(int slotIndex, SkillCastContext context)
    {
        var slot = _loadout.Get(slotIndex);
        if (slot == null || !slot.IsReady) return false;

        slot.Cast(context);
        OnSkillCast?.Invoke(slotIndex);
        return true;
    }
}
