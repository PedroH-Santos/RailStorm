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
    public GameObject walletBox;
    public TMP_Text costText;
    public TMP_Text walletText;
    public TMP_Text costBurnText;
    public float costBurnRise = 60f;
    public string maxLevelCostText = "MÁX";
    public string newSkillText = "NOVA";

    [Header("Runas (aba RUNAS)")]
    public List<GameObject> coinIcons = new();
    public List<GameObject> runeIcons = new();
    [Tooltip("Linha da carteira. Some na aba RUNAS: o saldo de Runas fica no painel de Status.")]
    public GameObject walletRow;

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

    public void ShowNothing(bool showWallet, int coins)
    {
        ShowContent(false);
        ShowWalletBox(showWallet);
        SetCurrency(false);
        SetWallet(coins, true, true, 0);
    }

    public void ShowForSale(SkillDefinition skill, int coins, bool affordable, bool animate)
    {
        var firstLevel = SkillLevel.First;

        ShowContent(true);
        ShowWalletBox(true);
        SetCurrency(false);

        ApplyHeader(skill.skillName, $"{newSkillText}  ·  {LevelLabel(skill, firstLevel)}", skill.description, skill.icon, skill.RarityAt(firstLevel));
        BuildRows(CurrentStatLines(skill, firstLevel));
        SetWallet(coins, affordable, false, skill.purchaseCost);
        PlayCardPunch(animate);
    }

    public void ShowForUpgrade(OwnedSkill skill, int coins, bool affordable, bool animate)
    {
        ShowOwnedSkillHeader(skill, true);

        var lines = skill.IsAtMaxLevel
            ? CurrentStatLines(skill.Definition, skill.Level)
            : UpgradeStatLines(skill.Definition, skill.Level);
        BuildRows(lines);

        SetWallet(coins, affordable, skill.IsAtMaxLevel, skill.UpgradeCost);
        PlayCardPunch(animate);
    }

    public void ShowForEquip(OwnedSkill skill, bool animate)
    {
        ShowOwnedSkillHeader(skill, false);
        BuildRows(CurrentStatLines(skill.Definition, skill.Level));
        PlayCardPunch(animate);
    }

    public void ShowVariant(OwnedSkill skill, SkillVariantDefinition variant, bool unlocked, int cost, bool affordable, bool animate)
    {
        bool costsARune = !skill.IsUsing(variant) && !unlocked;

        ShowContent(true);
        ShowWalletBox(costsARune);
        SetCurrency(true);

        var definition = skill.Definition;
        ApplyHeader(variant.variantName, definition.skillName, variant.description,
            variant.icon != null ? variant.icon : definition.icon, skill.Rarity);

        BuildRows(CurrentStatLines(definition, skill.Level));

        if (costText != null)
        {
            costText.text = cost.ToString();
            var theme = UIThemeConfig.Instance;
            costText.color = affordable || theme == null ? _costColor : theme.actionDestructive;
        }

        PlayCardPunch(animate);
    }

    void ShowOwnedSkillHeader(OwnedSkill skill, bool showWallet)
    {
        ShowContent(true);
        ShowWalletBox(showWallet);
        SetCurrency(false);

        var definition = skill.Definition;
        string levelLabel = LevelLabel(definition, skill.Level);
        if (skill.IsAtMaxLevel) levelLabel = $"{levelLabel} (máx.)";

        ApplyHeader(definition.skillName, levelLabel, definition.description, definition.icon, skill.Rarity);
    }

    static string LevelLabel(SkillDefinition skill, SkillLevel level) => $"Nível {level.Number} / {skill.LevelCount}";

    void ShowContent(bool hasSkill)
    {
        if (contentRoot != null) contentRoot.SetActive(hasSkill);
        if (emptyState != null) emptyState.SetActive(!hasSkill);
    }

    void ShowWalletBox(bool visible)
    {
        if (walletBox != null) walletBox.SetActive(visible);
    }

    void SetCurrency(bool runes)
    {
        if (walletRow != null) walletRow.SetActive(!runes);

        foreach (var icon in coinIcons)
            if (icon != null) icon.SetActive(!runes);

        foreach (var icon in runeIcons)
            if (icon != null) icon.SetActive(runes);
    }

    void PlayCardPunch(bool animate)
    {
        if (!animate || card == null) return;

        card.DOKill(true);
        card.localScale = Vector3.one;
        card.DOPunchScale(Vector3.one * 0.03f, 0.22f, 6, 0.5f).AsUI(card.gameObject);
    }

    void ApplyHeader(string title, string levelLabel, string description, Sprite icon, int rarity)
    {
        if (nameText != null) nameText.text = title;
        if (levelText != null) levelText.text = levelLabel;

        if (descriptionText != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(description);
            descriptionText.gameObject.SetActive(hasDescription);
            if (descriptionBox != null) descriptionBox.SetActive(hasDescription);
            descriptionText.text = description;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

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
    }

    List<TooltipStatLine> CurrentStatLines(SkillDefinition skill, SkillLevel level)
    {
        var lines = new List<TooltipStatLine>();
        foreach (var stat in skill.DisplayStats)
            lines.Add(new TooltipStatLine(StatLabels.Of(stat), FormatStat(skill, level, stat)));
        return lines;
    }

    List<TooltipStatLine> UpgradeStatLines(SkillDefinition skill, SkillLevel level)
    {
        var lines = new List<TooltipStatLine>();
        var next = skill.NextLevelOrLast(level);
        string improvementHex = ColorUtility.ToHtmlStringRGB(improvementColor);

        foreach (var stat in skill.DisplayStats)
        {
            string current = FormatStat(skill, level, stat);
            string value = StatImproves(skill, level, next, stat)
                ? $"{current} <color=#{improvementHex}>» {FormatStat(skill, next, stat)}</color>"
                : current;

            lines.Add(new TooltipStatLine(StatLabels.Of(stat), value));
        }

        return lines;
    }

    string FormatStat(SkillDefinition skill, SkillLevel level, ESkillStatTarget stat)
    {
        bool isMissingCooldown = stat == ESkillStatTarget.Cooldown && !skill.HasCooldown(level);
        return isMissingCooldown ? noCooldownText : SkillStatFormatting.Format(stat, skill.GetStatValue(level, stat));
    }

    static bool StatImproves(SkillDefinition skill, SkillLevel current, SkillLevel next, ESkillStatTarget stat)
    {
        if (stat == ESkillStatTarget.Cooldown)
        {
            bool hasNow = skill.HasCooldown(current);
            bool hasNext = skill.HasCooldown(next);
            if (!hasNow) return false;
            if (!hasNext) return true;
        }

        return SkillStatFormatting.IsImprovement(stat, skill.GetStatValue(current, stat), skill.GetStatValue(next, stat));
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
