using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarterAssets;
using Assets.Scripts.Systems.Rarity;
public static class SkillDrawer
{
    public static List<SkillCardData> Draw(
        List<SkillDefinition> skillPool,
        List<WeaponDefinition> weaponPool,
        PlayerSkillHandler skillHandler,
        CarWeaponHandler weaponHandler,
        int count,
        IEnumerable<SkillCardData> exclude = null)
    {
        var cfg = RarityConfig.Instance;
        if (cfg == null) return new List<SkillCardData>();

        float luck = skillHandler?.luckPercent ?? 0f;

        var excludedNames = new HashSet<string>();
        if (exclude != null)
            foreach (var c in exclude)
                excludedNames.Add(c.drawable.DisplayName);

        var candidates = new List<SkillCardData>();

        if (skillPool != null && skillHandler != null)
        {
            foreach (var s in skillPool)
            {
                if (excludedNames.Contains(s.DisplayName)) continue;
                if (skillHandler.IsExiled(s)) continue;

                int currentRi = skillHandler.GetSkillRarityHelper(s);
                if (currentRi >= s.MaxLevelIndex) continue; // já está no máximo

                int targetRi = RollRarityAbove(cfg, currentRi, s.MaxLevelIndex, luck);
                candidates.Add(new SkillCardData(s, targetRi));
            }
        }

        if (weaponPool != null && weaponHandler != null && !weaponHandler.IsFull)
        {
            foreach (var w in weaponPool)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (weaponHandler.HasWeapon(w)) continue;
                if (weaponHandler.IsExiled(w)) continue;

                int targetRi = RollRarity(cfg, 0, cfg.Count - 1, luck);
                candidates.Add(new SkillCardData(w, targetRi));
            }
        }

        if (weaponHandler != null)
        {
            foreach (var w in weaponHandler.AcquiredWeapons)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (!w.CanUpgrade) continue;

                // Upgrade sempre sobe exatamente um nível de raridade
                candidates.Add(new SkillCardData(w, w.NextRarityHelper, true));
            }
        }

        var result = new List<SkillCardData>();
        var remaining = new List<SkillCardData>(candidates);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            float total = remaining.Sum(c => cfg.GetWeight(c.targetRarityHelper, luck));
            if (total <= 0f) break;

            float roll = Random.Range(0f, total);
            float acc = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                acc += cfg.GetWeight(remaining[j].targetRarityHelper, luck);
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

    static int RollRarity(RarityConfig cfg, int minRi, int maxRi, float luck)
    {
        float total = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
            total += cfg.GetWeight(ri, luck);

        if (total <= 0f) return minRi;

        float roll = Random.Range(0f, total);
        float acc = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
        {
            acc += cfg.GetWeight(ri, luck);
            if (roll <= acc) return ri;
        }
        return maxRi;
    }

    static int RollRarityAbove(RarityConfig cfg, int currentRi, int maxRi, float luck)
        => RollRarity(cfg, currentRi + 1, maxRi, luck);
}