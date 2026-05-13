using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// CarWeaponHandler
//
// Espelha PlayerSkillHandler para armas do carro:
//   • AcquiredWeapons — lista de armas adquiridas (cap: maxWeapons = 3)
//   • ExiledWeapons   — armas banidas permanentemente (nunca mais oferecidas)
//   • ExileWeapon / IsExiled — mesmo padrão de PlayerSkillHandler
//   • IsFull / HasWeapon — verificações usadas pelo SkillDrawer
//
// Attach no mesmo GameObject que PlayerSkillHandler (o Player).
// ─────────────────────────────────────────────────────────────────────────────
public class CarWeaponHandler : MonoBehaviour
{
    [Header("Config")]
    public int maxWeapons = 3;

    public event Action OnWeaponsChanged;

    readonly List<WeaponDefinition> _acquired = new();
    public IReadOnlyList<WeaponDefinition> AcquiredWeapons => _acquired;

    readonly HashSet<WeaponDefinition> _exiled = new();
    public IReadOnlyCollection<WeaponDefinition> ExiledWeapons => _exiled;

    public bool IsFull => _acquired.Count >= maxWeapons;
    public bool HasWeapon(WeaponDefinition w) => _acquired.Contains(w);
    public bool IsExiled(WeaponDefinition w) => _exiled.Contains(w);

    public bool AcquireWeapon(WeaponDefinition weapon)
    {
        if (weapon == null) return false;

        if (IsFull)
        {
            Debug.Log($"[Car] Slots cheios ({maxWeapons}/{maxWeapons}). Não é possível adicionar '{weapon.weaponName}'.");
            return false;
        }

        if (HasWeapon(weapon))
        {
            Debug.Log($"[Car] '{weapon.weaponName}' já equipada.");
            return false;
        }

        _acquired.Add(weapon);
        Debug.Log($"[Car] Arma adquirida: '{weapon.weaponName}' ({_acquired.Count}/{maxWeapons})");
        OnWeaponsChanged?.Invoke();
        return true;
    }
    public void ExileWeapon(WeaponDefinition weapon)
    {
        _exiled.Add(weapon);
        Debug.Log($"[Car] '{weapon.weaponName}' foi banida permanentemente.");
        OnWeaponsChanged?.Invoke();
    }
}