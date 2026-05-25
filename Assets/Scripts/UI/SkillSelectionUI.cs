using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject parentPanel;
    private GameObject panel;
    public Image gameBackground;

    [Header("Cards de melhoria")]
    public List<SkillCardUI> cards;

    [Header("Inventário")]
    public InventoryUI inventoryUI;

    [Header("Stats")]
    public StatsUI statsUI;

    [Header("Botões")]
    public Button btnExile;
    public Button btnPass;
    public Button btnRefresh;

    [Header("Badges dos Botões")]
    public TMP_Text exileCountText;
    public TMP_Text refreshCountText;

    [Header("Config")]
    public int maxRefreshes = 2;
    public int maxExiles = 3;

    Color _normalBgColor;
    readonly Color _exileBgColor = new Color(0.6f, 0f, 0f, 0.85f);

    StarterAssets.PlayerController _playerController;
    StarterAssets.PlayerSkillHandler _skillHandler;
    CarWeaponHandler _weaponHandler;

    List<SkillCardData> _currentOptions = new();
    List<SkillDefinition> _fullSkillPool = new();
    List<WeaponDefinition> _fullWeaponPool = new();

    int _refreshesLeft;
    int _exilesLeft;
    bool _exileMode;

    Action<SkillDefinition> _onChosen;
    Action _onPassed;
    Action _onClosed;


    void Awake()
    {
        panel = gameObject;
        btnExile.onClick.AddListener(OnExile);
        btnPass.onClick.AddListener(OnPass);
        btnRefresh.onClick.AddListener(OnRefresh);

        if (gameBackground != null)
        {
            gameBackground.gameObject.SetActive(false);
            _normalBgColor = gameBackground.color;
        }
    }

    public void Show(
        List<SkillCardData> options,
        List<SkillDefinition> fullSkillPool,
        List<WeaponDefinition> fullWeaponPool,
        StarterAssets.PlayerController playerController,
        StarterAssets.PlayerSkillHandler skillHandler,
        CarWeaponHandler weaponHandler,
        Action<SkillDefinition> onChosen,
        Action onPassed = null,
        Action onClosed = null)
    {
        _currentOptions = new List<SkillCardData>(options);
        _fullSkillPool = fullSkillPool;
        _fullWeaponPool = fullWeaponPool;
        _playerController = playerController;
        _skillHandler = skillHandler;
        _weaponHandler = weaponHandler;
        _onChosen = onChosen;
        _onPassed = onPassed;
        _refreshesLeft = maxRefreshes;
        _exilesLeft = maxExiles;
        _exileMode = false;
        _onClosed = onClosed;

        parentPanel.SetActive(true);
        if (gameBackground != null) gameBackground.gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;

        SetBackground(false);
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
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    void OnCardClicked(int index)
    {
        SkillCardData data = _currentOptions[index];

        if (_exileMode)
        {
            _exilesLeft--;
            _exileMode = false;
            SetBackground(false);

            if (data.drawable is SkillDefinition skill)
                _skillHandler.ExileSkill(skill);
            else if (data.drawable is WeaponDefinition weapon)
                _weaponHandler?.ExileWeapon(weapon);

            _currentOptions.RemoveAt(index);

            SkillCardData replacement = DrawReplacement();
            if (replacement != null)
                _currentOptions.Add(replacement);

            RenderCards();
            UpdateButtons();
        }
        else
        {
            if (data.drawable is SkillDefinition chosenSkill)
            {
                Close();
                _onChosen?.Invoke(chosenSkill);
            }
            else if (data.drawable is WeaponDefinition chosenWeapon)
            {
                Close();
                _weaponHandler?.AcquireWeapon(chosenWeapon);
            }
        }
    }

    void OnExile()
    {
        if (_exilesLeft <= 0) return;
        _exileMode = !_exileMode;
        SetBackground(_exileMode);
        UpdateButtons();
    }

    void OnPass()
    {
        Close();
        _onPassed?.Invoke();
    }

    void OnRefresh()
    {
        if (_refreshesLeft <= 0) return;

        _refreshesLeft--;
        _currentOptions = SkillDrawer.Draw(
            _fullSkillPool, _fullWeaponPool,
            _skillHandler, _weaponHandler,
            cards.Count);

        RenderCards();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnExile.interactable = _exilesLeft > 0;
        btnRefresh.interactable = _refreshesLeft > 0;

        if (exileCountText != null) exileCountText.text = $"{_exilesLeft}";
        if (refreshCountText != null) refreshCountText.text = $"{_refreshesLeft}";
    }

    void SetBackground(bool exileMode)
    {
        if (gameBackground == null) return;
        gameBackground.color = exileMode ? _exileBgColor : _normalBgColor;
    }

    SkillCardData DrawReplacement()
    {
        var result = SkillDrawer.Draw(
            _fullSkillPool, _fullWeaponPool,
            _skillHandler, _weaponHandler,
            1,
            _currentOptions);

        return result.Count > 0 ? result[0] : null;
    }

    void Close()
    {
        Time.timeScale = 1f;
        parentPanel.SetActive(false);
        if (gameBackground != null) gameBackground.gameObject.SetActive(false);
        Cursor.visible = true;
        _onClosed?.Invoke(); 
    }
}