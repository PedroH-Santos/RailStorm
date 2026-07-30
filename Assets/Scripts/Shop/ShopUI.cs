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
    public ShopManager shopManager;
    public List<ShopSlotUI> slots;
    public TMP_Text coinsText;
    public TMP_Text timerText;
    public TMP_Text totalText;
    public Button confirmBuyButton;

    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly HashSet<ItemDefinition> _selected = new();

    void Awake()
    {
        if (confirmBuyButton != null)
            confirmBuyButton.onClick.AddListener(ConfirmPurchase);
    }

    void OnEnable()
    {
        if (shopManager != null) shopManager.OnStockChanged += RenderStock;
    }

    void OnDisable()
    {
        if (shopManager != null) shopManager.OnStockChanged -= RenderStock;
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        UpdateTimerText();
    }

    public void Open(PlayerStatsAggregator stats, PlayerItemHandler itemHandler)
    {
        _stats = stats;
        _itemHandler = itemHandler;

        root.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RenderStock();
        UpdateCoinsText();
    }

    public void Close()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void RenderStock()
    {
        _selected.Clear();

        var stock = shopManager.CurrentStock;

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
        if (!shopManager.TryBuyMultiple(_selected, _stats, _itemHandler)) return;

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
        if (timerText == null || shopManager == null) return;

        float remaining = shopManager.TimeUntilRefresh;
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}