using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarWeaponHandler : MonoBehaviour
{
    [Header("Config")]
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

    public bool AcquireWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || IsFull || HasWeapon(weapon)) return false;

        weapon.Acquire();
        _acquired.Add(weapon);

        Debug.Log($"[Car] '{weapon.weaponName}' adquirida → Nível 1 (Common)");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool UpgradeWeapon(WeaponDefinition weapon)
    {
        if (!CanUpgrade(weapon)) return false;

        weapon.Upgrade();

        Debug.Log($"[Car] '{weapon.weaponName}' → Nível {weapon.CurrentLevel} ({weapon.Rarity.DisplayName()})");
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public void ExileWeapon(WeaponDefinition weapon)
    {
        _exiled.Add(weapon);
        Debug.Log($"[Car] '{weapon.weaponName}' banida permanentemente.");
        OnWeaponsChanged?.Invoke();
    }
}
