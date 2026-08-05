using System.Collections.Generic;
using UnityEngine;

public class HordeTotemSpawner : MonoBehaviour
{
    [Header("Totem")]
    [SerializeField] private GameObject totemPrefab;
    [SerializeField] private HordeSpawner hordeSpawner;

    [Header("Marcadores (posicionados manualmente perto do trilho)")]
    [SerializeField] private List<Transform> spawnMarkers = new();

    HordeTotemInteractable _active;

    public bool HasActiveTotem => _active != null;

    public void SpawnRandom()
    {
        if (_active != null || totemPrefab == null || spawnMarkers.Count == 0 || hordeSpawner == null) return;
        if (hordeSpawner.IsActive) return;

        Transform marker = spawnMarkers[Random.Range(0, spawnMarkers.Count)];
        SpawnAt(marker);
    }

    public void SpawnAt(Transform marker)
    {
        if (_active != null || totemPrefab == null || marker == null || hordeSpawner == null) return;

        GameObject totem = Instantiate(totemPrefab, marker.position, marker.rotation);

        if (!totem.TryGetComponent<HordeTotemInteractable>(out var interactable))
            interactable = totem.AddComponent<HordeTotemInteractable>();

        interactable.SetHordeSpawner(hordeSpawner);
        interactable.OnAcceptedOrDespawned += HandleActiveCleared;
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
