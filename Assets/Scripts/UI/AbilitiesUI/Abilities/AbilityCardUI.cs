using System;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
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

    [Header("Animação")]
    [Tooltip("Escala do card enquanto o cursor está sobre ele.")]
    public float hoverScale = 1.015f;

    [Tooltip("Duração do crescimento/retorno do hover.")]
    public float hoverDuration = 0.18f;

    [Tooltip("Escala inicial do card quando ele entra na tela.")]
    public float enterFromScale = 0.6f;

    [Tooltip("Duração da entrada do card.")]
    public float enterDuration = 0.32f;

    [Header("Exílio")]
    [Tooltip("CanvasGroup no root do card. Usado para o card sumir ao ser exilado.")]
    public CanvasGroup cardGroup;

    [Tooltip("Camada vermelha + carimbo EXILAR mostrada enquanto o modo exílio está ligado.")]
    public CanvasGroup exileOverlay;

    [Tooltip("Etiqueta EXILAR presa na borda do card. Recebe o golpe de entrada, o crescimento de hover e o golpe final.")]
    public RectTransform exileStamp;

    [Tooltip("Escala da etiqueta EXILAR enquanto o cursor está sobre o card no modo exílio.")]
    public float exileStampHoverScale = 1.2f;

    [Tooltip("Meio ciclo do pisca-pisca vermelho do anel de raridade durante o modo exílio.")]
    public float exileBorderPulseDuration = 0.45f;

    [Tooltip("Duração do card encolhendo e sumindo ao ser exilado.")]
    public float exileVanishDuration = 0.3f;

    Action _onClick;
    Button _selfButton;
    bool _hovered;
    bool _exileMode;
    bool _vanishing;
    Color _rarityColor = Color.white;
    Tween _borderPulse;

    void Awake() => ApplyTheme();

    void OnDisable()
    {
        SetSelected(false);
        transform.DOKill();
        transform.localScale = Vector3.one;
        ResetExileVisuals();
    }

    public void SetExileMode(bool on, float delay)
    {
        _exileMode = on;

        if (exileOverlay != null)
        {
            exileOverlay.DOKill();
            exileOverlay.DOFade(on ? 1f : 0f, 0.18f)
                .SetDelay(on ? delay : 0f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        if (exileStamp != null)
        {
            exileStamp.DOKill();
            if (on)
            {
                exileStamp.localScale = Vector3.one * 1.6f;
                exileStamp.DOScale(_hovered ? exileStampHoverScale : 1f, 0.28f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
            else
            {
                exileStamp.localScale = Vector3.one;
            }
        }

        _borderPulse?.Kill();
        _borderPulse = null;
        if (cardBorder == null) return;

        cardBorder.color = _rarityColor;
        var theme = UIThemeConfig.Instance;
        if (on && theme != null)
        {
            _borderPulse = cardBorder.DOColor(theme.actionDestructive, exileBorderPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }

    public void PlayExile(Action onComplete)
    {
        _vanishing = true;
        _borderPulse?.Kill();
        _borderPulse = null;
        var theme = UIThemeConfig.Instance;
        if (cardBorder != null && theme != null) cardBorder.color = theme.actionDestructive;

        transform.DOKill();
        var sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        if (exileStamp != null)
        {
            exileStamp.DOKill();
            exileStamp.localScale = Vector3.one;
            sequence.Append(exileStamp.DOScale(1.35f, 0.08f).SetEase(Ease.OutQuad));
            sequence.Append(exileStamp.DOScale(1f, 0.12f).SetEase(Ease.InQuad));
        }

        sequence.Join(transform.DOShakeRotation(0.3f, new Vector3(0f, 0f, 7f), 18, 90f, false));
        sequence.Append(transform.DOScale(0.2f, exileVanishDuration).SetEase(Ease.InBack));
        sequence.Join(transform.DORotate(new Vector3(0f, 0f, -12f), exileVanishDuration));
        if (cardGroup != null) sequence.Join(cardGroup.DOFade(0f, exileVanishDuration));

        sequence.OnComplete(() =>
        {
            ResetExileVisuals();
            onComplete?.Invoke();
        });
    }

    void ResetExileVisuals()
    {
        _exileMode = false;
        _vanishing = false;
        _borderPulse?.Kill();
        _borderPulse = null;
        if (cardBorder != null) cardBorder.color = _rarityColor;

        if (exileOverlay != null)
        {
            exileOverlay.DOKill();
            exileOverlay.alpha = 0f;
        }

        if (exileStamp != null)
        {
            exileStamp.DOKill();
            exileStamp.localScale = Vector3.one;
        }

        if (cardGroup != null)
        {
            cardGroup.DOKill();
            cardGroup.alpha = 1f;
        }

        transform.localRotation = Quaternion.identity;
    }

    public void PlayEnter(float delay)
    {
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        if (cardGroup != null) cardGroup.alpha = 1f;
        transform.localScale = Vector3.one * enterFromScale;
        transform.DOScale(_hovered ? hoverScale : 1f, enterDuration)
            .SetDelay(delay)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

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

        if (_hovered == selected) return;
        _hovered = selected;

        if (!isActiveAndEnabled || _vanishing) return;

        if (_exileMode && exileStamp != null)
        {
            exileStamp.DOKill(true);
            exileStamp.DOScale(selected ? exileStampHoverScale : 1f, hoverDuration)
                .SetEase(selected ? Ease.OutBack : Ease.OutCubic)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        transform.DOKill();
        transform.DOScale(selected ? hoverScale : 1f, hoverDuration)
            .SetEase(selected ? Ease.OutBack : Ease.OutCubic)
            .SetUpdate(true)
            .SetLink(gameObject);
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

        _rarityColor = rarityColor;
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
                : WeaponStatFormatting.BuildSummary(stats);
        }
        else if (d is WeaponDefinition wu && data.isUpgrade)
        {
            var handler = PlayerCartWeaponHandler.Instance;
            var prev = handler != null ? handler.GetCurrentStats(wu) : wu.GetStatsForRarity(0);
            var next = handler != null ? handler.GetNextStats(wu) : wu.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(wu.description)
                ? wu.description
                : WeaponStatFormatting.BuildTransitionSummary(prev, next);
        }

        if (_selfButton == null) _selfButton = GetComponent<Button>();
        _selfButton.onClick.RemoveAllListeners();
        _selfButton.onClick.AddListener(() => _onClick?.Invoke());

    }



    public void OnPointerClick(PointerEventData e) => _onClick?.Invoke();

    static Color Shade(Color color, float factor)
        => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}