using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectionUI : MonoBehaviour
{
    public GameObject parentPanel;
    public Image gameBackground;
    public List<AbilityCardUI> cards;
    public InventoryUI inventoryUI;
    public StatsUI statsUI;
    public Button btnExile;
    public Button btnPass;
    public Button btnRefresh;
    public TMP_Text exileCountText;
    public TMP_Text refreshCountText;
    public int maxRefreshes = 2;
    public int maxExiles = 3;

    Color _normalBgColor;
    readonly Color _exileBgColor = new Color(0.6f, 0f, 0f, 0.85f);

    StarterAssets.PlayerController _playerController;
    StarterAssets.PlayerSkillHandler _skillHandler;
    PlayerCartWeaponHandler _weaponHandler;

    List<AbilityCardData> _currentOptions = new();
    List<SkillDefinition> _fullSkillPool = new();
    List<WeaponDefinition> _fullWeaponPool = new();
    List<WeaponSkillDefinition> _fullWeaponSkillPool = new();

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

        if (gameBackground != null)
        {
            gameBackground.gameObject.SetActive(false);
            _normalBgColor = gameBackground.color;
        }
    }

    public void Show(
        List<AbilityCardData> options,
        List<SkillDefinition> fullSkillPool,
        List<WeaponDefinition> fullWeaponPool,
        List<WeaponSkillDefinition> fullWeaponSkillPool,
        StarterAssets.PlayerController playerController,
        StarterAssets.PlayerSkillHandler skillHandler,
        PlayerCartWeaponHandler weaponHandler,
        Action onPassed = null,
        Action onClosed = null)
    {
        _currentOptions = new List<AbilityCardData>(options);
        _fullSkillPool = fullSkillPool;
        _fullWeaponPool = fullWeaponPool;
        _fullWeaponSkillPool = fullWeaponSkillPool;
        _playerController = playerController;
        _skillHandler = skillHandler;
        _weaponHandler = weaponHandler;
        _onPassed = onPassed;
        _onClosed = onClosed;
        _refreshesLeft = maxRefreshes;
        _exilesLeft = maxExiles;
        _exileMode = false;

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
        AbilityCardData data = _currentOptions[index];

        if (_exileMode)
        {
            _exilesLeft--;
            _exileMode = false;
            SetBackground(false);

            if (data.drawable is SkillDefinition s) _skillHandler.ExileSkill(s);
            if (data.drawable is WeaponDefinition w) _weaponHandler?.ExileWeapon(w);

            _currentOptions.RemoveAt(index);
            var rep = DrawReplacement();
            if (rep != null) _currentOptions.Add(rep);

            RenderCards();
            UpdateButtons();
            return;
        }

        Close();

        if (data.drawable is SkillDefinition skill)
            _skillHandler.ApplySkill(skill, data.targetRarity);
        else if (data.drawable is WeaponSkillDefinition weaponSkill)
            data.targetWeapon?.ApplyWeaponSkill(weaponSkill, data.targetRarity);
        else if (data.drawable is WeaponDefinition weapon)
        {
            if (data.isUpgrade) _weaponHandler?.UpgradeWeapon(weapon, data.targetRarity);
            else _weaponHandler?.AcquireWeapon(weapon, data.targetRarity);
        }
    }

    void OnExile()
    {
        if (_exilesLeft <= 0) return;
        _exileMode = !_exileMode;
        SetBackground(_exileMode);
        UpdateButtons();
    }

    void OnPass() { Close(); _onPassed?.Invoke(); }

    void OnRefresh()
    {
        if (_refreshesLeft <= 0) return;
        _refreshesLeft--;
        _currentOptions = AbilityDrawer.Draw(
            _fullSkillPool, _fullWeaponPool, _fullWeaponSkillPool, _skillHandler, _weaponHandler, cards.Count);
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

    void SetBackground(bool exile)
    {
        if (gameBackground == null) return;
        gameBackground.color = exile ? _exileBgColor : _normalBgColor;
    }

    AbilityCardData DrawReplacement()
    {
        var r = AbilityDrawer.Draw(
            _fullSkillPool, _fullWeaponPool, _fullWeaponSkillPool, _skillHandler, _weaponHandler, 1, _currentOptions);
        return r.Count > 0 ? r[0] : null;
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
