using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ChestLootRoller
{
    public static ItemDefinition PickItem(IReadOnlyList<ItemDefinition> pool, int rarityIndex, Func<ItemDefinition, bool> isExcluded = null)
    {
        if (pool == null || pool.Count == 0) return null;

        bool Allowed(ItemDefinition i) => i != null && (isExcluded == null || !isExcluded(i));

        var atRarity = pool.Where(i => Allowed(i) && i.rarity == rarityIndex).ToList();
        if (atRarity.Count > 0)
            return atRarity[Random.Range(0, atRarity.Count)];

        for (int offset = 1; offset < RarityHelper.Count; offset++)
        {
            var lower = pool.Where(i => Allowed(i) && i.rarity == rarityIndex - offset).ToList();
            if (lower.Count > 0)
                return lower[Random.Range(0, lower.Count)];

            var higher = pool.Where(i => Allowed(i) && i.rarity == rarityIndex + offset).ToList();
            if (higher.Count > 0)
                return higher[Random.Range(0, higher.Count)];
        }

        var any = pool.Where(Allowed).ToList();
        return any.Count > 0 ? any[Random.Range(0, any.Count)] : null;
    }
}
