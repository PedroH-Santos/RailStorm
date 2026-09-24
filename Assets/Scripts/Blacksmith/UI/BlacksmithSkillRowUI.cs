using System;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlacksmithSkillRowUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Conteúdo")]
    public Image iconImage;
    public Image iconPlate;
    public Image iconGlow;
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text costText;
    public GameObject coinIcon;

    [Header("Placa")]
    public Image cardFill;
    public Image cardBorder;
    [Range(0f, 1f)] public float cardFillRarityBlend = 0.7f;
    [Range(0f, 1f)] public float cardFillDarkness = 0.3f;

    [Header("Interação")]
    public Button upgradeButton;
    public TMP_Text upgradeLabel;
    public string upgradeText = "MELHORAR";
    public string maxLevelText = "MÁXIMO";
    public string maxLevelCostText = "MÁX";
    public string equippedFormat = "Tecla {0}";
    public GameObject selectionBrackets;
    public CanvasGroup group;

    [Header("Animação")]
    public float enterFromScale = 0.6f;
    public float enterDuration = 0.32f;
    public float upgradePunch = 0.08f;

    public SkillDefinition Skill { get; private set; }

    Action<BlacksmithSkillRowUI> _onFocus;
    Action<BlacksmithSkillRowUI> _onUpgrade;
    Color _costColor;
    bool _costColorCaptured;

    void Awake()
    {
        CaptureCostColor();
        if (upgradeButton != null) upgradeButton.onClick.AddListener(() => _onUpgrade?.Invoke(this));
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        if (group != null) group.alpha = 1f;
    }

    public void Setup(SkillDefinition skill, int level, int cost, bool isMax, bool affordable, string equippedKey,
        Action<BlacksmithSkillRowUI> onFocus, Action<BlacksmithSkillRowUI> onUpgrade)
    {
        Skill = skill;
        _onFocus = onFocus;
        _onUpgrade = onUpgrade;
        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = skill.icon;
            iconImage.enabled = skill.icon != null;
        }

        if (nameText != null) nameText.text = skill.skillName;

        if (levelText != null)
        {
            string levelLabel = $"Nv. {level + 1}/{skill.LevelCount}";
            levelText.text = string.IsNullOrEmpty(equippedKey) ? levelLabel : $"{levelLabel}  ·  {string.Format(equippedFormat, equippedKey)}";
        }

        int rarity = skill.RarityForLevel(level);
        Color plateColor = RarityHelper.Color(rarity);

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(rarity);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = plateColor;
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(rarity);
            iconGlow.enabled = glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(rarity);
        }

        if (cardBorder != null) cardBorder.color = plateColor;

        var theme = UIThemeConfig.Instance;
        if (cardFill != null && theme != null)
            cardFill.color = Color.Lerp(theme.panelBackground, Shade(plateColor, cardFillDarkness), cardFillRarityBlend);

        SetUpgradeState(cost, isMax, affordable);
        SetFocused(false);
    }

    public void SetUpgradeState(int cost, bool isMax, bool affordable)
    {
        CaptureCostColor();

        if (costText != null)
        {
            costText.text = isMax ? maxLevelCostText : cost.ToString();
            var theme = UIThemeConfig.Instance;
            costText.color = isMax || affordable || theme == null ? _costColor : theme.actionDestructive;
        }

        if (coinIcon != null) coinIcon.SetActive(!isMax);
        if (upgradeLabel != null) upgradeLabel.text = isMax ? maxLevelText : upgradeText;
        if (upgradeButton != null) upgradeButton.interactable = !isMax && affordable;
    }

    public void SetFocused(bool focused)
    {
        if (selectionBrackets != null) selectionBrackets.SetActive(focused);
    }

    public void PlayEnter(float delay)
    {
        transform.DOKill();
        transform.localScale = Vector3.one * enterFromScale;
        if (group != null)
        {
            group.alpha = 0f;
            group.DOFade(1f, enterDuration * 0.6f).SetDelay(delay).AsUI(gameObject);
        }

        transform.DOScale(1f, enterDuration).SetDelay(delay).SetEase(Ease.OutBack).AsUI(gameObject);
    }

    public void PlayUpgraded()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * upgradePunch, 0.3f, 6, 0.5f).AsUI(gameObject);

        if (levelText != null)
        {
            levelText.transform.DOKill(true);
            levelText.transform.localScale = Vector3.one;
            levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.35f, 6, 0.5f).AsUI(levelText.gameObject);
        }

        if (upgradeButton != null)
        {
            upgradeButton.transform.DOKill(true);
            upgradeButton.transform.localScale = Vector3.one;
            upgradeButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.6f).AsUI(upgradeButton.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => _onFocus?.Invoke(this);

    public void OnPointerClick(PointerEventData eventData) => _onFocus?.Invoke(this);

    void CaptureCostColor()
    {
        if (_costColorCaptured || costText == null) return;
        _costColor = costText.color;
        _costColorCaptured = true;
    }

    static Color Shade(Color color, float factor) => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}
