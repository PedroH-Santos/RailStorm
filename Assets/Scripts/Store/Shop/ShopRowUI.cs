using System;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopRowUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Conteúdo")]
    public Image iconImage;
    public Image iconPlate;
    public Image iconGlow;
    public TMP_Text nameText;
    public TMP_Text rarityText;
    public TMP_Text priceText;

    [Header("Raridade")]
    public Image cardFill;
    public Image cardBorder;

    [Range(0f, 1f)]
    [Tooltip("Quanto da cor de raridade escurecida entra no fundo da fileira.")]
    public float cardFillRarityBlend = 0.7f;

    [Range(0f, 1f)]
    [Tooltip("Fator que escurece a cor de raridade antes de virar fundo da fileira.")]
    public float cardFillDarkness = 0.3f;

    [Header("Interação")]
    public Button buyButton;
    public GameObject selectionBrackets;
    public CanvasGroup group;

    [Header("Animação")]
    public float enterFromScale = 0.6f;
    public float enterDuration = 0.32f;
    public float boughtDuration = 0.3f;

    public ItemDefinition Item { get; private set; }

    Action<ShopRowUI> _onFocus;
    Action<ShopRowUI> _onBuy;
    Color _priceColor;
    bool _priceColorCaptured;

    void Awake()
    {
        CapturePriceColor();
        if (buyButton != null) buyButton.onClick.AddListener(HandleBuyClicked);
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (group != null) group.alpha = 1f;
    }

    public void Setup(ItemDefinition item, bool affordable, Action<ShopRowUI> onFocus, Action<ShopRowUI> onBuy)
    {
        Item = item;
        _onFocus = onFocus;
        _onBuy = onBuy;

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = item.price.ToString();

        Color rarityColor = RarityHelper.Color(item.rarity);

        if (rarityText != null)
        {
            rarityText.text = RarityHelper.DisplayName(item.rarity);
            rarityText.color = rarityColor;
        }

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(item.rarity);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = rarityColor;
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(item.rarity);
            iconGlow.enabled = glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(item.rarity);
        }

        if (cardBorder != null) cardBorder.color = rarityColor;

        var theme = UIThemeConfig.Instance;
        if (cardFill != null && theme != null)
            cardFill.color = Color.Lerp(theme.panelBackground, Shade(rarityColor, cardFillDarkness), cardFillRarityBlend);

        SetAffordable(affordable);
        SetFocused(false);
    }

    public void SetAffordable(bool affordable)
    {
        CapturePriceColor();

        if (buyButton != null) buyButton.interactable = affordable;

        if (priceText != null)
        {
            var theme = UIThemeConfig.Instance;
            priceText.color = affordable || theme == null ? _priceColor : theme.actionDestructive;
        }
    }

    public void SetFocused(bool focused)
    {
        if (selectionBrackets != null) selectionBrackets.SetActive(focused);
    }

    public void PlayEnter(float delay)
    {
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * enterFromScale;
        if (group != null)
        {
            group.alpha = 0f;
            group.DOFade(1f, enterDuration * 0.6f).SetDelay(delay).AsUI(gameObject);
        }

        transform.DOScale(1f, enterDuration).SetDelay(delay).SetEase(Ease.OutBack).AsUI(gameObject);
    }

    public void PlayBought(Action onDone)
    {
        transform.DOKill();
        group?.DOKill();

        var sequence = DOTween.Sequence()
            .Append(transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 6, 0.5f))
            .Append(transform.DOScale(0.2f, boughtDuration).SetEase(Ease.InBack))
            .Join(transform.DOLocalRotate(new Vector3(0f, 0f, -6f), boughtDuration));

        if (group != null) sequence.Join(group.DOFade(0f, boughtDuration));

        if (buyButton != null)
            buyButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.6f).AsUI(buyButton.gameObject);

        sequence.AsUI(gameObject).OnComplete(() => onDone?.Invoke());
    }

    public void OnPointerEnter(PointerEventData eventData) => _onFocus?.Invoke(this);

    public void OnPointerClick(PointerEventData eventData) => _onFocus?.Invoke(this);

    void HandleBuyClicked() => _onBuy?.Invoke(this);

    void CapturePriceColor()
    {
        if (_priceColorCaptured || priceText == null) return;
        _priceColor = priceText.color;
        _priceColorCaptured = true;
    }

    static Color Shade(Color color, float factor) => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}
