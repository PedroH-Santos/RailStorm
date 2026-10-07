using System.Collections.Generic;
using UnityEngine;

public abstract class LeveledSkillDefinition<TLevel> : SkillDefinition where TLevel : SkillLevelData, new()
{
    public List<TLevel> levels = new() { new TLevel() };

    public override int LevelCount => levels.Count;

    public TLevel GetLevelStats(SkillLevel level)
    {
        bool hasNoLevelsConfigured = levels.Count == 0;
        if (hasNoLevelsConfigured) return new TLevel();

        return levels[Mathf.Clamp(level.Index, 0, levels.Count - 1)];
    }

    public override int GetUpgradeCost(SkillLevel level) => GetLevelStats(level).upgradeCost;
    public override float GetCooldown(SkillLevel level) => GetLevelStats(level).cooldown;
    public override float GetStatValue(SkillLevel level, ESkillStatTarget stat) => GetLevelStats(level).GetStat(stat);
}
