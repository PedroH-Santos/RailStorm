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
    Transform _lastMarker;

    public bool HasActiveChest => _active != null;

    public void SpawnRandom()
    {
        if (_active != null || chestPrefab == null) return;

        // Ignora slots vazios (não configurados no Inspector) para não desperdiçar o sorteio.
        var validMarkers = spawnMarkers.FindAll(m => m != null);
        if (validMarkers.Count == 0)
        {
            Debug.LogWarning($"[ChestSpawner] '{name}' não tem nenhum Spawn Marker atribuído.");
            return;
        }

        // Evita repetir o mesmo marcador duas vezes seguidas, quando há mais de uma opção.
        var candidates = validMarkers.Count > 1
            ? validMarkers.FindAll(m => m != _lastMarker)
            : validMarkers;

        Transform marker = candidates[Random.Range(0, candidates.Count)];
        _lastMarker = marker;
        SpawnAt(marker);
    }

    public void SpawnAt(Transform marker)
    {
        if (_active != null || chestPrefab == null || marker == null) return;

        GameObject chest = Instantiate(chestPrefab, marker.position, marker.rotation);

        if (!chest.TryGetComponent<ChestInteractable>(out var interactable))
            interactable = chest.AddComponent<ChestInteractable>();

        interactable.SetLootTable(lootTable);
        interactable.OnConsumedOrDespawned += HandleActiveCleared;
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
