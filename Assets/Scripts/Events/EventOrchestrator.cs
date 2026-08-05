using System.Collections.Generic;
using UnityEngine;

public class EventOrchestrator : MonoBehaviour
{
    [Header("Baú")]
    [SerializeField] private EventTiming chestTiming = EventTiming.DuringWave;
    [Tooltip("Chance (0-1) do baú ser o evento sorteado quando elegível neste gatilho.")]
    [Range(0f, 1f)]
    [SerializeField] private float chestSpawnChance = 0.5f;
    [SerializeField] private List<ChestSpawner> chestSpawners = new();

    [Header("Horda")]
    [SerializeField] private EventTiming hordeTiming = EventTiming.AfterWave;
    [Tooltip("Chance (0-1) da horda ser o evento sorteado quando elegível neste gatilho.")]
    [Range(0f, 1f)]
    [SerializeField] private float hordeSpawnChance = 0.35f;
    [SerializeField] private List<HordeTotemSpawner> hordeTotemSpawners = new();

    void OnEnable()
    {
        EnemySpawner.OnWaveStarted += HandleWaveStarted;
        EnemySpawner.OnWaveCleared += HandleWaveCleared;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= HandleWaveStarted;
        EnemySpawner.OnWaveCleared -= HandleWaveCleared;
    }

    void HandleWaveStarted()
    {
        bool chestEligible = chestTiming == EventTiming.DuringWave;
        bool hordeEligible = hordeTiming == EventTiming.DuringWave;

        if (!chestEligible) DespawnAllChests();
        if (!hordeEligible) DespawnAllHordeTotems();

        RollAndSpawn(chestEligible, hordeEligible);
    }

    void HandleWaveCleared()
    {
        bool chestEligible = chestTiming == EventTiming.AfterWave;
        bool hordeEligible = hordeTiming == EventTiming.AfterWave;

        if (!chestEligible) DespawnAllChests();
        if (!hordeEligible) DespawnAllHordeTotems();

        RollAndSpawn(chestEligible, hordeEligible);
    }

    /// <summary>
    /// Sorteia, entre os tipos de evento elegíveis para o gatilho atual, qual (se algum) vai
    /// aparecer — nunca mais de um por gatilho. Cada "spawnChance" é o tamanho da fatia desse
    /// tipo na roleta; o restante (até 1) é a chance de nenhum evento aparecer.
    /// </summary>
    void RollAndSpawn(bool chestEligible, bool hordeEligible)
    {
        EventKind selected = RollEventKind(chestEligible, hordeEligible);

        switch (selected)
        {
            case EventKind.Chest:
                SpawnChest();
                break;
            case EventKind.Horde:
                SpawnHordeTotem();
                break;
        }
    }

    EventKind RollEventKind(bool chestEligible, bool hordeEligible)
    {
        float chestSlice = chestEligible ? chestSpawnChance : 0f;
        float hordeSlice = hordeEligible ? hordeSpawnChance : 0f;

        if (chestSlice <= 0f && hordeSlice <= 0f) return EventKind.None;

        float roll = Random.value;

        if (roll < chestSlice) return EventKind.Chest;
        if (roll < chestSlice + hordeSlice) return EventKind.Horde;
        return EventKind.None;
    }

    void SpawnChest()
    {
        if (chestSpawners.Count == 0) return;

        var candidates = chestSpawners.FindAll(s => s != null && !s.HasActiveChest);
        if (candidates.Count == 0) return;

        candidates[Random.Range(0, candidates.Count)].SpawnRandom();
    }

    void SpawnHordeTotem()
    {
        if (hordeTotemSpawners.Count == 0) return;

        var candidates = hordeTotemSpawners.FindAll(s => s != null && !s.HasActiveTotem);
        if (candidates.Count == 0) return;

        candidates[Random.Range(0, candidates.Count)].SpawnRandom();
    }

    void DespawnAllChests()
    {
        foreach (var spawner in chestSpawners)
            spawner?.DespawnActive();
    }

    void DespawnAllHordeTotems()
    {
        foreach (var spawner in hordeTotemSpawners)
            spawner?.DespawnActive();
    }
}
