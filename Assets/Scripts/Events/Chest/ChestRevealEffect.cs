using System;
using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestRevealEffect : MonoBehaviour
{
    static ChestRevealEffect _instance;

    public static ChestRevealEffect Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<ChestRevealEffect>(FindObjectsInactive.Include);
                if (found != null && !found.gameObject.activeSelf)
                    found.gameObject.SetActive(true);
            }

            return _instance;
        }
    }

    [Header("Tela")]
    [SerializeField] private Image dimBackground;
    [SerializeField] private CanvasGroup panel;

    [Header("Card do item")]
    [SerializeField] private ChestRevealStage stage;
    [SerializeField] private RectTransform itemCard;
    [SerializeField] private CanvasGroup itemCardGroup;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private CanvasGroup exileOverlay;
    [SerializeField] private RectTransform exileStamp;

    [Header("Botões")]
    [SerializeField] private Button btnExile;
    [SerializeField] private Button btnSkip;
    [SerializeField] private Button btnTake;

    [Header("Painéis laterais")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private StatsUI statsUI;
    [SerializeField] private StarterAssets.PlayerStatsAggregator playerStats;

    Action _onTake, _onExile, _onSkip;
    bool _canDecide;
    float _dimAlpha;

    Button[] Buttons => new[] { btnExile, btnSkip, btnTake };

    void Awake()
    {
        _instance = this;
        _dimAlpha = dimBackground.color.a;
        dimBackground.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);

        btnTake.onClick.AddListener(() => Decide(btnTake, TakeAnimation, _onTake));
        btnExile.onClick.AddListener(() => Decide(btnExile, ExileAnimation, _onExile));
        btnSkip.onClick.AddListener(() => Decide(btnSkip, SkipAnimation, _onSkip));

        if (playerStats == null) playerStats = FindFirstObjectByType<StarterAssets.PlayerStatsAggregator>();
    }

    public void Show(ItemDefinition item, IReadOnlyList<ItemDefinition> reelPool, Action onTake, Action onExile, Action onSkip)
    {
        _onTake = onTake;
        _onExile = onExile;
        _onSkip = onSkip;
        _canDecide = false;

        Open();
        ResetCard();
        SetButtonsVisible(false);
        itemNameText.text = "Sorteando...";
        descriptionText.text = string.Empty;

        inventoryUI.gameObject.SetActive(true);
        statsUI.Bind(playerStats);

        stage.PlayRoulette(item, reelPool, () =>
        {
            _canDecide = true;
            itemNameText.text = item.itemName;
            descriptionText.text = item.description;
            itemNameText.transform.localScale = Vector3.one * 1.25f;
            itemNameText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).AsUI(itemNameText.gameObject);
            SetButtonsVisible(true);
        });
    }

    void Open()
    {
        dimBackground.gameObject.SetActive(true);
        dimBackground.color = WithAlpha(dimBackground.color, 0f);
        dimBackground.DOFade(_dimAlpha, 0.15f).AsUI(dimBackground.gameObject);

        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.85f;
        panel.DOFade(1f, 0.15f).AsUI(panel.gameObject);
        panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsUI(panel.gameObject);
    }

    void ResetCard()
    {
        itemCard.localScale = Vector3.one;
        itemCard.localRotation = Quaternion.identity;
        itemCardGroup.alpha = 1f;
        exileOverlay.alpha = 0f;
        exileStamp.localScale = Vector3.one;
    }

    void SetButtonsVisible(bool visible)
    {
        var buttons = Buttons;
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            button.interactable = visible;
            button.transform.DOKill();
            button.transform.localScale = Vector3.one * (visible ? 0.6f : 0f);
            if (visible)
                button.transform.DOScale(1f, 0.32f).SetDelay(i * 0.06f).SetEase(Ease.OutBack).AsUI(button.gameObject);
        }
    }

    void Decide(Button button, Func<Sequence> animation, Action onDecided)
    {
        if (!_canDecide) return;
        _canDecide = false;

        foreach (var b in Buttons) b.interactable = false;
        button.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.6f).AsUI(button.gameObject);

        animation()
            .Append(panel.transform.DOScale(0.9f, 0.22f).SetEase(Ease.InBack))
            .Join(panel.DOFade(0f, 0.22f))
            .Join(dimBackground.DOFade(0f, 0.22f))
            .AsUI(gameObject)
            .OnComplete(() =>
            {
                panel.gameObject.SetActive(false);
                dimBackground.gameObject.SetActive(false);
                onDecided?.Invoke();
            });
    }

    Sequence TakeAnimation()
    {
        stage.PlayBurst(0.3f);
        return DOTween.Sequence()
            .Append(itemCard.DOPunchScale(Vector3.one * 0.06f, 0.3f, 6, 0.5f));
    }

    Sequence SkipAnimation()
    {
        return DOTween.Sequence()
            .Join(itemCard.DOScale(0.92f, 0.18f))
            .Join(itemCardGroup.DOFade(0.35f, 0.18f));
    }

    Sequence ExileAnimation()
    {
        Color destructive = UIThemeConfig.Instance.actionDestructive;
        stage.Paint(destructive, Color.Lerp(destructive, Color.white, 0.3f), 0.15f);
        exileStamp.localScale = Vector3.one * 1.8f;

        return DOTween.Sequence()
            .Join(exileOverlay.DOFade(1f, 0.12f))
            .Join(exileStamp.DOScale(1f, 0.26f).SetEase(Ease.OutBack))
            .Append(itemCard.DOShakeRotation(0.3f, new Vector3(0f, 0f, 7f), 18, 90f, false))
            .Append(itemCard.DOScale(0.2f, 0.3f).SetEase(Ease.InBack))
            .Join(itemCard.DOLocalRotate(new Vector3(0f, 0f, -12f), 0.3f))
            .Join(itemCardGroup.DOFade(0f, 0.3f));
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
