// PlayerSkillHandler.cs
using Assets.Scripts.Systems.Rarity;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        PlayerStatsAggregator _stats;
        CarWeaponHandler _weaponHandler;

        readonly Dictionary<SkillDefinition, int> _skillRarities = new();
        public IReadOnlyDictionary<SkillDefinition, int> AcquiredSkills => _skillRarities;

        public HashSet<SkillDefinition> ExiledSkills { get; private set; } = new();

        public float luckPercent => _stats != null ? _stats.LuckPercent : 0f;

        void Awake()
        {
            _stats = GetComponent<PlayerStatsAggregator>();
            _weaponHandler = GetComponent<CarWeaponHandler>()
                          ?? FindFirstObjectByType<CarWeaponHandler>();
        }

        public int GetSkillRarityHelper(SkillDefinition skill) =>
            _skillRarities.TryGetValue(skill, out int ri) ? ri : -1;

        public bool HasSkill(SkillDefinition skill) => _skillRarities.ContainsKey(skill);
        public bool IsExiled(SkillDefinition skill) => ExiledSkills.Contains(skill);

        public void ApplySkill(SkillDefinition skill, int rarityHelper)
        {
            int current = GetSkillRarityHelper(skill);

            if (rarityHelper <= current)
            {
                Debug.LogWarning($"[Skills] {skill.skillName}: RarityHelper {rarityHelper} não supera o atual {current}.");
                return;
            }

            _skillRarities[skill] = rarityHelper;
            ApplyStat(skill, rarityHelper);

            Debug.Log($"[Skills] {skill.skillName} → {RarityHelper.DisplayName(rarityHelper)}");
        }

        void ApplyStat(SkillDefinition skill, int RarityHelper)
        {
            if (_stats == null) return;

            SkillLevel data = skill.GetLevelForRarity(RarityHelper);

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
                    ApplyToAllWeapons(d => d.fireRate = data.isMultiplier
                        ? d.fireRate * data.statValue
                        : d.fireRate + data.statValue);
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
            // _weaponHandler.OnWeaponsChanged?.Invoke();
        }

        public void ExileSkill(SkillDefinition skill)
        {
            ExiledSkills.Add(skill);
            Debug.Log($"[Skills] {skill.skillName} exilada.");
        }
    }
}