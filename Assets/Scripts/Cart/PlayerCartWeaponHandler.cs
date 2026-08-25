using Assets.Scripts.Systems.Rarity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCartWeaponHandler : MonoBehaviour
{
    public static PlayerCartWeaponHandler Instance { get; private set; }

    public int maxWeapons = 3;

    public event Action OnWeaponsChanged;

    readonly List<WeaponDefinition> _acquired = new();
    readonly HashSet<WeaponDefinition> _exiled = new();

    readonly Dictionary<WeaponDefinition, int> _rarityByWeapon = new();
    readonly Dictionary<WeaponSkillDefinition, int> _rarityByWeaponSkill = new();
    readonly Dictionary<WeaponDefinition, List<WeaponSkillDefinition>> _skillsByWeapon = new();
    readonly Dictionary<WeaponDefinition, WeaponLevelData> _effectiveStatsCache = new();

    static readonly List<WeaponSkillDefinition> EmptySkills = new();

    public IReadOnlyList<WeaponDefinition> AcquiredWeapons => _acquired;
    public IReadOnlyCollection<WeaponDefinition> ExiledWeapons => _exiled;

    public bool IsFull => _acquired.Count >= maxWeapons;
    public bool HasWeapon(WeaponDefinition w) => w != null && _rarityByWeapon.ContainsKey(w);
    public bool IsExiled(WeaponDefinition w) => _exiled.Contains(w);

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public int GetRarity(WeaponDefinition w)
        => w != null && _rarityByWeapon.TryGetValue(w, out int r) ? r : -1;

    public bool CanUpgrade(WeaponDefinition w) => HasWeapon(w) && GetRarity(w) < w.MaxRarity;
    public int NextRarity(WeaponDefinition w) => GetRarity(w) + 1;

    public int GetRarity(WeaponSkillDefinition s)
        => s != null && _rarityByWeaponSkill.TryGetValue(s, out int r) ? r : -1;

    public bool HasWeaponSkill(WeaponSkillDefinition s) => s != null && _rarityByWeaponSkill.ContainsKey(s);
    public bool CanLevelUp(WeaponSkillDefinition s) => s != null && GetRarity(s) < s.MaxRarity;
    public int NextRarity(WeaponSkillDefinition s) => GetRarity(s) + 1;

    public IReadOnlyList<WeaponSkillDefinition> GetAppliedSkills(WeaponDefinition w)
        => w != null && _skillsByWeapon.TryGetValue(w, out var list) ? list : EmptySkills;

    public bool AcquireWeapon(WeaponDefinition weapon, int rarityIndex)
    {
        if (weapon == null || IsFull || HasWeapon(weapon) || IsExiled(weapon))
            return false;

        _rarityByWeapon[weapon] = Mathf.Clamp(rarityIndex, 0, weapon.MaxRarity);
        _acquired.Add(weapon);
        InvalidateCache(weapon);

        Debug.Log($"[Car] '{weapon.weaponName}' adquirida → {RarityHelper.DisplayName(GetRarity(weapon))}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool UpgradeWeapon(WeaponDefinition weapon, int targetRarity)
    {
        if (!CanUpgrade(weapon)) return false;
        if (targetRarity <= GetRarity(weapon) || targetRarity > weapon.MaxRarity) return false;

        _rarityByWeapon[weapon] = targetRarity;
        InvalidateCache(weapon);

        Debug.Log($"[Car] '{weapon.weaponName}' → {RarityHelper.DisplayName(targetRarity)}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool ApplyWeaponSkill(WeaponDefinition weapon, WeaponSkillDefinition skill, int rarityIndex)
    {
        if (weapon == null || skill == null || skill.weaponType != weapon.weaponType) return false;
        if (!HasWeapon(weapon)) return false;
        if (rarityIndex <= GetRarity(skill)) return false;

        _rarityByWeaponSkill[skill] = Mathf.Clamp(rarityIndex, 0, skill.MaxRarity);

        if (!_skillsByWeapon.TryGetValue(weapon, out var list))
        {
            list = new List<WeaponSkillDefinition>();
            _skillsByWeapon[weapon] = list;
        }

        if (!list.Contains(skill))
            list.Add(skill);

        InvalidateCache(weapon);

        Debug.Log($"[Car] '{skill.skillName}' aplicada em '{weapon.weaponName}' → {RarityHelper.DisplayName(GetRarity(skill))}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public WeaponLevelData GetEffectiveStats(WeaponDefinition weapon)
    {
        if (weapon == null) return null;

        if (_effectiveStatsCache.TryGetValue(weapon, out var cached) && cached != null)
            return cached;

        var baseStats = weapon.GetStatsForRarity(Mathf.Max(GetRarity(weapon), 0));
        var stats = baseStats?.Clone();

        if (stats != null)
        {
            foreach (var skill in GetAppliedSkills(weapon))
            {
                int rarity = GetRarity(skill);
                if (rarity < 0) continue;

                var data = skill.GetLevelForRarity(rarity);
                stats.ApplyModifier(skill.statTarget, data.statValue, data.isMultiplier);
            }
        }

        _effectiveStatsCache[weapon] = stats;
        return stats;
    }

    public T GetEffectiveStats<T>(WeaponDefinition weapon) where T : WeaponLevelData
        => GetEffectiveStats(weapon) as T;

    public WeaponLevelData GetCurrentStats(WeaponDefinition weapon)
        => weapon?.GetStatsForRarity(Mathf.Max(GetRarity(weapon), 0));

    public WeaponLevelData GetNextStats(WeaponDefinition weapon)
        => weapon?.GetStatsForRarity(Mathf.Min(GetRarity(weapon) + 1, weapon.MaxRarity));

    public void ExileWeapon(WeaponDefinition weapon)
    {
        _exiled.Add(weapon);
        Debug.Log($"[Car] '{weapon.weaponName}' banida.");
        OnWeaponsChanged?.Invoke();
    }

    public void ResetForNewRun()
    {
        _acquired.Clear();
        _exiled.Clear();
        _rarityByWeapon.Clear();
        _rarityByWeaponSkill.Clear();
        _skillsByWeapon.Clear();
        _effectiveStatsCache.Clear();
        OnWeaponsChanged?.Invoke();
    }

    void InvalidateCache(WeaponDefinition weapon) => _effectiveStatsCache.Remove(weapon);
}
