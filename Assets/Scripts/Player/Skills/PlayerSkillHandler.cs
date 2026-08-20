using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        PlayerStatsAggregator _stats;

        public HashSet<SkillDefinition> ExiledSkills { get; private set; } = new();

        public event Action OnSkillsChanged;

        readonly List<SkillDefinition> _acquired = new();

        public IReadOnlyList<SkillDefinition> AcquiredSkills => _acquired;

        public float luckPercent => _stats != null ? _stats.LuckPercent : 0f;

        void Awake()
        {
            _stats = GetComponent<PlayerStatsAggregator>();

        }

        public bool HasSkill(SkillDefinition skill) => skill.IsAcquired;
        public bool IsExiled(SkillDefinition skill) => ExiledSkills.Contains(skill);
        public int GetSkillRarityIndex(SkillDefinition skill) => skill.CurrentRarity;

        public void ApplySkill(SkillDefinition skill, int rarityIndex)
        {
            if (rarityIndex <= skill.CurrentRarity)
            {
                Debug.LogWarning($"[Skills] {skill.skillName}: rarityIndex {rarityIndex} não supera o atual {skill.CurrentRarity}.");
                return;
            }

            if (!skill.IsAcquired)
                skill.Acquire(rarityIndex);
            else
                skill.Upgrade(rarityIndex);

            ApplyStat(skill, skill.CurrentRarity);

            if (!_acquired.Contains(skill))
                _acquired.Add(skill);

            Debug.Log($"[Skills] {skill.skillName} → {RarityHelper.DisplayName(skill.CurrentRarity)}");
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
            ExiledSkills.Add(skill);
            Debug.Log($"[Skills] {skill.skillName} exilada.");
            OnSkillsChanged?.Invoke();
        }

        public void ResetForNewRun()
        {
            _acquired.Clear();
            ExiledSkills.Clear();
            OnSkillsChanged?.Invoke();
        }
    }
}
