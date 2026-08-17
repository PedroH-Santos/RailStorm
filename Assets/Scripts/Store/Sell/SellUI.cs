using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellUI : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public TMP_Text coinsText;
    public TMP_Text totalText;
    public Button confirmSellButton;

    [Header("Itens")]
    public Transform inventoryContainer;
    public SellInventorySlotUI inventorySlotPrefab;

    SellManager _sellManager;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly List<ItemDefinition> _selected = new();
    readonly List<SellInventorySlotUI> _inventorySlots = new();

    void Awake()
    {
        if (confirmSellButton != null)
            confirmSellButton.onClick.AddListener(ConfirmSale);

        ClearContainer(inventoryContainer);
    }

    static void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            DestroyImmediate(container.GetChild(i).gameObject);
    }

    public void Open(PlayerStatsAggregator stats, PlayerItemHandler itemHandler, SellManager sellManager)
    {
        if (_itemHandler != null)
            _itemHandler.OnItemsChanged -= RenderInventory;

        _stats = stats;
        _itemHandler = itemHandler;
        _sellManager = sellManager;

        if (_itemHandler != null)
            _itemHandler.OnItemsChanged += RenderInventory;

        _selected.Clear();

        root.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RenderInventory();
        UpdateCoinsText();
    }

    public void Close()
    {
        if (_itemHandler != null)
            _itemHandler.OnItemsChanged -= RenderInventory;

        _itemHandler = null;
        _selected.Clear();

        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void RenderInventory()
    {
        Debug.Log("Rendering inventory");

        var owned = _itemHandler != null
            ? _itemHandler.AcquiredItems
            : (IReadOnlyList<ItemDefinition>)System.Array.Empty<ItemDefinition>();

        _selected.RemoveAll(i => !owned.Contains(i));

        EnsureSlots(_inventorySlots, inventorySlotPrefab, inventoryContainer, owned.Count);

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            if (i < owned.Count)
            {
                var item = owned[i];
                int price = _sellManager != null ? _sellManager.GetSellPrice(item) : 0;
                _inventorySlots[i].Setup(item, price, _selected.Contains(item), OnInventorySlotToggled);
            }
            else
            {
                _inventorySlots[i].Clear();
            }
        }

        UpdateCartState();
    }

    void OnInventorySlotToggled(ItemDefinition item)
    {
        if (_selected.Contains(item))
            _selected.Remove(item);
        else
            _selected.Add(item);

        RenderInventory();
    }

    void ConfirmSale()
    {
        if (_sellManager == null) return;
        if (!_sellManager.TrySellMultiple(_selected, _stats, _itemHandler)) return;

        _selected.Clear();
        UpdateCoinsText();
        RenderInventory();
    }

    void UpdateCartState()
    {
        int total = _sellManager != null ? _selected.Sum(_sellManager.GetSellPrice) : 0;

        if (totalText != null)
            totalText.text = total.ToString();

        if (confirmSellButton != null)
            confirmSellButton.interactable = _selected.Count > 0;
    }

    void UpdateCoinsText()
    {
        if (coinsText != null && _stats != null)
            coinsText.text = $"{_stats.Coins}";
    }

    static void EnsureSlots<T>(List<T> slots, T prefab, Transform container, int needed) where T : Component
    {
        while (slots.Count < needed)
            slots.Add(Instantiate(prefab, container));
    }
}
