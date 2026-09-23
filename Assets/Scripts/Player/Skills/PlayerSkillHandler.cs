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

    readonly List<SkillDefinition> _owned = new();
    readonly Dictionary<SkillDefinition, int> _levelBySkill = new();
    SkillDefinition[] _slots = Array.Empty<SkillDefinition>();
    float[] _cooldownRemaining = Array.Empty<float>();
    float[] _cooldownDuration = Array.Empty<float>();
    int _unlockedSlots;

    public event Action OnSkillsChanged;
    public event Action OnLoadoutChanged;
    public event Action<SkillDefinition> OnSkillUpgraded;
    public event Action<int> OnSkillCast;

    public PlayerWeaponDefinition Weapon => weapon;
    public IReadOnlyList<SkillDefinition> Owned => _owned;
    public int MaxSlots => _slots.Length;
    public int UnlockedSlots => _unlockedSlots;

    void Awake()
    {
        Instance = this;
        BuildInitialLoadout();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        for (int i = 0; i < _cooldownRemaining.Length; i++)
            if (_cooldownRemaining[i] > 0f)
                _cooldownRemaining[i] = Mathf.Max(0f, _cooldownRemaining[i] - Time.deltaTime);
    }

    void BuildInitialLoadout()
    {
        _owned.Clear();
        _levelBySkill.Clear();

        foreach (var skill in startingSkills)
        {
            if (skill == null || _owned.Contains(skill)) continue;
            if (weapon != null && !weapon.Supports(skill)) continue;

            _owned.Add(skill);
            _levelBySkill[skill] = 0;
        }

        int slotCount = Mathf.Clamp(maxSlots, 1, SlotLimit);
        _slots = new SkillDefinition[slotCount];
        _cooldownRemaining = new float[slotCount];
        _cooldownDuration = new float[slotCount];
        _unlockedSlots = Mathf.Clamp(startingUnlockedSlots, 1, slotCount);

        for (int i = 0; i < startingEquipped.Count && i < _unlockedSlots; i++)
            if (Owns(startingEquipped[i]) && IndexOf(startingEquipped[i]) < 0)
                _slots[i] = startingEquipped[i];

        if (IsEmpty() && _owned.Count > 0)
            _slots[0] = _owned[0];
    }

    bool IsEmpty()
    {
        foreach (var slot in _slots)
            if (slot != null) return false;
        return true;
    }

    public bool Owns(SkillDefinition skill) => skill != null && _levelBySkill.ContainsKey(skill);

    public int GetLevel(SkillDefinition skill)
        => skill != null && _levelBySkill.TryGetValue(skill, out int level) ? level : -1;

    public bool IsMaxLevel(SkillDefinition skill) => Owns(skill) && GetLevel(skill) >= skill.MaxLevel;

    public int GetUpgradeCost(SkillDefinition skill)
        => Owns(skill) && !IsMaxLevel(skill) ? skill.GetUpgradeCost(GetLevel(skill)) : 0;

    public bool CanUpgrade(SkillDefinition skill, PlayerStatsAggregator stats)
        => Owns(skill) && !IsMaxLevel(skill) && stats != null && stats.Coins >= GetUpgradeCost(skill);

    public bool TryUpgrade(SkillDefinition skill, PlayerStatsAggregator stats)
    {
        if (!CanUpgrade(skill, stats)) return false;

        stats.SpendCoins(GetUpgradeCost(skill));
        _levelBySkill[skill] = GetLevel(skill) + 1;

        Debug.Log($"[Skills] {skill.skillName} → Nv. {GetLevel(skill) + 1}");
        OnSkillUpgraded?.Invoke(skill);
        OnSkillsChanged?.Invoke();
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool IsSlotUnlocked(int slot) => slot >= 0 && slot < _unlockedSlots;

    public SkillDefinition GetSlot(int slot)
        => slot >= 0 && slot < _slots.Length ? _slots[slot] : null;

    public int IndexOf(SkillDefinition skill)
    {
        if (skill == null) return -1;
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i] == skill) return i;
        return -1;
    }

    public bool Equip(int slot, SkillDefinition skill)
    {
        if (!IsSlotUnlocked(slot) || !Owns(skill)) return false;
        if (_slots[slot] == skill) return false;

        int previousSlot = IndexOf(skill);
        if (previousSlot >= 0)
        {
            _slots[previousSlot] = _slots[slot];
            (_cooldownRemaining[previousSlot], _cooldownRemaining[slot]) = (_cooldownRemaining[slot], _cooldownRemaining[previousSlot]);
            (_cooldownDuration[previousSlot], _cooldownDuration[slot]) = (_cooldownDuration[slot], _cooldownDuration[previousSlot]);
        }
        else
        {
            _cooldownRemaining[slot] = 0f;
            _cooldownDuration[slot] = 0f;
        }

        _slots[slot] = skill;
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool Unequip(int slot)
    {
        if (GetSlot(slot) == null) return false;

        _slots[slot] = null;
        _cooldownRemaining[slot] = 0f;
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool UnlockSlot()
    {
        if (_unlockedSlots >= _slots.Length) return false;

        _unlockedSlots++;
        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool HasCooldown(int slot)
    {
        var skill = GetSlot(slot);
        return skill != null && skill.HasCooldown(GetLevel(skill));
    }

    public float CooldownRemaining(int slot)
        => slot >= 0 && slot < _cooldownRemaining.Length ? _cooldownRemaining[slot] : 0f;

    public float CooldownNormalized(int slot)
    {
        if (slot < 0 || slot >= _cooldownRemaining.Length || _cooldownDuration[slot] <= 0f) return 0f;
        return Mathf.Clamp01(_cooldownRemaining[slot] / _cooldownDuration[slot]);
    }

    public bool IsReady(int slot) => GetSlot(slot) != null && IsSlotUnlocked(slot) && CooldownRemaining(slot) <= 0f;

    public bool TryCast(int slot, SkillCastContext context)
    {
        if (!IsReady(slot)) return false;

        var skill = _slots[slot];
        int level = GetLevel(skill);
        skill.Cast(context, level);

        float cooldown = skill.GetCooldown(level);
        _cooldownDuration[slot] = cooldown;
        _cooldownRemaining[slot] = cooldown;

        OnSkillCast?.Invoke(slot);
        return true;
    }

    public void ResetForNewRun()
    {
        BuildInitialLoadout();
        OnSkillsChanged?.Invoke();
        OnLoadoutChanged?.Invoke();
    }
}
