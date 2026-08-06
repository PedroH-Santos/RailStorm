using System.Collections.Generic;
using UnityEngine;

public class ChestEventController : EventController
{
    [SerializeField] private List<ChestSpawner> spawners = new();

    public override bool IsOccupied => spawners.Exists(s => s != null && s.HasActiveChest);

    protected override void ExecuteActivation()
    {
        if (spawners.Count == 0) return;

        var candidates = spawners.FindAll(s => s != null && !s.HasActiveChest);
        if (candidates.Count == 0) return;

        candidates[Random.Range(0, candidates.Count)].SpawnRandom();
    }

    protected override void DespawnActive()
    {
        foreach (var spawner in spawners)
            spawner?.DespawnActive();
    }
}
