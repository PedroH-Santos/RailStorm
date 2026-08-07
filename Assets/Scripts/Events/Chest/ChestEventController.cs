using System.Collections.Generic;
using UnityEngine;

public class ChestEventController : EventController
{
    [SerializeField] private List<ChestSpawner> spawners = new();

    public override bool IsOccupied =>
        spawners.Exists(s => s != null && s.HasActiveChest) || !AnyLootAvailable;

    bool AnyLootAvailable => spawners.Exists(s => s != null && s.HasAvailableLoot);

    protected override void ExecuteActivation()
    {
        var candidates = spawners.FindAll(s => s != null && !s.HasActiveChest && s.HasAvailableLoot);
        if (candidates.Count == 0) return;

        candidates[Random.Range(0, candidates.Count)].SpawnRandom();
    }

    protected override void DespawnActive()
    {
        foreach (var spawner in spawners)
            spawner?.DespawnActive();
    }
}
