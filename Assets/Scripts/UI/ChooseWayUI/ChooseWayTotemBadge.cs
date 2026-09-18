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
    [SerializeField] private Image icon;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
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
    Color _costColor;
    Coroutine _routine;

    void Awake()
    {
        _animator = GetComponentInChildren<ChooseWayBadgeAnimator>(true);
        _canvas = GetComponent<Canvas>();
        if (costText != null) _costColor = costText.color;
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

    public void SetAccent(Color color)
    {
        _accent = color;
        SetSelected(false);
    }

    public void Bind(SplineEntry entry, bool affordable)
    {
        if (titleText != null) titleText.text = entry.destinationName;

        if (costText != null)
        {
            costText.text = entry.unlockCost.ToString();
            costText.color = affordable ? _costColor : UIThemeConfig.Instance.actionDestructive;
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

            if (accentPlate != null) accentPlate.color = Color.Lerp(_accent, Color.white, flash);

            yield return null;
        }

        if (accentPlate != null) accentPlate.color = _accent;
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
            icon.color = selected ? Color.white : Color.Lerp(Color.white, dim, restingCardBlend);

        _animator?.SetSelected(selected);
    }
}
