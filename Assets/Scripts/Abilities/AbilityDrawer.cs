using Assets.Scripts.Systems.Rarity;
using StarterAssets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AbilityDrawer
{
    public static List<AbilityCardData> Draw(
        List<SkillDefinition> skillPool,
        List<WeaponDefinition> weaponPool,
        PlayerSkillHandler skillHandler,
        CarWeaponHandler weaponHandler,
        int count,
        IEnumerable<AbilityCardData> exclude = null)
    {
        var excludedNames = new HashSet<string>();
        if (exclude != null)
            foreach (var c in exclude)
                excludedNames.Add(c.drawable.DisplayName);

        float luck = skillHandler?.luckPercent ?? 0f;
        var candidates = new List<AbilityCardData>();

        if (skillPool != null && skillHandler != null)
        {
            foreach (var s in skillPool)
            {
                if (excludedNames.Contains(s.DisplayName)) continue;
                if (skillHandler.IsExiled(s)) continue;
                if (!s.CanLevelUp && s.IsAcquired) continue;

                int minRarity = s.IsAcquired ? s.NextRarity : 0;
                int targetRi = RollRarity(minRarity, s.MaxRarity, luck);
                candidates.Add(new AbilityCardData(s, targetRi));
            }
        }

        if (weaponPool != null && weaponHandler != null && !weaponHandler.IsFull)
        {
            foreach (var w in weaponPool)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (weaponHandler.HasWeapon(w)) continue;
                if (weaponHandler.IsExiled(w)) continue;

                int targetRi = RollRarity(0, RarityHelper.Count - 1, luck);
                candidates.Add(new AbilityCardData(w, targetRi));
            }
        }

        if (weaponHandler != null)
        {
            foreach (var w in weaponHandler.AcquiredWeapons)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (!w.CanUpgrade) continue;

                candidates.Add(new AbilityCardData(w, w.NextRarity, true));
            }
        }

        var result = new List<AbilityCardData>();
        var remaining = new List<AbilityCardData>(candidates);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            float total = remaining.Sum(c => RarityHelper.GetWeight(c.targetRarity, luck));
            if (total <= 0f) break;

            float roll = Random.Range(0f, total);
            float acc = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                acc += RarityHelper.GetWeight(remaining[j].targetRarity, luck);
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

    static int RollRarity(int minRi, int maxRi, float luck)
    {
        float total = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
            total += RarityHelper.GetWeight(ri, luck);

        if (total <= 0f) return minRi;

        float roll = Random.Range(0f, total);
        float acc = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
        {
            acc += RarityHelper.GetWeight(ri, luck);
            if (roll <= acc) return ri;
        }
        return maxRi;
    }
}
