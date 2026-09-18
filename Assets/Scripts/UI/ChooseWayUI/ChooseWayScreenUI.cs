using System;
using System.Collections;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChooseWayScreenUI : MonoBehaviour
{
    static ChooseWayScreenUI _instance;
    public static ChooseWayScreenUI Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<ChooseWayScreenUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    found.gameObject.SetActive(true);
                    _instance = found;
                }
            }

            return _instance;
        }
    }

    [Header("Raiz")]
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private GameObject barRoot;

    [Header("Ícone")]
    [FormerlySerializedAs("shield")]
    [SerializeField] private Image iconPlate;
    [SerializeField] private Image icon;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    [Header("Pips")]
    [SerializeField] private Transform pipsContainer;
    [SerializeField] private GameObject pipEmptyPrefab;
    [SerializeField] private GameObject pipFullPrefab;

    [Header("Acoes")]
    [SerializeField] private Button unlockButton;
    [SerializeField] private TMP_Text unlockLabel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private BackButtonUI backButton;

    [Header("Dicas")]
    [SerializeField] private TMP_Text hintsText;

    [Header("Desbloqueio")]
    [SerializeField] private TMP_Text unlockedStamp;
    [SerializeField] private TMP_Text costBurn;
    [SerializeField] private string unlockedMessage = "CAMINHO LIBERADO";
    [SerializeField] private float costBurnRise = 60f;
    [SerializeField] private float costBurnDuration = 0.7f;

    ChooseWayScreenAnimator _animator;
    Color _accent = Color.white;
    Color _costColor;
    Coroutine _burnRoutine;

    void Awake()
    {
        _instance = this;
        _animator = GetComponentInChildren<ChooseWayScreenAnimator>(true);
        if (costText != null) _costColor = costText.color;
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    public void Show(SplineEntry entry, int position, int total, bool affordable, int coins, Action onUnlock, Action onPrevious, Action onNext, Action onBack)
    {
        bool wasVisible = barRoot != null && barRoot.activeSelf;

        if (canvasRoot != null) canvasRoot.SetActive(true);
        if (barRoot != null) barRoot.SetActive(true);

        _accent = IsUnset(entry.themeColor) ? UIThemeConfig.Instance.panelBorder : entry.themeColor;
        _accent.a = 1f;

        Tint(iconPlate, _accent, 1f);

        if (titleText != null) titleText.text = entry.destinationName;
        if (descriptionText != null) descriptionText.text = entry.description;

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

        if (unlockedStamp != null) unlockedStamp.gameObject.SetActive(false);
        if (costBurn != null) costBurn.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.gameObject.SetActive(true);

        BuildPips(position, total);

        Wire(unlockButton, onUnlock);
        unlockButton.interactable = affordable;

        Wire(previousButton, onPrevious);
        previousButton.gameObject.SetActive(position > 0);

        Wire(nextButton, onNext);
        nextButton.gameObject.SetActive(position < total - 1);

        if (backButton != null)
        {
            backButton.SetAction(onBack);
            backButton.Interactable = true;
        }

        if (!wasVisible) _animator?.PlayEnter();
    }

    public void Pop()
    {
        _animator?.PlayPop();
    }

    public void PlayUnlocked(int cost)
    {
        if (unlockButton != null) unlockButton.interactable = false;
        if (backButton != null) backButton.Interactable = false;

        if (unlockedStamp != null)
        {
            unlockedStamp.text = unlockedMessage;
            unlockedStamp.color = _accent;
            unlockedStamp.gameObject.SetActive(true);
            if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        }

        if (costText != null) costText.text = "0";

        if (costBurn != null)
        {
            if (_burnRoutine != null) StopCoroutine(_burnRoutine);
            _burnRoutine = StartCoroutine(CostBurnRoutine(cost));
        }

        _animator?.PlayUnlockPunch();
    }

    IEnumerator CostBurnRoutine(int cost)
    {
        costBurn.text = "-" + cost;
        costBurn.gameObject.SetActive(true);

        RectTransform rect = costBurn.rectTransform;
        Vector2 basePosition = rect.anchoredPosition;
        Color baseColor = costBurn.color;

        float t = 0f;
        while (t < costBurnDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / costBurnDuration);

            rect.anchoredPosition = basePosition + new Vector2(0f, costBurnRise * Easing.CubicOut(p));
            costBurn.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - Easing.CubicIn(p));

            yield return null;
        }

        rect.anchoredPosition = basePosition;
        costBurn.color = baseColor;
        costBurn.gameObject.SetActive(false);
        _burnRoutine = null;
    }

    public void Hide()
    {
        if (backButton != null) backButton.Interactable = false;

        if (barRoot == null || !barRoot.activeSelf)
        {
            HideImmediate();
            return;
        }

        if (_animator == null)
        {
            HideImmediate();
            return;
        }

        _animator.PlayExit(HideImmediate);
    }

    void HideImmediate()
    {
        if (_burnRoutine != null)
        {
            StopCoroutine(_burnRoutine);
            _burnRoutine = null;
        }

        if (unlockedStamp != null) unlockedStamp.gameObject.SetActive(false);
        if (costBurn != null) costBurn.gameObject.SetActive(false);
        if (barRoot != null) barRoot.SetActive(false);
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    void BuildPips(int position, int total)
    {
        if (pipsContainer == null || pipEmptyPrefab == null || pipFullPrefab == null) return;

        for (int i = pipsContainer.childCount - 1; i >= 0; i--)
            Destroy(pipsContainer.GetChild(i).gameObject);

        for (int i = 0; i < total; i++)
        {
            bool current = i == position;
            var pip = Instantiate(current ? pipFullPrefab : pipEmptyPrefab, pipsContainer);
            pip.SetActive(true);
            if (current && pip.TryGetComponent(out Image pipImage)) Tint(pipImage, _accent, 1f);
        }
    }

    static void Wire(Button button, Action callback)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        if (callback != null) button.onClick.AddListener(() => callback());
    }

    static void Tint(Graphic graphic, Color color, float alpha)
    {
        if (graphic == null) return;
        color.a = alpha;
        graphic.color = color;
    }

    static bool IsUnset(Color color) => color.r > 0.97f && color.g > 0.97f && color.b > 0.97f;
}
