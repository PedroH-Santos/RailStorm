using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        PlayerStatsAggregator _stats;

        Dictionary<SkillDefinition, int> _skillLevels = new();
        public IReadOnlyDictionary<SkillDefinition, int> AcquiredSkills => _skillLevels;
        public HashSet<SkillDefinition> ExiledSkills { get; private set; } = new();

        public float luckPercent => _stats != null ? _stats.LuckPercent : 0f;

        void Awake()
        {
            _stats = GetComponent<PlayerStatsAggregator>();
        }

        public int GetSkillLevel(SkillDefinition skill)
            => _skillLevels.TryGetValue(skill, out int lvl) ? lvl : 0;

        public bool HasSkill(SkillDefinition skill) => _skillLevels.ContainsKey(skill);

        public bool CanLevelUp(SkillDefinition skill)
        {
            int current = GetSkillLevel(skill);
            return current > 0 && current < skill.MaxLevel;
        }

        public void ApplySkill(SkillDefinition skill)
        {
            int currentLevel = GetSkillLevel(skill);
            int nextLevel = currentLevel + 1;

            if (nextLevel > skill.MaxLevel)
            {
                Debug.Log($"[Skills] {skill.skillName} já está no nível máximo.");
                return;
            }

            _skillLevels[skill] = nextLevel;

            ApplyStat(skill, nextLevel);

            Debug.Log($"[Skills] {skill.skillName} → Nível {nextLevel}");
        }

        void ApplyStat(SkillDefinition skill, int level)
        {
            if (_stats == null) return;

            SkillLevel data = skill.GetLevel(level);

            switch (skill.statTarget)
            {
                case StatTarget.MoveSpeed:
                    _stats.MoveSpeed = data.isMultiplier
                        ? _stats.MoveSpeed * data.statValue
                        : _stats.MoveSpeed + data.statValue;
                    break;

                case StatTarget.Coins:
                    _stats.Coins = data.isMultiplier
                        ? _stats.Coins * (int)data.statValue
                        : _stats.Coins + (int)data.statValue;
                    break;
                case StatTarget.MaxHP:
                    _stats.MaxHP = data.isMultiplier
                        ? _stats.MaxHP * (int)data.statValue
                        : _stats.MaxHP + (int)data.statValue;
                    break;
            }
        }

        public void ExileSkill(SkillDefinition skill)
        {
            ExiledSkills.Add(skill);
            Debug.Log($"[Skills] {skill.skillName} foi exilada permanentemente.");
        }

        public bool IsExiled(SkillDefinition skill) => ExiledSkills.Contains(skill);
    }
}