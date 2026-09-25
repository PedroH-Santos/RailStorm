using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    readonly List<ItemDefinition> _acquiredItems = new();
    readonly List<ItemDefinition> _exiledItems = new();

    PlayerStatsAggregator _stats;

    public event Action OnItemsChanged;

    public IReadOnlyList<ItemDefinition> AcquiredItems => _acquiredItems;

    void Awake()
    {
        _stats = GetComponent<PlayerStatsAggregator>();
    }

    public bool HasItem(ItemDefinition item) => item != null && _acquiredItems.Contains(item);

    public bool AcquireItem(ItemDefinition item)
    {
        if (item == null || HasItem(item)) return false;

        _acquiredItems.Add(item);
        ApplyEffect(item);

        Debug.Log($"[Items] '{item.itemName}' adquirido → {RarityHelper.DisplayName(item.rarity)}");
        OnItemsChanged?.Invoke();
        return true;
    }

    void ApplyEffect(ItemDefinition item)
    {
        switch (item.effectType)
        {
            case EItemEffectType.StatChange:
                ApplyStatChange(item);
                break;
            case EItemEffectType.Ability:
                ApplyAbility(item);
                break;
        }
    }

    void ApplyStatChange(ItemDefinition item)
    {
        if (!TryGetStatValue(item.statTarget, out float current))
        {
            Debug.LogWarning($"[Items] EStatTarget.{item.statTarget} não tratado em PlayerItemHandler.");
            return;
        }

        SetStatValue(item.statTarget, WithItem(item, current));
    }

    public bool TryGetStatValue(EStatTarget target, out float value)
    {
        value = 0f;
        if (_stats == null) return false;

        switch (target)
        {
            case EStatTarget.MoveSpeed: value = _stats.MoveSpeed; return true;
            case EStatTarget.MaxHP: value = _stats.MaxHP; return true;
            case EStatTarget.HP: value = _stats.HP; return true;
            case EStatTarget.Coins: value = _stats.Coins; return true;
            case EStatTarget.LuckPercent: value = _stats.LuckPercent; return true;
            default: return false;
        }
    }

    public bool TryPreviewStatChange(ItemDefinition item, bool removing, out float before, out float after)
    {
        after = 0f;
        if (item == null || item.effectType != EItemEffectType.StatChange || !TryGetStatValue(item.statTarget, out before))
        {
            before = 0f;
            return false;
        }

        after = RoundIfInteger(item.statTarget, removing ? WithoutItem(item, before) : WithItem(item, before));
        return true;
    }

    void SetStatValue(EStatTarget target, float value)
    {
        switch (target)
        {
            case EStatTarget.MoveSpeed: _stats.MoveSpeed = value; break;
            case EStatTarget.MaxHP: _stats.MaxHP = Mathf.RoundToInt(value); break;
            case EStatTarget.HP: _stats.HP = Mathf.RoundToInt(value); break;
            case EStatTarget.Coins: _stats.Coins = Mathf.RoundToInt(value); break;
            case EStatTarget.LuckPercent: _stats.LuckPercent = value; break;
        }
    }

    static bool IsIntegerStat(EStatTarget target) =>
        target == EStatTarget.MaxHP || target == EStatTarget.HP || target == EStatTarget.Coins;

    static float RoundIfInteger(EStatTarget target, float value) =>
        IsIntegerStat(target) ? Mathf.RoundToInt(value) : value;

    static float WithItem(ItemDefinition item, float current) =>
        item.isMultiplier ? current * (1f + item.statValue / 100f) : current + item.statValue;

    static float WithoutItem(ItemDefinition item, float current)
    {
        if (!item.isMultiplier) return current - item.statValue;

        float inverseMultiplier = 1f + item.statValue / 100f;
        return Mathf.Approximately(inverseMultiplier, 0f) ? current : current / inverseMultiplier;
    }

    void ApplyAbility(ItemDefinition item)
    {
        var type = item.GetAbilityType();
        if (type == null)
        {
            Debug.LogWarning($"[Items] '{item.itemName}' é do tipo Ability mas não tem um script de habilidade válido definido.");
            return;
        }

        if (gameObject.GetComponent(type) == null)
            gameObject.AddComponent(type);
    }

    public bool RemoveItem(ItemDefinition item)
    {
        if (item == null || !HasItem(item)) return false;

        RevertEffect(item);
        _acquiredItems.Remove(item);

        Debug.Log($"[Items] '{item.itemName}' removido do inventário (venda).");
        OnItemsChanged?.Invoke();
        return true;
    }

    void RevertEffect(ItemDefinition item)
    {
        switch (item.effectType)
        {
            case EItemEffectType.StatChange:
                RevertStatChange(item);
                break;
            case EItemEffectType.Ability:
                RevertAbility(item);
                break;
        }
    }

    void RevertStatChange(ItemDefinition item)
    {
        if (!TryGetStatValue(item.statTarget, out float current))
        {
            Debug.LogWarning($"[Items] EStatTarget.{item.statTarget} não tratado ao reverter em PlayerItemHandler.");
            return;
        }

        SetStatValue(item.statTarget, WithoutItem(item, current));
    }

    void RevertAbility(ItemDefinition item)
    {
        var type = item.GetAbilityType();
        if (type == null) return;

        var component = gameObject.GetComponent(type);
        if (component != null)
            Destroy(component);
    }

    public bool IsExiled(ItemDefinition item) => item != null && _exiledItems.Contains(item);

    public void ExileItem(ItemDefinition item)
    {
        if (item == null || _exiledItems.Contains(item)) return;
        _exiledItems.Add(item);
        Debug.Log($"[Items] '{item.itemName}' exilado — não aparecerá mais em baús.");
        OnItemsChanged?.Invoke();
    }

    public void ResetForNewRun()
    {
        _acquiredItems.Clear();
        _exiledItems.Clear();
    }
}
