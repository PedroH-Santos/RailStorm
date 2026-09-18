using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
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

    [Header("Renovação do estoque")]
    public TMP_Text timerText;
    public RectTransform timerPulseTarget;
    [Tooltip("Abaixo desse tempo (segundos) o cronômetro fica vermelho e pulsa.")]
    public float urgentSeconds = 10f;

    [Header("Voltar")]
    public BackButtonUI backButton;

    [Header("Painéis laterais")]
    public InventoryUI inventoryUI;
    public StatsUI statsUI;

    ShopManager _shopManager;
    PlayerStatsAggregator _stats;
    PlayerItemHandler _itemHandler;

    readonly List<ShopRowUI> _rows = new();
    ShopRowUI _focused;
    ItemDefinition _focusedItem;
    bool _buying;
    bool _stockDirty;
    bool _urgent;
    float _dimAlpha;
    Color _timerColor;
    Tween _timerPulse;
    Sequence _closeSequence;

    void Awake()
    {
        if (dimBackground != null) _dimAlpha = dimBackground.color.a;
        if (timerText != null) _timerColor = timerText.color;

        if (rowsContainer != null)
            for (int i = rowsContainer.childCount - 1; i >= 0; i--)
                Destroy(rowsContainer.GetChild(i).gameObject);
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        UpdateTimer();
    }

    public void Open(PlayerStatsAggregator stats, PlayerItemHandler itemHandler, ShopManager shopManager, System.Action onBack)
    {
        if (_shopManager != null)
            _shopManager.OnStockChanged -= HandleStockChanged;

        _shopManager = shopManager;
        _stats = stats;
        _itemHandler = itemHandler;
        _buying = false;
        _stockDirty = false;
        _focusedItem = null;

        if (_shopManager != null)
            _shopManager.OnStockChanged += HandleStockChanged;

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

        _urgent = false;
        SetTimerUrgent(false);
        UpdateTimer();
        RenderStock();
    }

    public void Close()
    {
        if (_shopManager != null)
            _shopManager.OnStockChanged -= HandleStockChanged;

        _shopManager = null;

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
        _timerPulse?.Kill();
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

    void HandleStockChanged()
    {
        if (_buying)
        {
            _stockDirty = true;
            return;
        }

        RenderStock();
    }

    void RenderStock()
    {
        var stock = _shopManager != null
            ? _shopManager.CurrentStock
            : (IReadOnlyList<ItemDefinition>)System.Array.Empty<ItemDefinition>();

        while (_rows.Count < stock.Count)
            _rows.Add(Instantiate(rowPrefab, rowsContainer));

        ShopRowUI focusTarget = null;
        int coins = _stats != null ? _stats.Coins : 0;

        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            if (i >= stock.Count)
            {
                row.gameObject.SetActive(false);
                continue;
            }

            row.Setup(stock[i], stock[i].price, coins >= stock[i].price, FocusRow, BuyRow);
            row.PlayEnter(i * rowEnterStagger);

            if (stock[i] == _focusedItem) focusTarget = row;
        }

        if (focusTarget == null && stock.Count > 0) focusTarget = _rows[0];

        _focused = null;
        SetFocus(focusTarget, false);
    }

    void FocusRow(ShopRowUI row)
    {
        if (_buying || row == _focused) return;
        SetFocus(row, true);
    }

    void SetFocus(ShopRowUI row, bool animate)
    {
        if (_focused != null) _focused.SetFocused(false);

        _focused = row;
        _focusedItem = row != null ? row.Item : null;

        if (_focused != null) _focused.SetFocused(true);

        if (detail == null) return;

        int coins = _stats != null ? _stats.Coins : 0;
        int price = _focusedItem != null ? _focusedItem.price : 0;
        detail.Show(_focusedItem, price, coins, coins >= price, animate);
    }

    void BuyRow(ShopRowUI row)
    {
        if (_buying || row == null || row.Item == null || _shopManager == null) return;

        var item = row.Item;
        if (!_shopManager.CanAfford(item, _stats)) return;

        if (row != _focused) SetFocus(row, false);

        _buying = true;
        _stockDirty = false;

        if (!_shopManager.TryBuy(item, _stats, _itemHandler))
        {
            _buying = false;
            return;
        }

        foreach (var other in _rows)
            if (other != row && other.gameObject.activeSelf && other.Item != null)
                other.SetAffordable(_stats.Coins >= other.Item.price);

        if (detail != null)
        {
            detail.SetWallet(_stats.Coins, true);
            detail.PlayWalletDelta(-item.price);
        }

        _focusedItem = null;

        row.PlayConsumed(() =>
        {
            _buying = false;
            row.gameObject.SetActive(false);
            if (_stockDirty) RenderStock();
        });
    }

    void UpdateTimer()
    {
        if (timerText == null || _shopManager == null) return;

        float remaining = _shopManager.TimeUntilRefresh;
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        bool urgent = remaining <= urgentSeconds;
        if (urgent != _urgent)
        {
            _urgent = urgent;
            SetTimerUrgent(urgent);
        }
    }

    void SetTimerUrgent(bool urgent)
    {
        var theme = UIThemeConfig.Instance;
        if (timerText != null)
            timerText.color = urgent && theme != null ? theme.actionDestructive : _timerColor;

        var target = timerPulseTarget != null ? timerPulseTarget : (timerText != null ? timerText.rectTransform : null);
        if (target == null) return;

        _timerPulse?.Kill();
        target.localScale = Vector3.one;

        if (urgent)
            _timerPulse = target.DOScale(1.12f, 0.35f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).AsUI(target.gameObject);
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
