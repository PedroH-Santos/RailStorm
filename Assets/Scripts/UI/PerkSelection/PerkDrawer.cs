using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarterAssets;

public static class PerkDrawer
{
    public static List<PerkCardData> Draw(
        List<PerkDefinition> perkPool,
        List<CarWeaponDefinition> weaponPool,
        List<WeaponPerkDefinition> weaponPerkPool,
        PlayerPerkHandler perkHandler,
        PlayerCarWeaponHandler weaponHandler,
        int count, 
        IEnumerable<PerkCardData> exclude = null)
    {
        var excludedNames = new HashSet<string>();
        if (exclude != null)
            foreach (var c in exclude)
                excludedNames.Add(c.drawable.DisplayName);

        float luck = perkHandler?.luckPercent ?? 0f;
        var candidates = new List<PerkCardData>();

        if (perkPool != null && perkHandler != null)
        {
            foreach (var s in perkPool)
            {
                if (excludedNames.Contains(s.DisplayName)) continue;
                if (perkHandler.IsExiled(s)) continue;
                if (!perkHandler.CanLevelUp(s)) continue;

                int minRarity = perkHandler.HasPerk(s) ? perkHandler.NextRarity(s) : 0;
                int targetRi = RollRarity(minRarity, s.MaxRarity, luck);
                candidates.Add(new PerkCardData(s, targetRi) { isNew = !perkHandler.HasPerk(s) });
            }
        }

        if (weaponPool != null && weaponHandler != null && !weaponHandler.IsFull)
        {
            foreach (var w in weaponPool)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (weaponHandler.HasWeapon(w)) continue;
                if (weaponHandler.IsExiled(w)) continue;

                int targetRi = RollRarity(0, Mathf.Min(RarityHelper.Count - 1, w.MaxRarity), luck);
                candidates.Add(new PerkCardData(w, targetRi) { isNew = true });
            }
        }

        if (weaponHandler != null)
        {
            foreach (var w in weaponHandler.AcquiredWeapons)
            {
                if (excludedNames.Contains(w.DisplayName)) continue;
                if (!weaponHandler.CanUpgrade(w)) continue;

                int minRarity = weaponHandler.NextRarity(w);
                int targetRi = RollRarity(minRarity, w.MaxRarity, luck);
                candidates.Add(new PerkCardData(w, targetRi, true));
            }
        }

        if (weaponPerkPool != null && weaponHandler != null)
        {
            foreach (var ws in weaponPerkPool)
            {
                if (excludedNames.Contains(ws.DisplayName)) continue;
                if (!weaponHandler.CanLevelUp(ws)) continue;

                CarWeaponDefinition owner = null;
                foreach (var w in weaponHandler.AcquiredWeapons)
                {
                    if (w.weaponType == ws.weaponType) { owner = w; break; }
                }
                if (owner == null) continue;

                int minRarity = weaponHandler.HasWeaponPerk(ws) ? weaponHandler.NextRarity(ws) : 0;
                int targetRi = RollRarity(minRarity, ws.MaxRarity, luck);
                candidates.Add(new PerkCardData(ws, targetRi, owner) { isNew = !weaponHandler.HasWeaponPerk(ws) });
            }
        }

        var result = new List<PerkCardData>();
        var remaining = new List<PerkCardData>(candidates);

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

    static int RollRarity(int minRi, int maxRi, float luck) => RarityRoller.Roll(minRi, maxRi, luck);
}