using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarterAssets
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Pool")]
        [Tooltip("Folder under Assets/Resources to load ItemDefinition assets from.")]
        [SerializeField] private string resourcesFolder = "Items";

        [Header("Player Reference")]
        [Tooltip("Used to read LuckPercent so rarer items become more/less likely to appear.")]
        [SerializeField] private PlayerStatsAggregator playerStats;

        [Tooltip("Used so items the player already owns are never rolled into the shop.")]
        [SerializeField] private PlayerItemHandler playerItemHandler;

        [Header("Settings")]
        public int slotsCount = 9;
        public float refreshInterval = 180f; // 3 minutes

        public IReadOnlyList<ItemDefinition> CurrentStock => _currentStock;
        public float TimeUntilRefresh => Mathf.Max(0f, refreshInterval - _timer);

        public event Action OnStockChanged;

        readonly List<ItemDefinition> _itemPool = new();
        readonly List<ItemDefinition> _currentStock = new();
        readonly List<ItemDefinition> _lastStock = new();
        float _timer;

        void Awake()
        {
            LoadPool();
            RollNewStock();

            if (playerItemHandler != null)
                playerItemHandler.OnItemsChanged += HandlePlayerItemsChanged;
        }

        void OnDestroy()
        {
            if (playerItemHandler != null)
                playerItemHandler.OnItemsChanged -= HandlePlayerItemsChanged;
        }

        // Player can acquire items outside the shop (ex: chest events). When that happens,
        // immediately drop any now-owned item from the current stock instead of waiting for
        // the next scheduled refresh.
        void HandlePlayerItemsChanged()
        {
            int removed = _currentStock.RemoveAll(IsOwned);
            if (removed == 0) return;

            RefillStock();
            OnStockChanged?.Invoke();
        }

        void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (_timer >= refreshInterval)
            {
                _timer = 0f;
                RollNewStock();
            }
        }

        void LoadPool()
        {
            _itemPool.Clear();
            _itemPool.AddRange(Resources.LoadAll<ItemDefinition>(resourcesFolder));

            Debug.Log($"[Shop] {_itemPool.Count} items loaded from Resources/{resourcesFolder}.");
        }

        bool IsOwned(ItemDefinition item) =>
            playerItemHandler != null && playerItemHandler.HasItem(item);

        void RollNewStock()
        {
            _lastStock.Clear();
            _lastStock.AddRange(_currentStock);

            // Exclude items shown in the previous batch AND items already owned.
            var candidates = _itemPool
                .Where(i => !_lastStock.Contains(i))
                .Where(i => !IsOwned(i))
                .ToList();

            // Not enough unique/unowned candidates? Relax the "previous batch" rule.
            if (candidates.Count < slotsCount)
                candidates = _itemPool.Where(i => !IsOwned(i)).ToList();

            float luck = playerStats != null ? playerStats.LuckPercent : 0f;

            _currentStock.Clear();
            _currentStock.AddRange(WeightedDraw(candidates, slotsCount, luck));

            OnStockChanged?.Invoke();
        }

        // Tops up empty slots right after a purchase, without waiting for the full refresh timer.
        void RefillStock()
        {
            int missing = slotsCount - _currentStock.Count;
            if (missing <= 0) return;

            var candidates = _itemPool
                .Where(i => !_currentStock.Contains(i))
                .Where(i => !IsOwned(i))
                .ToList();

            if (candidates.Count == 0) return;

            float luck = playerStats != null ? playerStats.LuckPercent : 0f;
            _currentStock.AddRange(WeightedDraw(candidates, missing, luck));
        }

        // Weighted draw without replacement, same approach as AbilityDrawer.Draw:
        // each item's chance is proportional to RarityHelper.GetWeight(item.rarity, luck),
        // so higher luck skews the shop towards rarer items.
        static List<ItemDefinition> WeightedDraw(List<ItemDefinition> pool, int count, float luck)
        {
            var remaining = new List<ItemDefinition>(pool);
            var result = new List<ItemDefinition>();

            for (int i = 0; i < count && remaining.Count > 0; i++)
            {
                float total = remaining.Sum(item => RarityHelper.GetWeight(item.rarity, luck));
                if (total <= 0f) break;

                float roll = UnityEngine.Random.Range(0f, total);
                float acc = 0f;

                for (int j = 0; j < remaining.Count; j++)
                {
                    acc += RarityHelper.GetWeight(remaining[j].rarity, luck);
                    if (roll <= acc)
                    {
                        result.Add(remaining[j]);
                        remaining.RemoveAt(j);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Buys every valid item in one go: only succeeds if the player can afford the
        /// combined total. Either everything in the cart is purchased, or nothing is.
        /// Purchased items are removed from stock and the shop tries to refill their slots.
        /// </summary>
        public bool TryBuyMultiple(IEnumerable<ItemDefinition> items, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
        {
            if (stats == null || items == null) return false;

            var toBuy = items
                .Where(i => i != null && _currentStock.Contains(i))
                .Where(i => itemHandler == null || !itemHandler.HasItem(i))
                .Distinct()
                .ToList();

            if (toBuy.Count == 0) return false;

            int total = toBuy.Sum(i => i.price);
            if (stats.Coins < total) return false;

            stats.SpendCoins(total);

            foreach (var item in toBuy)
                itemHandler?.AcquireItem(item);

            foreach (var item in toBuy)
                _currentStock.Remove(item);

            RefillStock();

            OnStockChanged?.Invoke();

            return true;
        }

        public bool TryBuy(ItemDefinition item, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
            => TryBuyMultiple(new[] { item }, stats, itemHandler);
    }
}