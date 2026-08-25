using System;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public TMP_Text levelText;
    public Image cardBackground;
    [Tooltip("Anel de borda do card. Recebe a cor cheia da raridade, destacando o card contra o painel.")]
    public Image cardBorder;

    [Header("Ícone")]
    [Tooltip("Brilho da raridade desenhado entre a placa e o ícone. Sprite e cor vêm do RarityConfig.")]
    public Image iconGlow;

    [Tooltip("Borda ao redor da placa do ícone. Recebe a cor da raridade escurecida, para destacar sem brigar com o fundo.")]
    public Image iconBorder;

    [Header("Fundo por raridade")]
    [Tooltip("Preenchimento do corpo do card. Recebe a cor do painel misturada com a cor da raridade.")]
    public Image cardFill;

    [Range(0f, 1f)]
    [Tooltip("Quanto da cor de raridade escurecida entra no fundo do card. 0 = cor do painel, 1 = só a raridade escurecida.")]
    public float cardFillRarityBlend = 0.7f;

    [Range(0f, 1f)]
    [Tooltip("Fator que escurece a cor de raridade antes de virar fundo do card. Preserva o matiz em vez de lavá-lo contra o navy.")]
    public float cardFillDarkness = 0.3f;

    [Header("Seleção")]
    [Tooltip("Container das cantoneiras que marcam o card sob o cursor. Fica oculto até o mouse entrar no card.")]
    public GameObject selectionBrackets;

    Action _onClick;
    Button _selfButton;

    void Awake() => ApplyTheme();

    void OnDisable() => SetSelected(false);

    void ApplyTheme()
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyIconOutline(iconImage != null ? iconImage.GetComponent<Outline>() : null);
        theme.ApplyTextShadow(nameText != null ? nameText.GetComponent<Shadow>() : null);
        theme.ApplyTextShadow(levelText != null ? levelText.GetComponent<Shadow>() : null);
        theme.ApplyTextShadow(rarityText != null ? rarityText.GetComponent<Shadow>() : null);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetSelected(true);

    public void OnPointerExit(PointerEventData eventData) => SetSelected(false);

    void SetSelected(bool selected)
    {
        if (selectionBrackets != null)
            selectionBrackets.SetActive(selected);
    }

    public void Setup(AbilityCardData data, Action onClick)
    {
        _onClick = onClick;
        SetSelected(false);

        IDrawable d = data.drawable;
        int ri = data.targetRarity;

        var theme = UIThemeConfig.Instance;
        theme?.ApplyTitle(nameText);
        theme?.ApplyBody(descriptionText);

        nameText.text = d.DisplayName;

        if (iconImage != null && d.Icon != null)
            iconImage.sprite = d.Icon;

        Color rarityColor = RarityHelper.Color(ri);

        if (rarityText != null)
        {
            rarityText.text = RarityHelper.DisplayName(ri);
            if (theme != null && theme.bodyFont != null) rarityText.font = theme.bodyFont;
            rarityText.color = rarityColor;
        }

        if (cardBackground != null)
        {
            var plate = RarityHelper.IconPlate(ri);
            if (plate != null) cardBackground.sprite = plate;
            cardBackground.color = rarityColor;
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(ri);
            iconGlow.enabled = glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(ri);
        }

        if (cardBorder != null) cardBorder.color = rarityColor;
        if (iconBorder != null) iconBorder.color = Shade(rarityColor, 0.45f);

        if (cardFill != null && theme != null)
            cardFill.color = Color.Lerp(theme.panelBackground, Shade(rarityColor, cardFillDarkness), cardFillRarityBlend);
        if (levelText != null)
        {
            if (theme != null && theme.titleFont != null) levelText.font = theme.titleFont;
            levelText.color = rarityColor;
            levelText.text = data.isNew ? "NOVO" : $"Nível {ri + 1}";
        }

        descriptionText.text = string.Empty;

        if (d is SkillDefinition skill)
        {
            descriptionText.text = skill.description;
        }
        else if (d is WeaponSkillDefinition weaponSkill)
        {
            var level = weaponSkill.GetLevelForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(weaponSkill.description)
                ? weaponSkill.description
                : $"{StatLabels.Of(weaponSkill.statTarget)} +{level.statValue:0.#}{(level.isMultiplier ? "%" : string.Empty)}";
        }
        else if (d is WeaponDefinition weapon && !data.isUpgrade)
        {
            var stats = weapon.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(weapon.description)
                ? weapon.description
                : $"DMG {stats.damage}  |  {stats.attackRate:F1}/s  |  Alc. {stats.range:F0}m";
        }
        else if (d is WeaponDefinition wu && data.isUpgrade)
        {
            var handler = PlayerCartWeaponHandler.Instance;
            var prev = handler != null ? handler.GetCurrentStats(wu) : wu.GetStatsForRarity(0);
            var next = handler != null ? handler.GetNextStats(wu) : wu.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(wu.description)
                ? wu.description
                : $"DMG {prev.damage}→{next.damage}" +
                  $"  |  {prev.attackRate:F1}→{next.attackRate:F1}/s" +
                  $"  |  Alc. {prev.range:F0}→{next.range:F0}m";
        }

        if (_selfButton == null) _selfButton = GetComponent<Button>();
        _selfButton.onClick.RemoveAllListeners();
        _selfButton.onClick.AddListener(() => _onClick?.Invoke());

    }



    public void OnPointerClick(PointerEventData e) => _onClick?.Invoke();

    static Color Shade(Color color, float factor)
        => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}