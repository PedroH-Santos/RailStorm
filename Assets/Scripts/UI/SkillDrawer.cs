using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarterAssets;

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
        var excludedSkills = new HashSet<string>();
        var excludedWeapons = new HashSet<string>();

        if (exclude != null)
        {
            foreach (var c in exclude)
            {
                if (c.drawable is SkillDefinition s) excludedSkills.Add(s.name);
                if (c.drawable is WeaponDefinition w) excludedWeapons.Add(w.name);
            }
        }

        var candidates = new List<SkillCardData>();

        foreach (var s in skillPool)
        {
            if (excludedSkills.Contains(s.name)) continue;
            if (skillHandler.IsExiled(s)) continue;
            if (skillHandler.GetSkillLevel(s) != 0 && !skillHandler.CanLevelUp(s)) continue;

            int next = skillHandler.GetSkillLevel(s) + 1;
            candidates.Add(new SkillCardData(s, next));
        }

        if (weaponPool != null && weaponHandler != null && !weaponHandler.IsFull)
        {
            foreach (var w in weaponPool)
            {
                if (excludedWeapons.Contains(w.name)) continue;
                if (weaponHandler.HasWeapon(w)) continue;
                if (weaponHandler.IsExiled(w)) continue;

                candidates.Add(new SkillCardData(w));
            }
        }

        float luck = skillHandler.luckPercent;

        var result = new List<SkillCardData>();
        var remaining = new List<SkillCardData>(candidates);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            float total = remaining.Sum(c => c.drawable.GetWeight(luck));
            float roll = Random.Range(0f, total);
            float acc = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                acc += remaining[j].drawable.GetWeight(luck);
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
}