using System.Collections.Generic;
using UnityEngine;

public class HordeEventController : EventController
{
    [SerializeField] private List<HordeTotemSpawner> totemSpawners = new();
    [SerializeField] private HordeSpawner hordeSpawner;

    // Ocupa a categoria desde o spawn do totem até o fim do combate da horda
    // (não só enquanto o totem existe) — generaliza a checagem que já existia
    // manualmente dentro de HordeTotemSpawner.SpawnRandom() (hordeSpawner.IsActive).
    public override bool IsOccupied =>
        (hordeSpawner != null && hordeSpawner.IsActive) ||
        totemSpawners.Exists(s => s != null && s.HasActiveTotem);

    protected override void ExecuteActivation()
    {
        if (totemSpawners.Count == 0) return;

        var candidates = totemSpawners.FindAll(s => s != null && !s.HasActiveTotem);
        if (candidates.Count == 0) return;

        candidates[Random.Range(0, candidates.Count)].SpawnRandom();
    }

    protected override void DespawnActive()
    {
        foreach (var spawner in totemSpawners)
            spawner?.DespawnActive();
    }
}
