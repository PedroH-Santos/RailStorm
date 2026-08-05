using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("Baú")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private ChestLootTable lootTable;

    [Header("Marcadores (posicionados manualmente perto do trilho)")]
    [SerializeField] private List<Transform> spawnMarkers = new();

    ChestInteractable _active;

    public bool HasActiveChest => _active != null;

    public void SpawnRandom()
    {
        if (_active != null || chestPrefab == null || spawnMarkers.Count == 0) return;

        Transform marker = spawnMarkers[Random.Range(0, spawnMarkers.Count)];
        SpawnAt(marker);
    }

    public void SpawnAt(Transform marker)
    {
        if (_active != null || chestPrefab == null || marker == null) return;

        GameObject chest = Instantiate(chestPrefab, marker.position, marker.rotation);

        if (!chest.TryGetComponent<ChestInteractable>(out var interactable))
            interactable = chest.AddComponent<ChestInteractable>();

        interactable.SetLootTable(lootTable);
        interactable.OnOpenedOrDespawned += HandleActiveCleared;
        _active = interactable;
    }

    public void DespawnActive()
    {
        if (_active == null) return;
        _active.Despawn();
    }

    void HandleActiveCleared()
    {
        _active = null;
    }
}
