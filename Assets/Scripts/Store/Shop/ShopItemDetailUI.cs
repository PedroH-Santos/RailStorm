using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemDetailUI : MonoBehaviour
{
    [Header("Card")]
    public RectTransform card;
    public Image cardFill;
    public Image cardBorder;

    [Range(0f, 1f)]
    public float cardFillRarityBlend = 0.7f;

    [Range(0f, 1f)]
    public float cardFillDarkness = 0.3f;

    [Header("Cabeçalho")]
    public Image iconImage;
    public Image iconPlate;
    public Image iconGlow;
    public TMP_Text nameText;
    public TMP_Text rarityText;
    public TMP_Text descriptionText;
    public GameObject descriptionBox;

    [Header("Efeitos")]
    public GameObject statsHeader;
    public Transform statsContainer;
    public TooltipStatRowUI statRowTemplate;
    public GameObject abilitySection;
    public TMP_Text abilityNameText;
    public TMP_Text abilityDescriptionText;

    [Header("Custo / Carteira")]
    public TMP_Text costText;
    public TMP_Text walletText;
    public TMP_Text costBurnText;
    public float costBurnRise = 60f;

    [Header("Vazio")]
    public GameObject contentRoot;
    public GameObject emptyState;

    readonly List<TooltipStatRowUI> _rows = new();
    Color _costColor;
    Vector2 _costBurnRest;

    void Awake()
    {
        if (costText != null) _costColor = costText.color;
        if (statRowTemplate != null) statRowTemplate.gameObject.SetActive(false);
        if (costBurnText != null)
        {
            _costBurnRest = costBurnText.rectTransform.anchoredPosition;
            costBurnText.gameObject.SetActive(false);
        }
    }

    public void Show(ItemDefinition item, int price, int coins, bool affordable, bool animate)
    {
        bool hasItem = item != null;
        if (contentRoot != null) contentRoot.SetActive(hasItem);
        if (emptyState != null) emptyState.SetActive(!hasItem);

        if (!hasItem)
        {
            SetWallet(coins, true);
            return;
        }

        var data = TooltipBuilder.Build(item, item.rarity);
        Color rarityColor = data.RarityColor;

        if (nameText != null) nameText.text = data.Title;
        if (rarityText != null)
        {
            rarityText.text = data.RarityLabel;
            rarityText.color = rarityColor;
        }

        if (descriptionText != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(data.Description);
            descriptionText.gameObject.SetActive(hasDescription);
            if (descriptionBox != null) descriptionBox.SetActive(hasDescription);
            descriptionText.text = data.Description;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = data.Icon != null;
        }

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(data.RarityIndex);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = rarityColor;
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(data.RarityIndex);
            iconGlow.enabled = glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(data.RarityIndex);
        }

        if (cardBorder != null) cardBorder.color = rarityColor;

        var theme = UIThemeConfig.Instance;
        if (cardFill != null && theme != null)
            cardFill.color = Color.Lerp(theme.panelBackground, Shade(rarityColor, cardFillDarkness), cardFillRarityBlend);

        BuildRows(data.Stats);

        if (abilitySection != null) abilitySection.SetActive(data.HasAbility);
        if (data.HasAbility)
        {
            if (abilityNameText != null)
            {
                bool hasName = !string.IsNullOrWhiteSpace(data.AbilityName);
                abilityNameText.gameObject.SetActive(hasName);
                abilityNameText.text = data.AbilityName;
            }

            if (abilityDescriptionText != null) abilityDescriptionText.text = data.AbilityDescription;
        }

        if (costText != null) costText.text = price.ToString();
        SetWallet(coins, affordable);

        if (animate && card != null)
        {
            card.DOKill(true);
            card.localScale = Vector3.one;
            card.DOPunchScale(Vector3.one * 0.03f, 0.22f, 6, 0.5f).AsUI(card.gameObject);
        }
    }

    public void SetWallet(int coins, bool affordable)
    {
        if (walletText != null) walletText.text = coins.ToString();

        if (costText != null)
        {
            var theme = UIThemeConfig.Instance;
            costText.color = affordable || theme == null ? _costColor : theme.actionDestructive;
        }
    }

    public void PlayWalletDelta(int delta)
    {
        if (costBurnText == null) return;

        var rect = costBurnText.rectTransform;
        rect.DOKill();
        costBurnText.DOKill();

        costBurnText.gameObject.SetActive(true);
        costBurnText.text = delta >= 0 ? $"+{delta}" : $"{delta}";
        costBurnText.alpha = 1f;
        rect.anchoredPosition = _costBurnRest;
        rect.localScale = Vector3.one * 1.4f;

        rect.DOScale(1f, 0.2f).SetEase(Ease.OutBack).AsUI(costBurnText.gameObject);
        rect.DOAnchorPosY(_costBurnRest.y + costBurnRise, 0.7f).SetEase(Ease.OutCubic).AsUI(costBurnText.gameObject);
        costBurnText.DOFade(0f, 0.35f).SetDelay(0.35f).AsUI(costBurnText.gameObject)
            .OnComplete(() => costBurnText.gameObject.SetActive(false));

        if (walletText != null)
        {
            walletText.transform.DOKill(true);
            walletText.transform.localScale = Vector3.one;
            walletText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6, 0.5f).AsUI(walletText.gameObject);
        }
    }

    void BuildRows(List<TooltipStatLine> lines)
    {
        if (statsHeader != null) statsHeader.SetActive(lines.Count > 0);
        if (statsContainer == null || statRowTemplate == null) return;

        while (_rows.Count < lines.Count)
            _rows.Add(Instantiate(statRowTemplate, statsContainer));

        for (int i = 0; i < _rows.Count; i++)
        {
            bool used = i < lines.Count;
            _rows[i].gameObject.SetActive(used);
            if (used) _rows[i].Setup(lines[i]);
        }
    }

    static Color Shade(Color color, float factor) => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}
