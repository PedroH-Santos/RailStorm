using StarterAssets;
using UnityEngine;

public class ChestInteractable : InteractableObject
{
    [Header("Loot")]
    [SerializeField] private ChestLootTable lootTable;

    [Header("Partículas (opcional, tocadas ao abrir)")]
    [SerializeField] private ParticleSystem openBurstParticles;

    public static event System.Action<ItemDefinition, int> OnChestOpened;

    PlayerController _player;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;
    ItemDefinition _pendingItem;
    int _pendingRarityIndex;

    public void SetLootTable(ChestLootTable table) => lootTable = table;

    protected override void OnInteract() => Open();

    void Open()
    {
        if (Consumed) return;

        if (lootTable == null)
        {
            Debug.LogWarning($"[Chest] '{name}' não tem uma Loot Table atribuída (nem no prefab, nem via SetLootTable pelo ChestSpawner).");
            return;
        }

        if (_stats == null || _itemHandler == null)
        {
            Debug.LogWarning($"[Chest] '{name}' não encontrou PlayerStatsAggregator/PlayerItemHandler no objeto com tag 'Player' " +
                              $"(_stats={_stats != null}, _itemHandler={_itemHandler != null}). Confirme que esses componentes " +
                              "estão no MESMO GameObject que tem a tag 'Player' e a collider que entrou no trigger do baú.");
            return;
        }

        int rarityIndex = RarityRoller.Roll(lootTable.minRarity, lootTable.ResolvedMaxRarity, _stats.LuckPercent);
        ItemDefinition item = ChestLootRoller.PickItem(lootTable.possibleItems, rarityIndex,
            i => _itemHandler.IsExiled(i) || _itemHandler.HasItem(i));

        if (item == null)
        {
            Debug.LogWarning($"[Chest] '{name}' não tem itens disponíveis (todos já possuídos, exilados ou loot table vazia).");
            return;
        }

        var reveal = ChestRevealEffect.Instance;
        if (reveal == null)
        {
            Debug.LogError($"[Chest] '{name}' não encontrou nenhum ChestRevealEffect na cena (nem ativo, nem inativo). " +
                            "Adicione o componente em algum objeto da cena antes de abrir o baú.");
            return;
        }

        _pendingItem = item;
        _pendingRarityIndex = item.rarity;

        SuppressInteraction();
        _player?.SetMovementLocked(true);
        Time.timeScale = 0f;

        if (openBurstParticles != null) openBurstParticles.Play();

        reveal.Show(item, _pendingRarityIndex, transform.position, OnTake, OnExile, OnSkip);
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

    void OnSkip() => FinishDecision();

    void FinishDecision()
    {
        Time.timeScale = 1f;
        _player?.SetMovementLocked(false);
        FinishLifecycle();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (Consumed || !other.CompareTag("Player")) return;

        _player = other.GetComponent<PlayerController>();
        _stats = other.GetComponent<PlayerStatsAggregator>();
        _itemHandler = other.GetComponent<PlayerItemHandler>();
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (!other.CompareTag("Player") || Consumed) return;

        _player = null;
        _stats = null;
        _itemHandler = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.15f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
