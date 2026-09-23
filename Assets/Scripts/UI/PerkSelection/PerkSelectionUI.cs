using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Systems.UITheme;

public class PerkSelectionUI : MonoBehaviour
{
    public GameObject parentPanel;
    public Image gameBackground;
    public List<PerkCardUI> cards;
    public InventoryUI inventoryUI;
    public StatsUI statsUI;
    public Button btnExile;
    public Button btnPass;
    public Button btnRefresh;
    public TMP_Text exileCountText;
    public TMP_Text refreshCountText;
    public int maxRefreshes = 2;
    public int maxExiles = 3;

    [Header("Animação")]
    [Tooltip("Atraso entre a entrada de um card e o seguinte.")]
    public float cardEnterStagger = 0.06f;

    [Tooltip("Força do punch de escala ao clicar num botão.")]
    public float buttonPunch = 0.12f;

    [Header("Modo exílio")]
    [Tooltip("Título do painel de cards. Troca de texto e cor enquanto o modo exílio está ligado.")]
    public TMP_Text panelTitle;

    [Tooltip("Dica abaixo do título que explica o exílio. Aparece só durante o modo exílio, fora dos cards, para não cobrir a descrição das habilidades.")]
    public CanvasGroup exileHint;

    [Tooltip("Texto do título do painel durante o modo exílio.")]
    public string exileTitle = "ESCOLHA UMA PARA EXILAR";

    [Tooltip("Rótulo do botão Exilar enquanto o modo exílio está ligado.")]
    public string exileCancelLabel = "Cancelar";

    [Tooltip("Atraso entre o carimbo EXILAR de um card e o do seguinte.")]
    public float exileStampStagger = 0.05f;

    [Range(0f, 1f)]
    [Tooltip("Quanto de branco entra na cor destrutiva do título durante o modo exílio. O vermelho puro some contra a faixa navy.")]
    public float exileTitleLighten = 0.3f;

    [Tooltip("Escala máxima do pulso do botão Exilar durante o modo exílio.")]
    public float exileButtonPulseScale = 1.06f;

    Color _normalBgColor;
    Color _exileBgColor;
    Color _destructiveColor = Color.red;

    string _normalTitle;
    Color _normalTitleColor;
    TMP_Text _exileLabel;
    string _normalExileLabel;
    Tween _exileButtonPulse;
    bool _busy;

    StarterAssets.PlayerController _playerController;
    StarterAssets.PlayerPerkHandler _perkHandler;
    PlayerCarWeaponHandler _weaponHandler;

    List<PerkCardData> _currentOptions = new();
    List<PerkDefinition> _fullPerkPool = new();
    List<CarWeaponDefinition> _fullWeaponPool = new();
    List<WeaponPerkDefinition> _fullWeaponPerkPool = new();

    int _refreshesLeft;
    int _exilesLeft;
    bool _exileMode;

    Action _onPassed;
    Action _onClosed;

    void Awake()
    {
        btnExile.onClick.AddListener(OnExile);
        btnPass.onClick.AddListener(OnPass);
        btnRefresh.onClick.AddListener(OnRefresh);

        ApplyTheme();

        if (panelTitle != null)
        {
            _normalTitle = panelTitle.text;
            _normalTitleColor = panelTitle.color;
        }

        _exileLabel = btnExile.GetComponentInChildren<TMP_Text>(true);
        if (_exileLabel != null) _normalExileLabel = _exileLabel.text;

        if (gameBackground != null)
        {
            gameBackground.gameObject.SetActive(false);
            gameBackground.color = _normalBgColor;
        }
    }

    void ApplyTheme()
    {
        var theme = UIThemeConfig.Instance;
        if (theme == null) return;

        theme.ApplyDestructiveAction(btnExile);
        theme.ApplyNeutralAction(btnPass);
        theme.ApplyPrimaryAction(btnRefresh);

        ApplyButtonLabel(theme, btnExile);
        ApplyButtonLabel(theme, btnPass);
        ApplyButtonLabel(theme, btnRefresh);

        theme.ApplyIconOutline(exileCountText != null ? exileCountText.GetComponent<Outline>() : null);
        theme.ApplyIconOutline(refreshCountText != null ? refreshCountText.GetComponent<Outline>() : null);

        _normalBgColor = theme.screenDim;
        _exileBgColor = theme.screenDimExile;
        _destructiveColor = theme.actionDestructive;
    }

    static void ApplyButtonLabel(UIThemeConfig theme, Button button)
    {
        if (button == null) return;

        var label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        theme.ApplyTitle(label);
        theme.ApplyTextShadow(label.GetComponent<Shadow>());
    }

    public void Show(
        List<PerkCardData> options,
        List<PerkDefinition> fullPerkPool,
        List<CarWeaponDefinition> fullWeaponPool,
        List<WeaponPerkDefinition> fullWeaponPerkPool,
        StarterAssets.PlayerController playerController,
        StarterAssets.PlayerPerkHandler perkHandler,
        PlayerCarWeaponHandler weaponHandler,
        Action onPassed = null,
        Action onClosed = null)
    {
        _currentOptions = new List<PerkCardData>(options);
        _fullPerkPool = fullPerkPool;
        _fullWeaponPool = fullWeaponPool;
        _fullWeaponPerkPool = fullWeaponPerkPool;
        _playerController = playerController;
        _perkHandler = perkHandler;
        _weaponHandler = weaponHandler;
        _onPassed = onPassed;
        _onClosed = onClosed;
        _refreshesLeft = maxRefreshes;
        _exilesLeft = maxExiles;
        _busy = false;

        parentPanel.SetActive(true);
        if (gameBackground != null) gameBackground.gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;

        SetExileMode(false);
        RenderCards();
        UpdateButtons();

        statsUI?.Bind(playerController.GetComponent<StarterAssets.PlayerStatsAggregator>());
        inventoryUI?.gameObject.SetActive(true);
    }

    void RenderCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (i < _currentOptions.Count)
            {
                int captured = i;
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(_currentOptions[i], () => OnCardClicked(captured));
                cards[i].PlayEnter(i * cardEnterStagger);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    void OnCardClicked(int index)
    {
        if (_busy || index >= _currentOptions.Count) return;

        PerkCardData data = _currentOptions[index];

        if (_exileMode)
        {
            _busy = true;
            LockButtons();
            cards[index].PlayExile(() => ConfirmExile(data));
            return;
        }

        Close();

        if (data.drawable is PerkDefinition perk)
            _perkHandler.ApplyPerk(perk, data.targetRarity);
        else if (data.drawable is WeaponPerkDefinition weaponPerk)
            _weaponHandler?.ApplyWeaponPerk(data.targetWeapon, weaponPerk, data.targetRarity);
        else if (data.drawable is CarWeaponDefinition weapon)
        {
            if (data.isUpgrade) _weaponHandler?.UpgradeWeapon(weapon, data.targetRarity);
            else _weaponHandler?.AcquireWeapon(weapon, data.targetRarity);
        }
    }

    void PunchButton(Button button)
    {
        if (button == null) return;
        var target = button.transform;
        target.DOKill(true);
        target.DOPunchScale(Vector3.one * buttonPunch, 0.25f, 8, 0.6f)
            .SetUpdate(true)
            .SetLink(button.gameObject);
    }

    void ConfirmExile(PerkCardData data)
    {
        _busy = false;
        _exilesLeft--;
        SetExileMode(false);

        if (data.drawable is PerkDefinition s) _perkHandler.ExilePerk(s);
        if (data.drawable is CarWeaponDefinition w) _weaponHandler?.ExileWeapon(w);

        _currentOptions.Remove(data);
        var rep = DrawReplacement();
        if (rep != null) _currentOptions.Add(rep);

        RenderCards();
        UpdateButtons();
    }

    void OnExile()
    {
        if (_busy) return;
        if (!_exileMode && _exilesLeft <= 0) return;
        SetExileMode(!_exileMode);
        UpdateButtons();
    }

    void SetExileMode(bool on)
    {
        _exileMode = on;
        SetBackground(on);

        for (int i = 0; i < cards.Count; i++)
            if (cards[i] != null && cards[i].gameObject.activeSelf)
                cards[i].SetExileMode(on, i * exileStampStagger);

        if (panelTitle != null && _normalTitle != null)
        {
            panelTitle.text = on ? exileTitle : _normalTitle;
            panelTitle.color = on ? Color.Lerp(_destructiveColor, Color.white, exileTitleLighten) : _normalTitleColor;
            if (on && panelTitle.isActiveAndEnabled)
            {
                panelTitle.transform.DOKill(true);
                panelTitle.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.6f)
                    .SetUpdate(true)
                    .SetLink(panelTitle.gameObject);
            }
        }

        if (exileHint != null)
        {
            exileHint.DOKill();
            if (exileHint.isActiveAndEnabled)
                exileHint.DOFade(on ? 1f : 0f, 0.2f).SetUpdate(true).SetLink(exileHint.gameObject);
            else
                exileHint.alpha = on ? 1f : 0f;
        }

        if (_exileLabel != null && _normalExileLabel != null)
            _exileLabel.text = on ? exileCancelLabel : _normalExileLabel;

        _exileButtonPulse?.Kill();
        _exileButtonPulse = null;
        btnExile.transform.localScale = Vector3.one;
        if (on && btnExile.isActiveAndEnabled)
        {
            _exileButtonPulse = btnExile.transform.DOScale(exileButtonPulseScale, 0.45f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .SetLink(btnExile.gameObject);
        }
    }

    void LockButtons()
    {
        btnExile.interactable = false;
        btnPass.interactable = false;
        btnRefresh.interactable = false;
    }

    void OnPass()
    {
        if (_busy) return;
        SetExileMode(false);
        Close();
        _onPassed?.Invoke();
    }

    void OnRefresh()
    {
        if (_busy || _refreshesLeft <= 0) return;
        SetExileMode(false);
        PunchButton(btnRefresh);
        _refreshesLeft--;
        _currentOptions = PerkDrawer.Draw(
            _fullPerkPool, _fullWeaponPool, _fullWeaponPerkPool, _perkHandler, _weaponHandler, cards.Count);
        RenderCards();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnExile.interactable = _exilesLeft > 0 || _exileMode;
        btnPass.interactable = true;
        btnRefresh.interactable = _refreshesLeft > 0;
        if (exileCountText != null) exileCountText.text = $"{_exilesLeft}";
        if (refreshCountText != null) refreshCountText.text = $"{_refreshesLeft}";
    }

    void SetBackground(bool exile)
    {
        if (gameBackground == null) return;
        gameBackground.color = exile ? _exileBgColor : _normalBgColor;
    }

    PerkCardData DrawReplacement()
    {
        var r = PerkDrawer.Draw(
            _fullPerkPool, _fullWeaponPool, _fullWeaponPerkPool, _perkHandler, _weaponHandler, 1, _currentOptions);
        return r.Count > 0 ? r[0] : null;
    }

    void Close()
    {
        SetExileMode(false);
        _busy = false;
        Time.timeScale = 1f;
        parentPanel.SetActive(false);
        if (gameBackground != null) gameBackground.gameObject.SetActive(false);
        Cursor.visible = true;
        _onClosed?.Invoke();
    }
}
