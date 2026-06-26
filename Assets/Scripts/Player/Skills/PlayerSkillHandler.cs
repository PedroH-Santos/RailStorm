using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        PlayerStatsAggregator _stats;
        PlayerCartWeaponHandler _weaponHandler;

        public HashSet<SkillDefinition> ExiledSkills { get; private set; } = new();

        public float luckPercent => _stats != null ? _stats.LuckPercent : 0f;

        void Awake()
        {
            _stats = GetComponent<PlayerStatsAggregator>();
            _weaponHandler = GetComponent<PlayerCartWeaponHandler>()
                          ?? FindFirstObjectByType<PlayerCartWeaponHandler>();
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
            Debug.Log($"[Skills] {skill.skillName} → {RarityHelper.DisplayName(skill.CurrentRarity)}");
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

                case EStatTarget.CarWeaponDamage:
                    ApplyToAllWeapons(d => d.damage = data.isMultiplier
                        ? (int)(d.damage * data.statValue)
                        : d.damage + (int)data.statValue);
                    break;

                case EStatTarget.CarFireRate:
                    ApplyToAllWeapons(d => d.attackRate = data.isMultiplier
                        ? d.attackRate * data.statValue
                        : d.attackRate + data.statValue);
                    break;

                case EStatTarget.CarRange:
                    ApplyToAllWeapons(d => d.range = data.isMultiplier
                        ? d.range * data.statValue
                        : d.range + data.statValue);
                    break;
            }
        }

        void ApplyToAllWeapons(System.Action<WeaponLevelData> modifier)
        {
            if (_weaponHandler == null) return;
            foreach (var w in _weaponHandler.AcquiredWeapons)
                modifier(w.CurrentStats);
            //_weaponHandler.OnWeaponsChanged?.Invoke();
        }

        public void ExileSkill(SkillDefinition skill)
        {
            ExiledSkills.Add(skill);
            Debug.Log($"[Skills] {skill.skillName} exilada.");
        }
    }
}
