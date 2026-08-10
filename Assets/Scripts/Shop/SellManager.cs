using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Vende todos os itens válidos de uma vez: soma o valor de venda de cada item
        /// (preço de compra menos o desconto configurado), credita as moedas e remove
        /// os itens do inventário do player, revertendo o efeito que eles concederam.
        /// </summary>
        public bool TrySellMultiple(IEnumerable<ItemDefinition> items, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
        {
            if (stats == null || itemHandler == null || items == null) return false;

            var toSell = items
                .Where(i => i != null && itemHandler.HasItem(i))
                .Distinct()
                .ToList();

            if (toSell.Count == 0) return false;

            int total = toSell.Sum(GetSellPrice);

            foreach (var item in toSell)
                itemHandler.RemoveItem(item);

            stats.Coins += total;

            OnItemSold?.Invoke();
            return true;
        }

        public bool TrySell(ItemDefinition item, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
            => TrySellMultiple(new[] { item }, stats, itemHandler);
    }
}
