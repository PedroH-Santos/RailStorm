using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public List<ShopSlotUI> slots;
    public TMP_Text coinsText;
    public TMP_Text timerText;
    public TMP_Text totalText;
    public Button confirmBuyButton;

    ShopManager _shopManager;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly HashSet<ItemDefinition> _selected = new();

    void Awake()
    {
        if (confirmBuyButton != null)
            confirmBuyButton.onClick.AddListener(ConfirmPurchase);
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        UpdateTimerText();
    }

    public void Open(PlayerStatsAggregator stats, PlayerItemHandler itemHandler, ShopManager shopManager)
    {
        if (_shopManager != null)
            _shopManager.OnStockChanged -= RenderStock;

        _shopManager = shopManager;
        _stats = stats;
        _itemHandler = itemHandler;

        if (_shopManager != null)
            _shopManager.OnStockChanged += RenderStock;

        root.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RenderStock();
        UpdateCoinsText();
    }

    public void Close()
    {
        if (_shopManager != null)
            _shopManager.OnStockChanged -= RenderStock;

        _shopManager = null;

        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void RenderStock()
    {
        _selected.Clear();

        var stock = _shopManager != null
            ? _shopManager.CurrentStock
            : (IReadOnlyList<ItemDefinition>)System.Array.Empty<ItemDefinition>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < stock.Count)
                slots[i].Setup(stock[i], _stats, _itemHandler, OnSlotToggled);
            else
                slots[i].Clear();
        }

        UpdateCartState();
    }

    void OnSlotToggled(ItemDefinition item)
    {
        var slot = slots.FirstOrDefault(s => s.Item == item);
        if (slot == null) return;

        bool nowSelected = !slot.IsSelected;
        slot.SetSelected(nowSelected);

        if (nowSelected) _selected.Add(item);
        else _selected.Remove(item);

        UpdateCartState();
    }

    void ConfirmPurchase()
    {
        if (_shopManager == null) return;
        if (!_shopManager.TryBuyMultiple(_selected, _stats, _itemHandler)) return;

        UpdateCoinsText();
        RenderStock(); 
    }

    void UpdateCartState()
    {
        int total = _selected.Sum(i => i.price);

        if (totalText != null)
            totalText.text = total.ToString();

        if (confirmBuyButton != null)
            confirmBuyButton.interactable = _selected.Count > 0 && _stats != null && _stats.Coins >= total;
    }

    void UpdateCoinsText()
    {
        if (coinsText != null && _stats != null)
            coinsText.text = $"{_stats.Coins}";
    }

    void UpdateTimerText()
    {
        if (timerText == null || _shopManager == null) return;

        float remaining = _shopManager.TimeUntilRefresh;
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}