using UnityEngine;

public class StatsUI : MonoBehaviour
{
    public Transform entitiesContainer;
    public GameObject statRowPrefab;

    public void Bind(StarterAssets.PlayerStatsAggregator aggregator)
    {
        foreach (Transform child in entitiesContainer)
            foreach (Transform row in child)
                Destroy(row.gameObject);

        if (aggregator == null) return;

        foreach (var stat in aggregator.AllStats)
        {
            Transform container = entitiesContainer.Find(stat.Group);
            if (container == null) continue;

            var row = Instantiate(statRowPrefab, container);
            row.SetActive(true);
            row.GetComponent<StatRowUI>()?.Setup(stat.Label, stat.GetValue());
        }
    }
}