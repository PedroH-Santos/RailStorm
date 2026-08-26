using System;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
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

    [Header("Placa")]
    [SerializeField] private Image plaqueFill;
    [SerializeField] private Image plaqueShadow;
    [SerializeField] private Image shield;
    [SerializeField] private Image icon;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image costIcon;
    [SerializeField] private TMP_Text walletText;

    [Header("Pips")]
    [SerializeField] private Transform pipsContainer;
    [SerializeField] private GameObject pipEmptyPrefab;
    [SerializeField] private GameObject pipFullPrefab;

    [Header("Acoes")]
    [SerializeField] private Button unlockButton;
    [SerializeField] private TMP_Text unlockLabel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Dicas")]
    [SerializeField] private TMP_Text hintsText;

    ChooseWayScreenAnimator _animator;
    Color _accent = Color.white;

    void Awake()
    {
        _instance = this;
        _animator = GetComponentInChildren<ChooseWayScreenAnimator>(true);
        ApplyTheme();
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    public void ApplyTheme()
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyTitle(titleText);
        theme.ApplyBody(descriptionText);
        theme.ApplyStatValue(costText);
        theme.ApplyBody(walletText);
        theme.ApplyTitle(unlockLabel);
        theme.ApplyBody(hintsText);
        theme.ApplyPrimaryAction(unlockButton);

        Color woodText = Color.Lerp(theme.woodOutline, theme.textTitle, 0.3f);
        if (descriptionText != null) descriptionText.color = woodText;
        if (walletText != null) walletText.color = woodText;
        if (hintsText != null) hintsText.color = woodText;

        if (costIcon != null) costIcon.color = theme.panelBorder;

        foreach (var shadow in GetComponentsInChildren<Shadow>(true))
        {
            if (shadow is Outline) theme.ApplyIconOutline(shadow);
            else theme.ApplyTextShadow(shadow);
        }
    }

    public void Show(SplineEntry entry, int position, int total, bool affordable, int coins, Action onUnlock, Action onPrevious, Action onNext)
    {
        if (canvasRoot != null) canvasRoot.SetActive(true);
        if (barRoot != null) barRoot.SetActive(true);

        _accent = IsUnset(entry.themeColor) ? UIThemeConfig.Instance.panelBorder : entry.themeColor;
        _accent.a = 1f;

        Tint(shield, _accent, 1f);

        if (titleText != null) titleText.text = entry.destinationName;
        if (descriptionText != null) descriptionText.text = entry.description;

        var theme = UIThemeConfig.Instance;

        if (costText != null)
        {
            costText.text = entry.unlockCost.ToString();
            costText.color = affordable ? theme.textTitle : theme.actionDestructive;
        }

        if (walletText != null) walletText.text = "Você tem " + coins;

        if (icon != null)
        {
            icon.enabled = entry.themeIcon != null;
            if (entry.themeIcon != null) icon.sprite = entry.themeIcon;
        }

        BuildPips(position, total);

        Wire(unlockButton, onUnlock);
        unlockButton.interactable = affordable;

        Wire(previousButton, onPrevious);
        previousButton.gameObject.SetActive(position > 0);

        Wire(nextButton, onNext);
        nextButton.gameObject.SetActive(position < total - 1);

        _animator?.PlayEnter();
    }

    public void Pop()
    {
        _animator?.PlayPop();
    }

    public void Hide()
    {
        if (barRoot != null) barRoot.SetActive(false);
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    void BuildPips(int position, int total)
    {
        if (pipsContainer == null || pipEmptyPrefab == null || pipFullPrefab == null) return;

        for (int i = pipsContainer.childCount - 1; i >= 0; i--)
            Destroy(pipsContainer.GetChild(i).gameObject);

        for (int i = 0; i < total; i++)
            Instantiate(i == position ? pipFullPrefab : pipEmptyPrefab, pipsContainer);
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
