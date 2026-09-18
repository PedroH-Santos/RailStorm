using System;
using UnityEngine;

namespace StarterAssets
{
    public class SellManager : MonoBehaviour
    {
        public static SellManager Instance { get; private set; }

        [Header("Settings")]
        [Range(0f, 1f)]
        [Tooltip("Percentual a menos do preço de compra (ItemDefinition.price) que o item vale ao ser vendido. Ex.: 0.15 = vende por 85% do preço de compra.")]
        [SerializeField] private float sellDiscountPercent = 0.15f;

        public event Action OnItemSold;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning($"[Sell] Mais de um SellManager na cena ('{Instance.name}' e '{name}').");
            else
                Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public int GetSellPrice(ItemDefinition item)
        {
            if (item == null) return 0;
            return Mathf.Max(0, Mathf.RoundToInt(item.price * (1f - sellDiscountPercent)));
        }

        public bool TrySell(ItemDefinition item, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
        {
            if (item == null || stats == null || itemHandler == null) return false;
            if (!itemHandler.HasItem(item)) return false;

            int price = GetSellPrice(item);
            itemHandler.RemoveItem(item);
            stats.Coins += price;

            OnItemSold?.Invoke();
            return true;
        }
    }
}
