using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [Serializable]
    public struct HighlightRow
    {
        public EStatKey key;
        public GameObject prefab;
    }

    public Transform entitiesContainer;
    public GameObject statRowPrefab;

    [Tooltip("Linha usada para stats marcados como destaque (ex.: Moedas). Se vazio, usa a linha padrão.")]
    public GameObject highlightRowPrefab;

    [Tooltip("Linha de destaque própria de um stat (ex.: Runas). Quem não estiver aqui usa a linha de destaque padrão.")]
    public List<HighlightRow> highlightRowsByStat = new();

    readonly List<(StatRowUI row, StatDescriptor stat)> _bound = new();

    public void Bind(StarterAssets.PlayerStatsAggregator aggregator)
    {
        if (aggregator == null) return;

        _bound.Clear();

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

            var prefab = stat.Highlight ? HighlightPrefabFor(stat.Key) : statRowPrefab;
            var row = Instantiate(prefab, container);
            row.SetActive(true);

            var rowUI = row.GetComponent<StatRowUI>();
            if (rowUI == null) continue;

            rowUI.Setup(stat.Label, stat.GetValue());
            _bound.Add((rowUI, stat));
        }
    }

    GameObject HighlightPrefabFor(EStatKey key)
    {
        foreach (var entry in highlightRowsByStat)
            if (entry.key == key && entry.prefab != null) return entry.prefab;

        return highlightRowPrefab != null ? highlightRowPrefab : statRowPrefab;
    }

    void LateUpdate()
    {
        foreach (var (row, stat) in _bound)
            if (row != null) row.SetValue(stat.GetValue());
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
