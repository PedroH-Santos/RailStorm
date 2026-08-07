using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("Baú")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private ChestLootTable lootTable;
    [SerializeField] private PlayerItemHandler playerItemHandler;

    [Header("Marcadores (posicionados manualmente perto do trilho)")]
    [Tooltip("A rotação de cada marcador é usada como a rotação do baú ao nascer — gire o " +
             "marcador na Scene View até ele 'olhar' para o trilho e o baú vai nascer olhando igual.")]
    [SerializeField] private List<Transform> spawnMarkers = new();

    ChestInteractable _active;
    Transform _lastMarker;

    public bool HasActiveChest => _active != null;

    public bool HasAvailableLoot =>
        lootTable != null && playerItemHandler != null &&
        lootTable.possibleItems.Exists(IsItemAvailable);

    bool IsItemAvailable(ItemDefinition item) =>
        item != null && !playerItemHandler.IsExiled(item) && !playerItemHandler.HasItem(item);

    void OnEnable()
    {
        if (playerItemHandler != null)
            playerItemHandler.OnItemsChanged += HandlePlayerItemsChanged;
    }

    void OnDisable()
    {
        if (playerItemHandler != null)
            playerItemHandler.OnItemsChanged -= HandlePlayerItemsChanged;
    }

    void HandlePlayerItemsChanged()
    {
        if (!HasAvailableLoot)
            DespawnActive();
    }

    public void SpawnRandom()
    {
        if (_active != null || chestPrefab == null || !HasAvailableLoot) return;

        var validMarkers = spawnMarkers.FindAll(m => m != null);
        if (validMarkers.Count == 0)
        {
            Debug.LogWarning($"[ChestSpawner] '{name}' não tem nenhum Spawn Marker atribuído.");
            return;
        }

        var candidates = validMarkers.Count > 1
            ? validMarkers.FindAll(m => m != _lastMarker)
            : validMarkers;

        Transform marker = candidates[Random.Range(0, candidates.Count)];
        _lastMarker = marker;
        SpawnAt(marker);
    }

    void SpawnAt(Transform marker)
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
