using System.Collections.Generic;
using DG.Tweening;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class SellUI : MonoBehaviour
{
    [Header("Tela")]
    public GameObject root;
    public Image dimBackground;
    public CanvasGroup panel;

    [Header("Lista")]
    public Transform rowsContainer;
    public ShopRowUI rowPrefab;
    public ScrollRect scrollRect;
    public float rowEnterStagger = 0.05f;

    [Header("Detalhe")]
    public ShopItemDetailUI detail;

    [Header("Voltar")]
    public BackButtonUI backButton;

    [Header("Painéis laterais")]
    public InventoryUI inventoryUI;
    public StatsUI statsUI;

    SellManager _sellManager;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly List<ShopRowUI> _rows = new();
    ShopRowUI _focused;
    ItemDefinition _focusedItem;
    bool _selling;
    bool _listDirty;
    float _dimAlpha;
    Sequence _closeSequence;

    void Awake()
    {
        if (dimBackground != null) _dimAlpha = dimBackground.color.a;

        if (rowsContainer != null)
            for (int i = rowsContainer.childCount - 1; i >= 0; i--)
                Destroy(rowsContainer.GetChild(i).gameObject);
    }

    public void Open(PlayerStatsAggregator stats, PlayerItemHandler itemHandler, SellManager sellManager, System.Action onBack)
    {
        if (_itemHandler != null)
            _itemHandler.OnItemsChanged -= HandleItemsChanged;

        _stats = stats;
        _itemHandler = itemHandler;
        _sellManager = sellManager;
        _selling = false;
        _listDirty = false;
        _focusedItem = null;

        if (_itemHandler != null)
            _itemHandler.OnItemsChanged += HandleItemsChanged;

        _closeSequence?.Kill();
        _closeSequence = null;

        root.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayOpen();

        if (backButton != null)
        {
            backButton.SetAction(onBack);
            backButton.Interactable = true;
        }

        if (inventoryUI != null) inventoryUI.gameObject.SetActive(true);
        if (statsUI != null) statsUI.Bind(_stats);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;

        RenderItems();
    }

    public void Close()
    {
        if (_itemHandler != null)
            _itemHandler.OnItemsChanged -= HandleItemsChanged;

        _itemHandler = null;

        if (backButton != null) backButton.Interactable = false;

        if (panel == null)
        {
            FinishClose();
            return;
        }

        _closeSequence?.Kill();
        var sequence = _closeSequence = DOTween.Sequence()
            .Join(panel.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InBack))
            .Join(panel.DOFade(0f, 0.2f));

        if (dimBackground != null) sequence.Join(dimBackground.DOFade(0f, 0.2f));

        sequence.AsUI(gameObject).OnComplete(FinishClose);
    }

    void FinishClose()
    {
        _closeSequence = null;
        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void PlayOpen()
    {
        if (dimBackground != null)
        {
            dimBackground.DOKill();
            dimBackground.color = WithAlpha(dimBackground.color, 0f);
            dimBackground.DOFade(_dimAlpha, 0.15f).AsUI(dimBackground.gameObject);
        }

        if (panel != null)
        {
            panel.DOKill();
            panel.transform.DOKill();
            panel.alpha = 0f;
            panel.transform.localScale = Vector3.one * 0.85f;
            panel.DOFade(1f, 0.15f).AsUI(panel.gameObject);
            panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsUI(panel.gameObject);
        }
    }

    void HandleItemsChanged()
    {
        if (_selling)
        {
            _listDirty = true;
            return;
        }

        RenderItems();
    }

    void RenderItems()
    {
        var owned = _itemHandler != null
            ? _itemHandler.AcquiredItems
            : (IReadOnlyList<ItemDefinition>)System.Array.Empty<ItemDefinition>();

        while (_rows.Count < owned.Count)
            _rows.Add(Instantiate(rowPrefab, rowsContainer));

        ShopRowUI focusTarget = null;

        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            if (i >= owned.Count)
            {
                row.gameObject.SetActive(false);
                continue;
            }

            row.Setup(owned[i], SellPrice(owned[i]), true, FocusRow, SellRow);
            row.PlayEnter(i * rowEnterStagger);

            if (owned[i] == _focusedItem) focusTarget = row;
        }

        if (focusTarget == null && owned.Count > 0) focusTarget = _rows[0];

        _focused = null;
        SetFocus(focusTarget, false);
    }

    void FocusRow(ShopRowUI row)
    {
        if (_selling || row == _focused) return;
        SetFocus(row, true);
    }

    void SetFocus(ShopRowUI row, bool animate)
    {
        if (_focused != null) _focused.SetFocused(false);

        _focused = row;
        _focusedItem = row != null ? row.Item : null;

        if (_focused != null) _focused.SetFocused(true);

        if (detail != null)
            detail.Show(_focusedItem, SellPrice(_focusedItem), _stats != null ? _stats.Coins : 0, true, animate);
    }

    void SellRow(ShopRowUI row)
    {
        if (_selling || row == null || row.Item == null || _sellManager == null) return;

        var item = row.Item;
        int price = SellPrice(item);

        if (row != _focused) SetFocus(row, false);

        _selling = true;
        _listDirty = false;

        if (!_sellManager.TrySell(item, _stats, _itemHandler))
        {
            _selling = false;
            return;
        }

        if (detail != null)
        {
            detail.SetWallet(_stats.Coins, true);
            detail.PlayWalletDelta(price);
        }

        _focusedItem = null;

        row.PlayConsumed(() =>
        {
            _selling = false;
            row.gameObject.SetActive(false);
            if (_listDirty) RenderItems();
        });
    }

    int SellPrice(ItemDefinition item) => _sellManager != null ? _sellManager.GetSellPrice(item) : 0;

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
