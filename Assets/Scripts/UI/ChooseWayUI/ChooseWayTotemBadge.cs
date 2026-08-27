using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseWayTotemBadge : MonoBehaviour
{
    [Header("Raiz")]
    [SerializeField] private GameObject root;

    [Header("Pecas")]
    [SerializeField] private Image cardFill;
    [SerializeField] private Image accentPlate;
    [SerializeField] private Image crest;
    [SerializeField] private Image icon;
    [SerializeField] private Image lock_;
    [SerializeField] private Image lockGlyph;
    [SerializeField] private Image costPill;
    [SerializeField] private Image costIcon;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    [Header("Cores")]
    [SerializeField, Range(0f, 1f)] private float restingBlend = 0.55f;
    [SerializeField, Range(0f, 1f)] private float restingCardBlend = 0.3f;

    ChooseWayBadgeAnimator _animator;
    Color _accent = Color.white;

    void Awake()
    {
        _animator = GetComponentInChildren<ChooseWayBadgeAnimator>(true);
        ApplyTheme();
        if (root != null) root.SetActive(false);
    }

    public void ApplyTheme()
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyTitle(titleText);
        theme.ApplyBody(descriptionText);
        theme.ApplyStatValue(costText);

        if (titleText != null)
        {
            titleText.fontStyle = FontStyles.UpperCase;
            titleText.color = theme.woodOutline;
        }

        if (descriptionText != null) descriptionText.color = theme.woodOutline;
        if (costText != null) costText.color = theme.woodOutline;

        if (crest != null) crest.color = theme.woodDark;
        if (lock_ != null) lock_.color = Color.white;
        if (lockGlyph != null) lockGlyph.color = theme.woodOutline;
        if (costIcon != null) costIcon.color = theme.panelBorder;
        if (costPill != null) costPill.color = Color.white;

        foreach (var shadow in GetComponentsInChildren<Shadow>(true))
        {
            if (shadow is Outline) theme.ApplyIconOutline(shadow);
            else theme.ApplyTextShadow(shadow);
        }
    }

    public void SetAccent(Color color)
    {
        _accent = color;
        SetSelected(false);
    }

    public void Bind(SplineEntry entry, bool affordable)
    {
        var theme = UIThemeConfig.Instance;

        if (titleText != null) titleText.text = entry.destinationName;
        if (descriptionText != null) descriptionText.text = entry.description;

        if (costText != null)
        {
            costText.text = entry.unlockCost.ToString();
            costText.color = theme == null
                ? (affordable ? Color.black : new Color(0.6f, 0.1f, 0.1f))
                : (affordable ? theme.woodOutline : theme.actionDestructive);
        }

        if (icon != null)
        {
            icon.enabled = entry.themeIcon != null;
            if (entry.themeIcon != null) icon.sprite = entry.themeIcon;
        }
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        var dim = new Color(0.2f, 0.2f, 0.2f);

        if (accentPlate != null)
            accentPlate.color = selected ? _accent : Color.Lerp(_accent, dim, restingBlend);

        if (cardFill != null)
            cardFill.color = selected ? Color.white : Color.Lerp(Color.white, dim, restingCardBlend);

        if (icon != null)
            icon.color = selected ? _accent : Color.Lerp(_accent, dim, restingBlend);

        _animator?.SetSelected(selected);
    }
}
