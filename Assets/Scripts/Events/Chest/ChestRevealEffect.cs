using System;
using System.Collections.Generic;
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

    [Header("Fundo escurecido (atrás do painel)")]
    [SerializeField] private Image dimBackground;
    [SerializeField] private float dimFadeDuration = 0.15f;

    [Header("Painel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image panelBorder;

    [Header("Textos")]
    [SerializeField] private Image rarityTagPlate;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Roleta")]
    [SerializeField] private ChestRouletteAnimator roulette;

    [Header("Botões de decisão")]
    [SerializeField] private Button btnTake;
    [SerializeField] private Button btnExile;
    [SerializeField] private Button btnSkip;

    [Header("Timings")]
    [SerializeField] private float panelInDuration = 0.25f;
    [SerializeField] private float panelOutDuration = 0.2f;

    [Header("Painéis laterais")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private StatsUI statsUI;
    [SerializeField] private StarterAssets.PlayerStatsAggregator playerStats;

    [Header("Partículas (opcional, na posição do baú no mundo)")]
    [SerializeField] private ParticleSystem burstParticlesPrefab;

    Action _onTake, _onExile, _onSkip;
    bool _waitingForDecision;
    bool _spinning;
    Tween _panelTween;
    float _dimTargetAlpha;

    void Awake()
    {
        _instance = this;

        if (dimBackground != null)
        {
            var c = dimBackground.color;
            _dimTargetAlpha = c.a;
            c.a = 0f;
            dimBackground.color = c;
            dimBackground.gameObject.SetActive(false);
        }

        if (panelRoot != null) panelRoot.SetActive(false);

        if (btnTake != null) btnTake.onClick.AddListener(() => Decide(_onTake));
        if (btnExile != null) btnExile.onClick.AddListener(() => Decide(_onExile));
        if (btnSkip != null) btnSkip.onClick.AddListener(() => Decide(_onSkip));

        if (playerStats == null) playerStats = FindFirstObjectByType<StarterAssets.PlayerStatsAggregator>();
    }

    public void Show(ItemDefinition item, int rarityIndex, IReadOnlyList<ItemDefinition> reelPool, Vector3 worldPos, Action onTake, Action onExile, Action onSkip)
    {
        _onTake = onTake;
        _onExile = onExile;
        _onSkip = onSkip;

        if (burstParticlesPrefab != null)
        {
            var ps = Instantiate(burstParticlesPrefab, worldPos, Quaternion.identity);
            var main = ps.main;
            main.useUnscaledTime = true;
            main.startColor = RarityHelper.Color(rarityIndex);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        SetButtonsInteractable(false);
        SetPendingTexts();

        if (inventoryUI != null) inventoryUI.gameObject.SetActive(true);
        statsUI?.Bind(playerStats);

        Open();

        _spinning = true;
        roulette?.Play(item, rarityIndex, reelPool, () =>
        {
            _spinning = false;
            _waitingForDecision = true;
            SetFinalTexts(item, rarityIndex);
            SetButtonsInteractable(true);
        });
    }

    void Open()
    {
        if (dimBackground != null)
        {
            dimBackground.gameObject.SetActive(true);
            dimBackground.DOKill();
            dimBackground.DOFade(_dimTargetAlpha, dimFadeDuration).SetUpdate(true);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.localScale = Vector3.one * 0.85f;
            _panelTween?.Kill();
            _panelTween = panelRoot.transform.DOScale(1f, panelInDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    void SetPendingTexts()
    {
        if (rarityText != null) rarityText.text = "???";
        if (itemNameText != null) itemNameText.text = "Sorteando...";
        if (descriptionText != null) descriptionText.text = string.Empty;
    }

    void SetFinalTexts(ItemDefinition item, int rarityIndex)
    {
        Color rarityColor = RarityHelper.Color(rarityIndex);

        if (rarityText != null)
        {
            rarityText.text = RarityHelper.DisplayName(rarityIndex);
            rarityText.transform.DOKill();
            rarityText.transform.localScale = Vector3.one * 1.3f;
            rarityText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (rarityTagPlate != null) rarityTagPlate.color = rarityColor;
        if (panelBorder != null) panelBorder.color = rarityColor;
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (btnTake != null) btnTake.interactable = interactable;
        if (btnExile != null) btnExile.interactable = interactable;
        if (btnSkip != null) btnSkip.interactable = interactable;
    }

    void Decide(Action callback)
    {
        if (!_waitingForDecision || _spinning) return;
        _waitingForDecision = false;

        callback?.Invoke();
        Close();
    }

    void Close()
    {
        roulette?.Stop();
        SetButtonsInteractable(false);

        _panelTween?.Kill();
        _panelTween = panelRoot.transform.DOScale(0f, panelOutDuration).SetEase(Ease.InCubic).SetUpdate(true)
            .OnComplete(() =>
            {
                panelRoot.SetActive(false);
                panelRoot.transform.localScale = Vector3.one;
            });

        if (dimBackground != null)
        {
            dimBackground.DOKill();
            dimBackground.DOFade(0f, dimFadeDuration).SetUpdate(true)
                .OnComplete(() => dimBackground.gameObject.SetActive(false));
        }
    }

    void OnDestroy()
    {
        _panelTween?.Kill();
    }
}
