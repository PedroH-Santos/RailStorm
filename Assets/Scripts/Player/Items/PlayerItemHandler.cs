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
        if (_stats == null) return;

        switch (item.statTarget)
        {
            case EStatTarget.MoveSpeed:
                _stats.MoveSpeed = item.isMultiplier
                    ? _stats.MoveSpeed * (1f + item.statValue / 100f)
                    : _stats.MoveSpeed + item.statValue;
                break;

            case EStatTarget.MaxHP:
                _stats.MaxHP = item.isMultiplier
                    ? Mathf.RoundToInt(_stats.MaxHP * (1f + item.statValue / 100f))
                    : _stats.MaxHP + (int)item.statValue;
                break;

            case EStatTarget.HP:
                _stats.HP = item.isMultiplier
                    ? Mathf.RoundToInt(_stats.HP * (1f + item.statValue / 100f))
                    : _stats.HP + (int)item.statValue;
                break;

            case EStatTarget.Coins:
                _stats.Coins = item.isMultiplier
                    ? Mathf.RoundToInt(_stats.Coins * (1f + item.statValue / 100f))
                    : _stats.Coins + (int)item.statValue;
                break;

            case EStatTarget.LuckPercent:
                _stats.LuckPercent = item.isMultiplier
                    ? _stats.LuckPercent * (1f + item.statValue / 100f)
                    : _stats.LuckPercent + item.statValue;
                break;

            default:
                Debug.LogWarning($"[Items] EStatTarget.{item.statTarget} não tratado em PlayerItemHandler.");
                break;
        }
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
        if (_stats == null) return;

        float inverseMultiplier = 1f + item.statValue / 100f;
        bool canDivide = !Mathf.Approximately(inverseMultiplier, 0f);

        switch (item.statTarget)
        {
            case EStatTarget.MoveSpeed:
                _stats.MoveSpeed = item.isMultiplier
                    ? (canDivide ? _stats.MoveSpeed / inverseMultiplier : _stats.MoveSpeed)
                    : _stats.MoveSpeed - item.statValue;
                break;

            case EStatTarget.MaxHP:
                _stats.MaxHP = item.isMultiplier
                    ? (canDivide ? Mathf.RoundToInt(_stats.MaxHP / inverseMultiplier) : _stats.MaxHP)
                    : _stats.MaxHP - (int)item.statValue;
                break;

            case EStatTarget.HP:
                _stats.HP = item.isMultiplier
                    ? (canDivide ? Mathf.RoundToInt(_stats.HP / inverseMultiplier) : _stats.HP)
                    : _stats.HP - (int)item.statValue;
                break;

            case EStatTarget.Coins:
                _stats.Coins = item.isMultiplier
                    ? (canDivide ? Mathf.RoundToInt(_stats.Coins / inverseMultiplier) : _stats.Coins)
                    : _stats.Coins - (int)item.statValue;
                break;

            case EStatTarget.LuckPercent:
                _stats.LuckPercent = item.isMultiplier
                    ? (canDivide ? _stats.LuckPercent / inverseMultiplier : _stats.LuckPercent)
                    : _stats.LuckPercent - item.statValue;
                break;

            default:
                Debug.LogWarning($"[Items] EStatTarget.{item.statTarget} não tratado ao reverter em PlayerItemHandler.");
                break;
        }
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