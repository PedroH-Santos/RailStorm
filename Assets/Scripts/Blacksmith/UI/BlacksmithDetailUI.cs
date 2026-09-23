using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithDetailUI : MonoBehaviour
{
    [Header("Card")]
    public RectTransform card;
    public Image cardFill;
    public Image cardBorder;
    [Range(0f, 1f)] public float cardFillRarityBlend = 0.7f;
    [Range(0f, 1f)] public float cardFillDarkness = 0.3f;

    [Header("Cabeçalho")]
    public Image iconImage;
    public Image iconPlate;
    public Image iconGlow;
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text descriptionText;
    public GameObject descriptionBox;

    [Header("Stats (atual » próximo)")]
    public Transform statsContainer;
    public TooltipStatRowUI statRowTemplate;
    [Tooltip("Cor do valor do próximo nível quando ele melhora o atual.")]
    public Color improvementColor = new Color(0.208f, 0.78f, 0.353f, 1f);
    public string noCooldownText = "Sem recarga";

    [Header("Custo / Carteira")]
    public TMP_Text costText;
    public TMP_Text walletText;
    public TMP_Text costBurnText;
    public float costBurnRise = 60f;
    public string maxLevelCostText = "MÁX";

    [Header("Equipar")]
    public Button equipButton;
    public TMP_Text equipLabel;

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

    public void Show(SkillDefinition skill, int level, int cost, bool isMax, int coins, bool affordable, bool animate)
    {
        bool hasSkill = skill != null;
        if (contentRoot != null) contentRoot.SetActive(hasSkill);
        if (emptyState != null) emptyState.SetActive(!hasSkill);

        if (!hasSkill)
        {
            SetWallet(coins, true, true, 0);
            return;
        }

        if (nameText != null) nameText.text = skill.skillName;
        if (levelText != null) levelText.text = isMax ? $"Nível {level + 1} / {skill.LevelCount} (máx.)" : $"Nível {level + 1} / {skill.LevelCount}";

        if (descriptionText != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(skill.description);
            descriptionText.gameObject.SetActive(hasDescription);
            if (descriptionBox != null) descriptionBox.SetActive(hasDescription);
            descriptionText.text = skill.description;
        }

        if (iconImage != null)
        {
            iconImage.sprite = skill.icon;
            iconImage.enabled = skill.icon != null;
        }

        Color plateColor = RarityHelper.Color(0);

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(0);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = plateColor;
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(0);
            iconGlow.enabled = glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(0);
        }

        if (cardBorder != null) cardBorder.color = plateColor;

        var theme = UIThemeConfig.Instance;
        if (cardFill != null && theme != null)
            cardFill.color = Color.Lerp(theme.panelBackground, Shade(plateColor, cardFillDarkness), cardFillRarityBlend);

        BuildRows(BuildLines(skill, level, isMax));
        SetWallet(coins, affordable, isMax, cost);

        if (animate && card != null)
        {
            card.DOKill(true);
            card.localScale = Vector3.one;
            card.DOPunchScale(Vector3.one * 0.03f, 0.22f, 6, 0.5f).AsUI(card.gameObject);
        }
    }

    List<TooltipStatLine> BuildLines(SkillDefinition skill, int level, bool isMax)
    {
        var lines = new List<TooltipStatLine>();
        int next = Mathf.Min(level + 1, skill.MaxLevel);
        string hex = ColorUtility.ToHtmlStringRGB(improvementColor);

        foreach (var target in skill.DisplayStats)
        {
            bool isCooldown = target == ESkillStatTarget.Cooldown;
            bool currentHas = !isCooldown || skill.HasCooldown(level);
            bool nextHas = !isCooldown || skill.HasCooldown(next);

            string current = currentHas ? SkillStatFormatting.Format(target, skill.GetStatValue(level, target)) : noCooldownText;
            if (isMax)
            {
                lines.Add(new TooltipStatLine(StatLabels.Of(target), current));
                continue;
            }

            string upcoming = nextHas ? SkillStatFormatting.Format(target, skill.GetStatValue(next, target)) : noCooldownText;
            bool improves = currentHas && nextHas
                ? SkillStatFormatting.IsImprovement(target, skill.GetStatValue(level, target), skill.GetStatValue(next, target))
                : currentHas && !nextHas;

            string value = improves ? $"{current} <color=#{hex}>» {upcoming}</color>" : current;
            lines.Add(new TooltipStatLine(StatLabels.Of(target), value));
        }

        return lines;
    }

    public void SetWallet(int coins, bool affordable, bool isMax, int cost)
    {
        if (walletText != null) walletText.text = coins.ToString();

        if (costText != null)
        {
            costText.text = isMax ? maxLevelCostText : cost.ToString();
            var theme = UIThemeConfig.Instance;
            costText.color = isMax || affordable || theme == null ? _costColor : theme.actionDestructive;
        }
    }

    public void SetEquip(string label, bool interactable)
    {
        if (equipLabel != null) equipLabel.text = label;
        if (equipButton != null) equipButton.interactable = interactable;
    }

    public void PlayUpgraded()
    {
        foreach (var row in _rows)
        {
            if (!row.gameObject.activeSelf || row.valueText == null) continue;
            var t = row.valueText.transform;
            t.DOKill(true);
            t.localScale = Vector3.one;
            t.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6, 0.5f).AsUI(row.gameObject);
        }

        if (levelText != null)
        {
            levelText.transform.DOKill(true);
            levelText.transform.localScale = Vector3.one;
            levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.35f, 6, 0.5f).AsUI(levelText.gameObject);
        }
    }

    public void PlayEquipped()
    {
        if (equipButton == null) return;
        var t = equipButton.transform;
        t.DOKill(true);
        t.localScale = Vector3.one;
        t.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.6f).AsUI(equipButton.gameObject);
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
