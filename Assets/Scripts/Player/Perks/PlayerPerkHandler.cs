using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerPerkHandler : MonoBehaviour
    {
        public static PlayerPerkHandler Instance { get; private set; }

        PlayerStatsAggregator _stats;

        readonly Dictionary<PerkDefinition, int> _rarityByPerk = new();
        readonly List<PerkDefinition> _acquired = new();
        readonly HashSet<PerkDefinition> _exiled = new();

        public event Action OnPerksChanged;

        public IReadOnlyList<PerkDefinition> AcquiredPerks => _acquired;
        public IReadOnlyCollection<PerkDefinition> ExiledPerks => _exiled;

        public float luckPercent => _stats != null ? _stats.LuckPercent : 0f;

        void Awake()
        {
            Instance = this;
            _stats = GetComponent<PlayerStatsAggregator>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public int GetRarity(PerkDefinition perk)
            => perk != null && _rarityByPerk.TryGetValue(perk, out int r) ? r : -1;

        public bool HasPerk(PerkDefinition perk) => perk != null && _rarityByPerk.ContainsKey(perk);
        public bool IsExiled(PerkDefinition perk) => _exiled.Contains(perk);
        public bool CanLevelUp(PerkDefinition perk) => perk != null && GetRarity(perk) < perk.MaxRarity;
        public int NextRarity(PerkDefinition perk) => GetRarity(perk) + 1;
        public int GetPerkRarityIndex(PerkDefinition perk) => GetRarity(perk);

        public void ApplyPerk(PerkDefinition perk, int rarityIndex)
        {
            if (perk == null) return;

            int current = GetRarity(perk);

            if (rarityIndex <= current)
            {
                Debug.LogWarning($"[Perks] {perk.perkName}: rarityIndex {rarityIndex} não supera o atual {current}.");
                return;
            }

            int applied = Mathf.Clamp(rarityIndex, 0, perk.MaxRarity);
            if (applied <= current) return;

            _rarityByPerk[perk] = applied;

            ApplyStat(perk, applied);

            if (!_acquired.Contains(perk))
                _acquired.Add(perk);

            Debug.Log($"[Perks] {perk.perkName} → {RarityHelper.DisplayName(applied)}");
            OnPerksChanged?.Invoke();
        }

        void ApplyStat(PerkDefinition perk, int rarityIndex)
        {
            if (_stats == null) return;

            PerkLevelData data = perk.GetLevelForRarity(rarityIndex);

            switch (perk.statTarget)
            {
                case EStatTarget.MoveSpeed:
                    _stats.MoveSpeed = data.isMultiplier
                        ? _stats.MoveSpeed * data.statValue
                        : _stats.MoveSpeed + data.statValue;
                    break;

                case EStatTarget.MaxHP:
                    _stats.MaxHP = data.isMultiplier
                        ? _stats.MaxHP * (int)data.statValue
                        : _stats.MaxHP + (int)data.statValue;
                    break;

                case EStatTarget.Coins:
                    _stats.Coins = data.isMultiplier
                        ? _stats.Coins * (int)data.statValue
                        : _stats.Coins + (int)data.statValue;
                    break;

                case EStatTarget.LuckPercent:
                    _stats.LuckPercent = data.isMultiplier
                        ? _stats.LuckPercent * data.statValue
                        : _stats.LuckPercent + data.statValue;
                    break;
            }
        }

        public void ExilePerk(PerkDefinition perk)
        {
            _exiled.Add(perk);
            Debug.Log($"[Perks] {perk.perkName} exilada.");
            OnPerksChanged?.Invoke();
        }

        public void ResetForNewRun()
        {
            _rarityByPerk.Clear();
            _acquired.Clear();
            _exiled.Clear();
            OnPerksChanged?.Invoke();
        }
    }
}
