using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseWayTotemBadge : MonoBehaviour
{
    [Header("Raiz")]
    [SerializeField] private GameObject root;

    [Header("Pecas")]
    [SerializeField] private Image shield;
    [SerializeField] private Image icon;
    [SerializeField] private Image lock_;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image costIcon;

    [Header("Cores")]
    [SerializeField, Range(0f, 1f)] private float restingBlend = 0.5f;

    ChooseWayBadgeAnimator _animator;
    Color _accent = Color.white;

    void Awake()
    {
        _animator = GetComponent<ChooseWayBadgeAnimator>();
        ApplyTheme();
        if (root != null) root.SetActive(false);
    }

    public void ApplyTheme()
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyStatValue(costText);
        if (costIcon != null) costIcon.color = theme.panelBorder;
    }

    public void SetAccent(Color color)
    {
        _accent = color;
        SetSelected(false);
    }

    public void Bind(SplineEntry entry, bool affordable)
    {
        var theme = UIThemeConfig.Instance;

        if (costText != null)
        {
            costText.text = entry.unlockCost.ToString();
            costText.color = theme == null
                ? (affordable ? Color.white : new Color(1f, 0.4f, 0.4f))
                : (affordable ? theme.textTitle : theme.actionDestructive);
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
        if (shield != null)
            shield.color = selected ? _accent : Color.Lerp(_accent, new Color(0.2f, 0.2f, 0.2f), restingBlend);

        _animator?.SetSelected(selected);
    }
}
