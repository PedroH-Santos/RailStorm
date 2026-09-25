using System;
using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BlacksmithRowMode
{
    Upgrade,
    Buy,
    Equip,
}

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
    public string buyText = "COMPRAR";
    public string newSkillText = "NOVA";
    public string notEquippedText = "Fora dos slots";
    public string equipPromptText = "Equipar em";
    public GameObject selectionBrackets;

    [Header("Teclas (aba EQUIPAR)")]
    public GameObject slotButtonsRoot;
    public List<Button> slotButtons = new();
    [Tooltip("Cor do botão da tecla onde a skill já está equipada.")]
    public Color slotEquippedColor = new Color(0.737f, 0.384f, 0.106f, 1f);
    [Tooltip("Cor do botão de uma tecla livre para equipar.")]
    public Color slotFreeColor = new Color(0.365f, 0.514f, 0.608f, 1f);
    public CanvasGroup group;

    [Header("Animação")]
    public float enterFromScale = 0.6f;
    public float enterDuration = 0.32f;
    public float upgradePunch = 0.08f;

    public SkillDefinition Skill { get; private set; }
    public BlacksmithRowMode Mode { get; private set; }

    Action<BlacksmithSkillRowUI> _onFocus;
    Action<BlacksmithSkillRowUI> _onAction;
    Action<BlacksmithSkillRowUI, int> _onSlot;
    Color _costColor;
    bool _costColorCaptured;

    void Awake()
    {
        CaptureCostColor();
        if (upgradeButton != null) upgradeButton.onClick.AddListener(() => _onAction?.Invoke(this));

        for (int i = 0; i < slotButtons.Count; i++)
        {
            int slot = i;
            if (slotButtons[i] != null) slotButtons[i].onClick.AddListener(() => _onSlot?.Invoke(this, slot));
        }
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        if (group != null) group.alpha = 1f;
    }

    public void SetupUpgrade(SkillDefinition skill, int level, int cost, bool isMax, bool affordable, string equippedKey,
        Action<BlacksmithSkillRowUI> onFocus, Action<BlacksmithSkillRowUI> onAction)
    {
        Bind(skill, level, BlacksmithRowMode.Upgrade, onFocus, onAction);
        SetLevelText(LevelLabel(skill, level), equippedKey, false);
        SetUpgradeState(cost, isMax, affordable);
    }

    public void SetupBuy(SkillDefinition skill, int cost, bool affordable,
        Action<BlacksmithSkillRowUI> onFocus, Action<BlacksmithSkillRowUI> onAction)
    {
        Bind(skill, 0, BlacksmithRowMode.Buy, onFocus, onAction);
        if (levelText != null) levelText.text = $"{newSkillText}  ·  {LevelLabel(skill, 0)}";
        SetPrice(cost, affordable, buyText);
    }

    public void SetupEquip(SkillDefinition skill, int level, int equippedSlot, IReadOnlyList<string> slotKeys, int unlockedSlots,
        Action<BlacksmithSkillRowUI> onFocus, Action<BlacksmithSkillRowUI, int> onSlot)
    {
        Bind(skill, level, BlacksmithRowMode.Equip, onFocus, null);
        _onSlot = onSlot;

        string equippedKey = equippedSlot >= 0 && equippedSlot < slotKeys.Count ? slotKeys[equippedSlot] : null;
        SetLevelText(LevelLabel(skill, level), equippedKey, true);

        CaptureCostColor();
        if (costText != null)
        {
            costText.text = equipPromptText;
            costText.color = _costColor;
        }

        if (coinIcon != null) coinIcon.SetActive(false);
        if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
        if (slotButtonsRoot != null) slotButtonsRoot.SetActive(true);

        for (int i = 0; i < slotButtons.Count; i++)
        {
            var button = slotButtons[i];
            if (button == null) continue;

            bool exists = i < slotKeys.Count;
            button.gameObject.SetActive(exists);
            if (!exists) continue;

            bool unlocked = i < unlockedSlots;
            bool equippedHere = i == equippedSlot;

            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = slotKeys[i];

            if (button.targetGraphic != null)
                button.targetGraphic.color = equippedHere ? slotEquippedColor : slotFreeColor;

            button.interactable = unlocked;
        }
    }

    static string LevelLabel(SkillDefinition skill, int level) => $"Nv. {level + 1}/{skill.LevelCount}";

    void SetLevelText(string levelLabel, string equippedKey, bool showUnequipped)
    {
        if (levelText == null) return;

        if (!string.IsNullOrEmpty(equippedKey))
            levelText.text = $"{levelLabel}  ·  {string.Format(equippedFormat, equippedKey)}";
        else
            levelText.text = showUnequipped ? $"{levelLabel}  ·  {notEquippedText}" : levelLabel;
    }

    void Bind(SkillDefinition skill, int level, BlacksmithRowMode mode, Action<BlacksmithSkillRowUI> onFocus, Action<BlacksmithSkillRowUI> onAction)
    {
        Skill = skill;
        Mode = mode;
        _onFocus = onFocus;
        _onAction = onAction;
        _onSlot = null;
        gameObject.SetActive(true);

        if (upgradeButton != null) upgradeButton.gameObject.SetActive(true);
        if (slotButtonsRoot != null) slotButtonsRoot.SetActive(false);

        if (iconImage != null)
        {
            iconImage.sprite = skill.icon;
            iconImage.enabled = skill.icon != null;
        }

        if (nameText != null) nameText.text = skill.skillName;

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

        SetFocused(false);
    }

    public void SetUpgradeState(int cost, bool isMax, bool affordable)
    {
        if (isMax)
        {
            CaptureCostColor();
            if (costText != null)
            {
                costText.text = maxLevelCostText;
                costText.color = _costColor;
            }

            if (coinIcon != null) coinIcon.SetActive(false);
            if (upgradeLabel != null) upgradeLabel.text = maxLevelText;
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        SetPrice(cost, affordable, upgradeText);
    }

    void SetPrice(int cost, bool affordable, string actionText)
    {
        CaptureCostColor();

        if (costText != null)
        {
            costText.text = cost.ToString();
            var theme = UIThemeConfig.Instance;
            costText.color = affordable || theme == null ? _costColor : theme.actionDestructive;
        }

        if (coinIcon != null) coinIcon.SetActive(true);
        if (upgradeLabel != null) upgradeLabel.text = actionText;
        if (upgradeButton != null) upgradeButton.interactable = affordable;
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

    public void PlaySlotEquipped(int slot)
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * upgradePunch, 0.3f, 6, 0.5f).AsUI(gameObject);

        if (slot < 0 || slot >= slotButtons.Count || slotButtons[slot] == null) return;

        var t = slotButtons[slot].transform;
        t.DOKill(true);
        t.localScale = Vector3.one;
        t.DOPunchScale(Vector3.one * 0.2f, 0.25f, 8, 0.6f).AsUI(slotButtons[slot].gameObject);
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
