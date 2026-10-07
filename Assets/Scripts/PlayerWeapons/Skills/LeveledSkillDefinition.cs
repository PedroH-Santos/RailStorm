using System.Collections.Generic;
using UnityEngine;

public abstract class LeveledSkillDefinition<TLevel> : SkillDefinition where TLevel : SkillLevelData, new()
{
    public List<TLevel> levels = new() { new TLevel() };

    public override int LevelCount => levels.Count;

    public TLevel GetLevelStats(int level)
    {
        bool hasNoLevelsConfigured = levels.Count == 0;
        if (hasNoLevelsConfigured) return new TLevel();

        return levels[Mathf.Clamp(level, 0, levels.Count - 1)];
    }

    public override int GetUpgradeCost(int level) => GetLevelStats(level).upgradeCost;
    public override float GetCooldown(int level) => GetLevelStats(level).cooldown;
    public override float GetStatValue(int level, ESkillStatTarget stat) => GetLevelStats(level).GetStat(stat);
}
