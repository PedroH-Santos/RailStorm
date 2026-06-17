using Assets.Scripts.Systems.Rarity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CarWeaponHandler : MonoBehaviour
{
    public int maxWeapons = 3;

    public event Action OnWeaponsChanged;

    readonly List<WeaponDefinition> _acquired = new();
    readonly HashSet<WeaponDefinition> _exiled = new();

    public IReadOnlyList<WeaponDefinition> AcquiredWeapons => _acquired;
    public IReadOnlyCollection<WeaponDefinition> ExiledWeapons => _exiled;

    public bool IsFull => _acquired.Count >= maxWeapons;
    public bool HasWeapon(WeaponDefinition w) => _acquired.Contains(w);
    public bool IsExiled(WeaponDefinition w) => _exiled.Contains(w);
    public bool CanUpgrade(WeaponDefinition w) => HasWeapon(w) && w.CanUpgrade;

    public bool AcquireWeapon(WeaponDefinition weapon, int rarityIndex)
    {
        if (weapon == null || IsFull || HasWeapon(weapon) || IsExiled(weapon))
            return false;

        weapon.Acquire(rarityIndex);
        _acquired.Add(weapon);

        Debug.Log($"[Car] '{weapon.weaponName}' adquirida → {RarityHelper.DisplayName(rarityIndex)}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool UpgradeWeapon(WeaponDefinition weapon)
    {
        if (!CanUpgrade(weapon)) return false;

        weapon.Upgrade();
        Debug.Log($"[Car] '{weapon.weaponName}' → {RarityHelper.DisplayName(weapon.CurrentRarity)}");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public void ExileWeapon(WeaponDefinition weapon)
    {
        _exiled.Add(weapon);
        Debug.Log($"[Car] '{weapon.weaponName}' banida.");
        OnWeaponsChanged?.Invoke();
    }
}
