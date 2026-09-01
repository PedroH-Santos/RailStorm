using System.Collections;
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

    [Header("Desbloqueio")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float flashDuration = 0.22f;
    [SerializeField] private float dismissRise = 42f;

    [Header("Cores")]
    [SerializeField, Range(0f, 1f)] private float restingBlend = 0.55f;
    [SerializeField, Range(0f, 1f)] private float restingCardBlend = 0.3f;

    ChooseWayBadgeAnimator _animator;
    Canvas _canvas;
    static Camera _mainCamera;
    Color _accent = Color.white;
    Coroutine _routine;

    void Awake()
    {
        _animator = GetComponentInChildren<ChooseWayBadgeAnimator>(true);
        _canvas = GetComponent<Canvas>();
        ApplyTheme();
        if (root != null) root.SetActive(false);
    }

    void LateUpdate()
    {
        if (_canvas == null || root == null || !root.activeInHierarchy) return;

        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null) return;

        Vector3 offset = transform.position - _mainCamera.transform.position;
        float depth = Vector3.Dot(offset, _mainCamera.transform.forward);
        _canvas.sortingOrder = Mathf.RoundToInt(-depth * 10f);
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
        if (root == null) return;

        if (group != null) group.alpha = 1f;
        root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void PlayUnlocked()
    {
        if (root == null) return;

        _animator?.PlayPunch();

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(UnlockedRoutine());
    }

    IEnumerator UnlockedRoutine()
    {
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / flashDuration);
            float flash = 1f - p;

            Color lit = Color.Lerp(_accent, Color.white, flash);
            if (accentPlate != null) accentPlate.color = lit;
            if (icon != null) icon.color = lit;

            yield return null;
        }

        if (accentPlate != null) accentPlate.color = _accent;
        if (icon != null) icon.color = _accent;
        _routine = null;
    }

    public void PlayDismiss(float duration)
    {
        if (root == null) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DismissRoutine(duration));
    }

    IEnumerator DismissRoutine(float duration)
    {
        RectTransform rect = root.transform as RectTransform;
        Vector2 basePosition = rect != null ? rect.anchoredPosition : Vector2.zero;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            if (rect != null)
                rect.anchoredPosition = basePosition + new Vector2(0f, dismissRise * Easing.CubicOut(p));

            if (group != null) group.alpha = 1f - Easing.CubicIn(p);

            yield return null;
        }

        if (rect != null) rect.anchoredPosition = basePosition;
        if (group != null) group.alpha = 1f;

        _routine = null;
        Hide();
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
