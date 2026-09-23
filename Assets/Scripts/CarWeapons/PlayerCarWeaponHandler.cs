using Assets.Scripts.Systems.Rarity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarWeaponHandler : MonoBehaviour
{
    public static PlayerCarWeaponHandler Instance { get; private set; }

    public int maxWeapons = 3;

    public event Action OnWeaponsChanged;

    readonly List<CarWeaponDefinition> _acquired = new();
    readonly HashSet<CarWeaponDefinition> _exiled = new();

    readonly Dictionary<CarWeaponDefinition, int> _rarityByWeapon = new();
    readonly Dictionary<WeaponPerkDefinition, int> _rarityByWeaponPerk = new();
    readonly Dictionary<CarWeaponDefinition, List<WeaponPerkDefinition>> _perksByWeapon = new();
    readonly Dictionary<CarWeaponDefinition, CarWeaponLevelData> _effectiveStatsCache = new();

    static readonly List<WeaponPerkDefinition> EmptyPerks = new();

    public IReadOnlyList<CarWeaponDefinition> AcquiredWeapons => _acquired;
    public IReadOnlyCollection<CarWeaponDefinition> ExiledWeapons => _exiled;

    public bool IsFull => _acquired.Count >= maxWeapons;
    public bool HasWeapon(CarWeaponDefinition w) => w != null && _rarityByWeapon.ContainsKey(w);
    public bool IsExiled(CarWeaponDefinition w) => _exiled.Contains(w);

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public int GetRarity(CarWeaponDefinition w)
        => w != null && _rarityByWeapon.TryGetValue(w, out int r) ? r : -1;

    public bool CanUpgrade(CarWeaponDefinition w) => HasWeapon(w) && GetRarity(w) < w.MaxRarity;
    public int NextRarity(CarWeaponDefinition w) => GetRarity(w) + 1;

    public int GetRarity(WeaponPerkDefinition s)
        => s != null && _rarityByWeaponPerk.TryGetValue(s, out int r) ? r : -1;

    public bool HasWeaponPerk(WeaponPerkDefinition s) => s != null && _rarityByWeaponPerk.ContainsKey(s);
    public bool CanLevelUp(WeaponPerkDefinition s) => s != null && GetRarity(s) < s.MaxRarity;
    public int NextRarity(WeaponPerkDefinition s) => GetRarity(s) + 1;

    public IReadOnlyList<WeaponPerkDefinition> GetAppliedPerks(CarWeaponDefinition w)
        => w != null && _perksByWeapon.TryGetValue(w, out var list) ? list : EmptyPerks;

    public bool AcquireWeapon(CarWeaponDefinition weapon, int rarityIndex)
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

    public bool UpgradeWeapon(CarWeaponDefinition weapon, int targetRarity)
    {
        if (!CanUpgrade(weapon)) return false;
        if (targetRarity <= GetRarity(weapon) || targetRarity > weapon.MaxRarity) return false;

        _rarityByWeapon[weapon] = targetRarity;
        InvalidateCache(weapon);

        Debug.Log($"[Car] '{weapon.weaponName}' → {RarityHelper.DisplayName(targetRarity)}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool ApplyWeaponPerk(CarWeaponDefinition weapon, WeaponPerkDefinition perk, int rarityIndex)
    {
        if (weapon == null || perk == null || perk.weaponType != weapon.weaponType) return false;
        if (!HasWeapon(weapon)) return false;
        if (rarityIndex <= GetRarity(perk)) return false;

        _rarityByWeaponPerk[perk] = Mathf.Clamp(rarityIndex, 0, perk.MaxRarity);

        if (!_perksByWeapon.TryGetValue(weapon, out var list))
        {
            list = new List<WeaponPerkDefinition>();
            _perksByWeapon[weapon] = list;
        }

        if (!list.Contains(perk))
            list.Add(perk);

        InvalidateCache(weapon);

        Debug.Log($"[Car] '{perk.perkName}' aplicada em '{weapon.weaponName}' → {RarityHelper.DisplayName(GetRarity(perk))}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public CarWeaponLevelData GetEffectiveStats(CarWeaponDefinition weapon)
    {
        if (weapon == null) return null;

        if (_effectiveStatsCache.TryGetValue(weapon, out var cached) && cached != null)
            return cached;

        var baseStats = weapon.GetStatsForRarity(Mathf.Max(GetRarity(weapon), 0));
        var stats = baseStats?.Clone();

        if (stats != null)
        {
            foreach (var perk in GetAppliedPerks(weapon))
            {
                int rarity = GetRarity(perk);
                if (rarity < 0) continue;

                var data = perk.GetLevelForRarity(rarity);
                stats.ApplyModifier(perk.statTarget, data.statValue, data.isMultiplier);
            }
        }

        _effectiveStatsCache[weapon] = stats;
        return stats;
    }

    public T GetEffectiveStats<T>(CarWeaponDefinition weapon) where T : CarWeaponLevelData
        => GetEffectiveStats(weapon) as T;

    public CarWeaponLevelData GetCurrentStats(CarWeaponDefinition weapon)
        => weapon?.GetStatsForRarity(Mathf.Max(GetRarity(weapon), 0));

    public CarWeaponLevelData GetNextStats(CarWeaponDefinition weapon)
        => weapon?.GetStatsForRarity(Mathf.Min(GetRarity(weapon) + 1, weapon.MaxRarity));

    public void ExileWeapon(CarWeaponDefinition weapon)
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
        _rarityByWeaponPerk.Clear();
        _perksByWeapon.Clear();
        _effectiveStatsCache.Clear();
        OnWeaponsChanged?.Invoke();
    }

    void InvalidateCache(CarWeaponDefinition weapon) => _effectiveStatsCache.Remove(weapon);
}
