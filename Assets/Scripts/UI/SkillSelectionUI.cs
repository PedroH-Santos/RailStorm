using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject panel;
    public Image gameBackground;

    [Header("Cards de melhoria")]
    public List<SkillCardUI> cards;

    [Header("Inventário")]
    public Transform inventorySkillsContainer;
    public GameObject inventorySlotPrefab;

    [Header("Atributos")]
    public Transform statsContainer;
    public GameObject statRowPrefab;

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

    [Header("BackGround")]
    Color _normalBgColor;
    Color _exileBgColor = new Color(0.6f, 0f, 0f, 0.85f);

    StarterAssets.PlayerController _playerController;
    StarterAssets.PlayerSkillHandler _skillHandler;

    List<SkillCardData> _currentOptions = new();
    List<SkillDefinition> _fullPool = new();
    int _refreshesLeft;
    int _exilesLeft;
    bool _exileMode = false;

    Action<SkillDefinition> _onChosen;
    Action _onPassed;

    void Awake()
    {
        panel.SetActive(false);

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
        List<SkillDefinition> fullPool,
        StarterAssets.PlayerController playerController,
        StarterAssets.PlayerSkillHandler skillHandler,
        Action<SkillDefinition> onChosen,
        Action onPassed = null)
    {
        _currentOptions = new List<SkillCardData>(options);
        _fullPool = fullPool;
        _playerController = playerController;
        _skillHandler = skillHandler;
        _onChosen = onChosen;
        _onPassed = onPassed;
        _refreshesLeft = maxRefreshes;
        _exilesLeft = maxExiles;
        _exileMode = false;
        panel.SetActive(true);
        gameBackground.gameObject.SetActive(true);
        Time.timeScale = 0f;

        Cursor.visible = true;

        SetBackground(false);
        RenderCards();
        RenderInventory();
        RenderStats();
        UpdateButtons();
    }

    // ── Cards ──────────────────────────────────────────────────────────────

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
        if (_exileMode)
        {
            _exilesLeft--;
            _exileMode = false;
            SetBackground(false);

            SkillDefinition exiled = _currentOptions[index].skill;
            _skillHandler.ExileSkill(exiled);
            _currentOptions.RemoveAt(index);

            SkillCardData replacement = DrawReplacement();
            if (replacement != null)
                _currentOptions.Add(replacement);

            RenderCards();
            UpdateButtons();
        }
        else
        {
            SkillDefinition chosen = _currentOptions[index].skill;
            Close();
            _onChosen?.Invoke(chosen);
        }
    }

    // ── Botões ─────────────────────────────────────────────────────────────

    void OnExile()
    {
        if (_exilesLeft <= 0) return;
        Debug.Log("IRA EXILAR");
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
        _currentOptions = DrawNewOptions(cards.Count);

        RenderCards();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnExile.interactable = _exilesLeft > 0;
        btnRefresh.interactable = _refreshesLeft > 0;

        // Atualiza badges
        if (exileCountText != null)
            exileCountText.text = $"{_exilesLeft}";

        if (refreshCountText != null)
            refreshCountText.text = $"{_refreshesLeft}";
    }

    void SetBackground(bool exileMode)
    {
        if (gameBackground == null) return;
        gameBackground.color = exileMode ? _exileBgColor : _normalBgColor;
    }
    SkillCardData DrawReplacement()
    {
        var inUse = _currentOptions.Select(o => o.skill);
        var result = SkillDrawer.Draw(_fullPool, _skillHandler, 1, inUse);
        return result.Count > 0 ? result[0] : null;
    }

    List<SkillCardData> DrawNewOptions(int count)
    {
        return SkillDrawer.Draw(_fullPool, _skillHandler, count);
    }

    // ── Inventário ─────────────────────────────────────────────────────────

    void RenderInventory()
    {
        foreach (Transform child in inventorySkillsContainer)
            Destroy(child.gameObject);

        foreach (var pair in _skillHandler.AcquiredSkills)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventorySkillsContainer);
            var slotUI = slot.GetComponent<InventorySlotUI>();
            if (slotUI != null)
                slotUI.Setup(pair.Key, pair.Value);
        }
    }

    // ── Atributos ──────────────────────────────────────────────────────────

    void RenderStats()
    {
        foreach (Transform child in statsContainer)
            Destroy(child.gameObject);

        AddStat("Velocidade", $"{_playerController.moveSpeed:F1}");
        AddStat("Moedas", $"{_playerController.Coins}");

        AddDivider();

        AddStat("Dash", _skillHandler.HasMechanic(MechanicType.Dash) ? "✓ ON" : "OFF");
        AddStat("Salto duplo", _skillHandler.HasMechanic(MechanicType.DoubleJump) ? "✓ ON" : "OFF");
        AddStat("Escudo", _skillHandler.HasMechanic(MechanicType.Shield) ? "✓ ON" : "OFF");

        AddDivider();

        foreach (var pair in _skillHandler.AcquiredSkills)
            if (pair.Key.skillType == SkillType.Stat)
                AddStat(pair.Key.skillName, $"Lv {pair.Value}/{pair.Key.MaxLevel}");
    }

    void AddStat(string label, string value)
    {
        GameObject row = Instantiate(statRowPrefab, statsContainer);
        var rowUI = row.GetComponent<StatRowUI>();
        if (rowUI != null) rowUI.Setup(label, value);
    }

    void AddDivider()
    {
        GameObject row = Instantiate(statRowPrefab, statsContainer);
        var rowUI = row.GetComponent<StatRowUI>();
        if (rowUI != null) rowUI.SetAsDivider();
    }

    // ── Fechar ─────────────────────────────────────────────────────────────

    void Close()
    {
        Time.timeScale = 1f;
        panel.SetActive(false);
        gameBackground.gameObject.SetActive(false);

        Cursor.visible = true;

    }
}