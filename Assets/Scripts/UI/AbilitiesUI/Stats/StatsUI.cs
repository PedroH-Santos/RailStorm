using System.Collections.Generic;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    public Transform entitiesContainer;
    public GameObject statRowPrefab;

    public void Bind(StarterAssets.PlayerStatsAggregator aggregator)
    {
        if (aggregator == null) return;

        var groups = new Dictionary<string, Transform>();

        foreach (var stat in aggregator.AllStats)
        {
            if (groups.ContainsKey(stat.Group)) continue;

            Transform container = FindDeep(entitiesContainer, stat.Group);
            if (container == null) continue;

            groups[stat.Group] = container;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        foreach (var stat in aggregator.AllStats)
        {
            if (!groups.TryGetValue(stat.Group, out var container)) continue;

            var row = Instantiate(statRowPrefab, container);
            row.SetActive(true);
            row.GetComponent<StatRowUI>()?.Setup(stat.Label, stat.GetValue());
        }
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindDeep(root.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }
}
