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

    [Header("Cards de melhoria")]
    public List<SkillCardUI> cards;

    [Header("Inventário")]
    public Transform inventorySkillsContainer;
    public GameObject inventorySlotPrefab;

    [Header("Atributos")]
    public Transform statsContainer;
    public GameObject statRowPrefab;

    [Header("Botões")]
    public Button btnConfirm;
    public Button btnExile;
    public Button btnPass;
    public Button btnRefresh;
    public TMP_Text refreshCountText;

    [Header("Config")]
    public int maxRefreshes = 2;

    StarterAssets.PlayerController _playerController;
    StarterAssets.PlayerSkillHandler _skillHandler;

    List<SkillCardData> _currentOptions = new();
    List<SkillDefinition> _fullPool = new();
    int _selectedIndex = -1;
    int _refreshesLeft;

    Action<SkillDefinition> _onChosen;
    Action _onPassed;

    void Awake()
    {
        panel.SetActive(false);

        //btnConfirm.onClick.AddListener(OnConfirm);
        //btnExile.onClick.AddListener(OnExile);
        //btnPass.onClick.AddListener(OnPass);
        //btnRefresh.onClick.AddListener(OnRefresh);
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
        _selectedIndex = -1;

        panel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RenderCards();
        //RenderInventory();
        //RenderStats();
        //UpdateButtons();
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
                cards[i].SetSelected(i == _selectedIndex);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    void OnCardClicked(int index)
    {
        if (_selectedIndex == index)
        {
            OnConfirm();
            return;
        }

        _selectedIndex = index;
        RenderCards();
        //UpdateButtons();
    }

    // ── Botões ─────────────────────────────────────────────────────────────

    void OnConfirm()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _currentOptions.Count) return;

        SkillDefinition chosen = _currentOptions[_selectedIndex].skill;
        Close();
        _onChosen?.Invoke(chosen);
    }

    void OnExile()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _currentOptions.Count) return;

        _currentOptions.RemoveAt(_selectedIndex);
        _selectedIndex = -1;

        SkillCardData replacement = DrawReplacement();
        if (replacement != null)
            _currentOptions.Add(replacement);

        RenderCards();
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
        _selectedIndex = -1;
        _currentOptions = DrawNewOptions(cards.Count);

        RenderCards();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        bool hasSelection = _selectedIndex >= 0;
        btnConfirm.interactable = hasSelection;
        btnExile.interactable = hasSelection;
        btnRefresh.interactable = _refreshesLeft > 0;

        if (refreshCountText != null)
            refreshCountText.text = $"Atualizar ({_refreshesLeft}/{maxRefreshes})";
    }

    // ── Sorteio para Exile / Refresh ───────────────────────────────────────

    SkillCardData DrawReplacement()
    {
        HashSet<string> inUse = new(_currentOptions.Select(o => o.skill.name));

        List<SkillDefinition> candidates = _fullPool
            .Where(s => !inUse.Contains(s.name))
            .Where(s => _skillHandler.GetSkillLevel(s) == 0 || _skillHandler.CanLevelUp(s))
            .ToList();

        if (candidates.Count == 0) return null;

        float total = candidates.Sum(s => s.weight);
        float roll = UnityEngine.Random.Range(0f, total);
        float acc = 0f;

        foreach (var s in candidates)
        {
            acc += s.weight;
            if (roll <= acc)
            {
                int current = _skillHandler.GetSkillLevel(s);
                return new SkillCardData(s, current + 1, s.weight);
            }
        }

        return null;
    }

    List<SkillCardData> DrawNewOptions(int count)
    {
        List<SkillDefinition> candidates = _fullPool
            .Where(s => _skillHandler.GetSkillLevel(s) == 0 || _skillHandler.CanLevelUp(s))
            .ToList();

        List<SkillCardData> result = new();
        List<SkillDefinition> remaining = new(candidates);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            float total = remaining.Sum(s => s.weight);
            float roll = UnityEngine.Random.Range(0f, total);
            float acc = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                acc += remaining[j].weight;
                if (roll <= acc)
                {
                    int current = _skillHandler.GetSkillLevel(remaining[j]);
                    result.Add(new SkillCardData(remaining[j], current + 1, remaining[j].weight));
                    remaining.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
}