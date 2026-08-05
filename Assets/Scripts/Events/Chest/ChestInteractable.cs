using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class ChestInteractable : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private ChestLootTable lootTable;

    [Header("Interação")]
    [SerializeField] private float interactRadius = 2f;

    [Header("Partículas (opcional, tocadas ao abrir)")]
    [SerializeField] private ParticleSystem openBurstParticles;

    public static event System.Action<ItemDefinition, int> OnChestOpened;
    public event System.Action OnOpenedOrDespawned;

    PlayerController _player;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;
    ItemDefinition _pendingItem;
    int _pendingRarityIndex;
    bool _playerInRange;
    bool _opened;

    public void SetLootTable(ChestLootTable table) => lootTable = table;

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = interactRadius;
        col.isTrigger = true;
    }

    void Update()
    {
        if (_opened || !_playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Open();
    }

    void Open()
    {
        if (_opened || lootTable == null || _stats == null || _itemHandler == null) return;

        int rarityIndex = RarityRoller.Roll(lootTable.minRarity, lootTable.ResolvedMaxRarity, _stats.LuckPercent);
        ItemDefinition item = ChestLootRoller.PickItem(lootTable.possibleItems, rarityIndex, _itemHandler.IsExiled);

        if (item == null)
        {
            Debug.LogWarning($"[Chest] '{name}' não tem itens disponíveis (todos exilados ou loot table vazia).");
            return;
        }

        _opened = true;
        _pendingItem = item;
        _pendingRarityIndex = rarityIndex;

        InteractPromptUI.Instance?.Hide();
        _player?.SetMovementLocked(true);
        Time.timeScale = 0f;

        if (openBurstParticles != null) openBurstParticles.Play();

        ChestRevealEffect.Instance?.Show(item, rarityIndex, transform.position, OnTake, OnExile, OnSkip);
    }

    void OnTake()
    {
        _itemHandler.AcquireItem(_pendingItem);
        OnChestOpened?.Invoke(_pendingItem, _pendingRarityIndex);
        FinishDecision();
    }

    void OnExile()
    {
        _itemHandler.ExileItem(_pendingItem);
        FinishDecision();
    }

    void OnSkip()
    {
        FinishDecision();
    }

    void FinishDecision()
    {
        Time.timeScale = 1f;
        _player?.SetMovementLocked(false);
        OnOpenedOrDespawned?.Invoke();
    }

    public void Despawn()
    {
        if (_opened) return;
        InteractPromptUI.Instance?.Hide();
        OnOpenedOrDespawned?.Invoke();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_opened || !other.CompareTag("Player")) return;

        _player = other.GetComponent<PlayerController>();
        _stats = other.GetComponent<PlayerStatsAggregator>();
        _itemHandler = other.GetComponent<PlayerItemHandler>();
        _playerInRange = true;

        InteractPromptUI.Instance?.Show();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (!_opened)
        {
            InteractPromptUI.Instance?.Hide();
            _player = null;
            _stats = null;
            _itemHandler = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.15f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
