using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarterAssets;

public static class SkillDrawer
{
    /// <summary>
    /// Sorteia `count` skills do pool, excluindo exiladas, 
    /// já adquiridas no máximo, e opcionalmente as que já estão em uso.
    /// </summary>
    public static List<SkillCardData> Draw(
        List<SkillDefinition> pool,
        PlayerSkillHandler handler,
        int count,
        IEnumerable<SkillDefinition> exclude = null)
    {
        HashSet<string> excluded = exclude != null
            ? new HashSet<string>(exclude.Select(s => s.name))
            : new HashSet<string>();

        List<SkillDefinition> candidates = pool
            .Where(s => !excluded.Contains(s.name))
            .Where(s => !handler.IsExiled(s))
            .Where(s => handler.GetSkillLevel(s) == 0 || handler.CanLevelUp(s))
            .ToList();

        List<SkillCardData> result = new();
        List<SkillDefinition> remaining = new(candidates);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            float total = remaining.Sum(s => s.weight);
            float roll = Random.Range(0f, total);
            float acc = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                acc += remaining[j].weight;
                if (roll <= acc)
                {
                    int current = handler.GetSkillLevel(remaining[j]);
                    result.Add(new SkillCardData(remaining[j], current + 1, remaining[j].weight));
                    remaining.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }
}