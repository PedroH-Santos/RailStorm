using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSkillHandler : MonoBehaviour
    {
        PlayerController _controller;

        public HashSet<MechanicType> UnlockedMechanics { get; private set; } = new();

        Dictionary<SkillDefinition, int> _skillLevels = new();
        public IReadOnlyDictionary<SkillDefinition, int> AcquiredSkills => _skillLevels;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        public int GetSkillLevel(SkillDefinition skill)
        {
            return _skillLevels.TryGetValue(skill, out int lvl) ? lvl : 0;
        }

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

            switch (skill.skillType)
            {
                case SkillType.Stat:
                    ApplyStat(skill, nextLevel);
                    break;
                case SkillType.Mechanic:
                    UnlockMechanic(skill);
                    break;
            }

            Debug.Log($"[Skills] {skill.skillName} → Nível {nextLevel}");
        }

        void ApplyStat(SkillDefinition skill, int level)
        {
            SkillLevel data = skill.GetLevel(level);

            switch (skill.statTarget)
            {
                case StatTarget.MoveSpeed:
                    if (data.isMultiplier)
                        _controller.moveSpeed *= data.statValue;
                    else
                        _controller.moveSpeed += data.statValue;
                    break;

                case StatTarget.Coins:
                    _controller.Coins += (int)data.statValue;
                    break;
            }
        }

        void UnlockMechanic(SkillDefinition skill)
        {
            if (UnlockedMechanics.Contains(skill.mechanicType))
            {
                Debug.Log($"[Skills] Mecânica {skill.mechanicType} já desbloqueada.");
                return;
            }

            UnlockedMechanics.Add(skill.mechanicType);

            switch (skill.mechanicType)
            {
                //case MechanicType.Dash:
                //    var dash = GetComponent<DashMechanic>();
                //    if (dash != null) dash.enabled = true;
                //    break;
                //case MechanicType.DoubleJump:
                //    var dj = GetComponent<DoubleJumpMechanic>();
                //    if (dj != null) dj.enabled = true;
                //    break;
                //case MechanicType.Shield:
                //    var shield = GetComponent<ShieldMechanic>();
                //    if (shield != null) shield.enabled = true;
                //    break;
            }
        }

        public bool HasMechanic(MechanicType mechanic) => UnlockedMechanics.Contains(mechanic);
    }
}