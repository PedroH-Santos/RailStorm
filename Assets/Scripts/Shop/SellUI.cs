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
    public List<SellInventorySlotUI> inventorySlots;
    public List<SellCartItemUI> cartSlots;
    public TMP_Text coinsText;
    public TMP_Text totalText;
    public Button confirmSellButton;

    SellManager _sellManager;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly List<ItemDefinition> _selected = new();

    void Awake()
    {
        if (confirmSellButton != null)
            confirmSellButton.onClick.AddListener(ConfirmSale);
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
        var owned = _itemHandler != null
            ? _itemHandler.AcquiredItems
            : (IReadOnlyList<ItemDefinition>)System.Array.Empty<ItemDefinition>();

        _selected.RemoveAll(i => !owned.Contains(i));

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < owned.Count)
            {
                var item = owned[i];
                inventorySlots[i].Setup(item, _selected.Contains(item), OnInventorySlotToggled);
            }
            else
            {
                inventorySlots[i].Clear();
            }
        }

        RenderCart();
    }

    void OnInventorySlotToggled(ItemDefinition item)
    {
        if (_selected.Contains(item))
            _selected.Remove(item);
        else
            _selected.Add(item);

        RenderInventory();
    }

    void RenderCart()
    {
        for (int i = 0; i < cartSlots.Count; i++)
        {
            if (i < _selected.Count)
            {
                var item = _selected[i];
                int price = _sellManager != null ? _sellManager.GetSellPrice(item) : 0;
                cartSlots[i].Setup(item, price, () => RemoveFromCart(item));
            }
            else
            {
                cartSlots[i].Clear();
            }
        }

        UpdateCartState();
    }

    void RemoveFromCart(ItemDefinition item)
    {
        _selected.Remove(item);
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
}
