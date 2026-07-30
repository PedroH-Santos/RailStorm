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
        }

        void Update()
        {
            _timer += Time.deltaTime;

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

        void RollNewStock()
        {
            _lastStock.Clear();
            _lastStock.AddRange(_currentStock);

            var candidates = _itemPool.Where(i => !_lastStock.Contains(i)).ToList();

            if (candidates.Count < slotsCount)
                candidates = new List<ItemDefinition>(_itemPool);

            float luck = playerStats != null ? playerStats.LuckPercent : 0f;

            _currentStock.Clear();
            _currentStock.AddRange(WeightedDraw(candidates, slotsCount, luck));

            OnStockChanged?.Invoke();
        }

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

            return true;
        }

        public bool TryBuy(ItemDefinition item, PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
            => TryBuyMultiple(new[] { item }, stats, itemHandler);
    }
}