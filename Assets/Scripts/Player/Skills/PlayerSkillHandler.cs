using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        public static PlayerSkillHandler Instance { get; private set; }

        PlayerStatsAggregator _stats;

        readonly Dictionary<SkillDefinition, int> _rarityBySkill = new();
        readonly List<SkillDefinition> _acquired = new();
        readonly HashSet<SkillDefinition> _exiled = new();

        public event Action OnSkillsChanged;

        public IReadOnlyList<SkillDefinition> AcquiredSkills => _acquired;
        public IReadOnlyCollection<SkillDefinition> ExiledSkills => _exiled;

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

        public int GetRarity(SkillDefinition skill)
            => skill != null && _rarityBySkill.TryGetValue(skill, out int r) ? r : -1;

        public bool HasSkill(SkillDefinition skill) => skill != null && _rarityBySkill.ContainsKey(skill);
        public bool IsExiled(SkillDefinition skill) => _exiled.Contains(skill);
        public bool CanLevelUp(SkillDefinition skill) => skill != null && GetRarity(skill) < skill.MaxRarity;
        public int NextRarity(SkillDefinition skill) => GetRarity(skill) + 1;
        public int GetSkillRarityIndex(SkillDefinition skill) => GetRarity(skill);

        public void ApplySkill(SkillDefinition skill, int rarityIndex)
        {
            if (skill == null) return;

            int current = GetRarity(skill);

            if (rarityIndex <= current)
            {
                Debug.LogWarning($"[Skills] {skill.skillName}: rarityIndex {rarityIndex} não supera o atual {current}.");
                return;
            }

            int applied = Mathf.Clamp(rarityIndex, 0, skill.MaxRarity);
            if (applied <= current) return;

            _rarityBySkill[skill] = applied;

            ApplyStat(skill, applied);

            if (!_acquired.Contains(skill))
                _acquired.Add(skill);

            Debug.Log($"[Skills] {skill.skillName} → {RarityHelper.DisplayName(applied)}");
            OnSkillsChanged?.Invoke();
        }

        void ApplyStat(SkillDefinition skill, int rarityIndex)
        {
            if (_stats == null) return;

            SkillLevelData data = skill.GetLevelForRarity(rarityIndex);

            switch (skill.statTarget)
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

        public void ExileSkill(SkillDefinition skill)
        {
            _exiled.Add(skill);
            Debug.Log($"[Skills] {skill.skillName} exilada.");
            OnSkillsChanged?.Invoke();
        }

        public void ResetForNewRun()
        {
            _rarityBySkill.Clear();
            _acquired.Clear();
            _exiled.Clear();
            OnSkillsChanged?.Invoke();
        }
    }
}
