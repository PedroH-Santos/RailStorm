using System.Collections.Generic;
using DG.Tweening;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithUI : MonoBehaviour
{
    static BlacksmithUI _instance;

    public static BlacksmithUI Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<BlacksmithUI>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    [Header("Tela")]
    public GameObject root;
    public Image dimBackground;
    public CanvasGroup panel;

    [Header("Abas")]
    public List<BlacksmithTabUI> tabs = new();
    public BlacksmithTab startingTab = BlacksmithTab.Shop;

    [Header("Slots")]
    public List<BlacksmithSlotUI> slots = new();

    [Header("Lista")]
    public Transform rowsContainer;
    public BlacksmithSkillRowUI rowPrefab;
    public ScrollRect scrollRect;
    public float rowEnterStagger = 0.05f;

    [Header("Detalhe")]
    public BlacksmithDetailUI detail;

    [Header("Arma")]
    public TMP_Text weaponNameText;

    [Header("Voltar")]
    public BackButtonUI backButton;

    [Header("Painéis laterais")]
    public InventoryUI inventoryUI;
    public StatsUI statsUI;

    PlayerStatsAggregator _stats;
    PlayerSkillHandler _handler;
    PlayerSkillCaster _caster;

    readonly List<BlacksmithSkillRowUI> _rows = new();
    BlacksmithSkillRowUI _focused;
    SkillDefinition _focusedSkill;
    BlacksmithTab _tab;
    float _dimAlpha;
    Sequence _closeSequence;

    void Awake()
    {
        _instance = this;

        if (dimBackground != null) _dimAlpha = dimBackground.color.a;

        if (rowsContainer != null)
            for (int i = rowsContainer.childCount - 1; i >= 0; i--)
                Destroy(rowsContainer.GetChild(i).gameObject);

        foreach (var tab in tabs)
            if (tab != null) tab.Bind(SelectTab);
    }

    public void Open(PlayerStatsAggregator stats, PlayerSkillHandler handler, PlayerSkillCaster caster, System.Action onBack)
    {
        _stats = stats;
        _handler = handler;
        _caster = caster;
        _focusedSkill = null;
        _tab = startingTab;
        RenderTabs(false);

        _closeSequence?.Kill();
        _closeSequence = null;

        root.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayOpen();

        if (backButton != null)
        {
            backButton.SetAction(onBack);
            backButton.Interactable = true;
        }

        if (weaponNameText != null)
            weaponNameText.text = _handler != null && _handler.Weapon != null ? _handler.Weapon.weaponName : string.Empty;

        if (inventoryUI != null) inventoryUI.gameObject.SetActive(true);
        if (statsUI != null) statsUI.Bind(_stats);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;

        RenderSlots();
        RenderRows(true);
    }

    public void Close()
    {
        if (backButton != null) backButton.Interactable = false;

        if (panel == null)
        {
            FinishClose();
            return;
        }

        _closeSequence?.Kill();
        var sequence = _closeSequence = DOTween.Sequence()
            .Join(panel.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InBack))
            .Join(panel.DOFade(0f, 0.2f));

        if (dimBackground != null) sequence.Join(dimBackground.DOFade(0f, 0.2f));

        sequence.AsUI(gameObject).OnComplete(FinishClose);
    }

    void FinishClose()
    {
        _closeSequence = null;
        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void PlayOpen()
    {
        if (dimBackground != null)
        {
            dimBackground.DOKill();
            dimBackground.color = WithAlpha(dimBackground.color, 0f);
            dimBackground.DOFade(_dimAlpha, 0.15f).AsUI(dimBackground.gameObject);
        }

        if (panel != null)
        {
            panel.DOKill();
            panel.transform.DOKill();
            panel.alpha = 0f;
            panel.transform.localScale = Vector3.one * 0.85f;
            panel.DOFade(1f, 0.15f).AsUI(panel.gameObject);
            panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsUI(panel.gameObject);
        }
    }

    void SelectTab(BlacksmithTab tab)
    {
        if (tab == _tab) return;

        _tab = tab;
        RenderTabs(true);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        RenderRows(true);
    }

    void RenderTabs(bool animate)
    {
        foreach (var tab in tabs)
            if (tab != null) tab.SetActive(tab.tab == _tab, animate);
    }

    string KeyLabel(int slot) => _caster != null ? _caster.GetKeyLabel(slot) : (slot + 1).ToString();

    void RenderSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            bool exists = _handler != null && i < _handler.MaxSlots;
            slot.gameObject.SetActive(exists);
            if (!exists) continue;

            var skill = _handler.GetSlot(i);
            bool holdsFocused = skill != null && skill == _focusedSkill;
            slot.Setup(i, skill, _handler.GetLevel(skill), _handler.IsSlotUnlocked(i), KeyLabel(i), holdsFocused, OnInventorySlotClicked);
        }
    }

    List<string> SlotKeys()
    {
        var keys = new List<string>();
        int count = _handler != null ? _handler.MaxSlots : 0;
        for (int i = 0; i < count; i++) keys.Add(KeyLabel(i));
        return keys;
    }

    void OnInventorySlotClicked(int slot)
    {
        if (_handler == null) return;

        var skill = _handler.GetSlot(slot);
        if (skill != null) _focusedSkill = skill;

        if (_tab != BlacksmithTab.Equip)
        {
            SelectTab(BlacksmithTab.Equip);
            return;
        }

        RenderRows(false);
    }

    List<SkillDefinition> ListedSkills()
    {
        var list = new List<SkillDefinition>();
        if (_handler == null) return list;

        list.AddRange(_handler.Owned);

        if (_tab == BlacksmithTab.Shop)
            foreach (var skill in _handler.Catalog)
                if (!_handler.Owns(skill)) list.Add(skill);

        return list;
    }

    BlacksmithRowMode ModeFor(SkillDefinition skill)
    {
        if (_tab == BlacksmithTab.Equip) return BlacksmithRowMode.Equip;
        return _handler.Owns(skill) ? BlacksmithRowMode.Upgrade : BlacksmithRowMode.Buy;
    }

    void RenderRows(bool animateEnter)
    {
        var listed = ListedSkills();

        while (_rows.Count < listed.Count)
            _rows.Add(Instantiate(rowPrefab, rowsContainer));

        BlacksmithSkillRowUI focusTarget = null;

        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            if (i >= listed.Count)
            {
                row.gameObject.SetActive(false);
                continue;
            }

            SetupRow(row, listed[i]);
            if (animateEnter) row.PlayEnter(i * rowEnterStagger);
            if (listed[i] == _focusedSkill) focusTarget = row;
        }

        if (focusTarget == null && listed.Count > 0) focusTarget = _rows[0];

        _focused = null;
        SetFocus(focusTarget, false);
    }

    void RefreshRows()
    {
        foreach (var row in _rows)
            if (row.gameObject.activeSelf && row.Skill != null)
                SetupRow(row, row.Skill);

        _focused?.SetFocused(true);
    }

    void SetupRow(BlacksmithSkillRowUI row, SkillDefinition skill)
    {
        int slot = _handler.IndexOf(skill);
        string key = slot >= 0 ? KeyLabel(slot) : null;

        switch (ModeFor(skill))
        {
            case BlacksmithRowMode.Buy:
                row.SetupBuy(skill, _handler.GetPurchaseCost(skill), _handler.CanBuy(skill, _stats), FocusRow, OnRowAction);
                break;

            case BlacksmithRowMode.Equip:
                row.SetupEquip(skill, _handler.GetLevel(skill), slot, SlotKeys(), _handler.UnlockedSlots, FocusRow, EquipRowInSlot);
                break;

            default:
                row.SetupUpgrade(skill, _handler.GetLevel(skill), _handler.GetUpgradeCost(skill), _handler.IsMaxLevel(skill),
                    _handler.CanUpgrade(skill, _stats), key, FocusRow, OnRowAction);
                break;
        }
    }

    void FocusRow(BlacksmithSkillRowUI row)
    {
        if (row == _focused) return;
        SetFocus(row, true);
    }

    void SetFocus(BlacksmithSkillRowUI row, bool animate)
    {
        if (_focused != null) _focused.SetFocused(false);

        _focused = row;
        _focusedSkill = row != null ? row.Skill : null;

        if (_focused != null) _focused.SetFocused(true);

        RenderSlots();
        RefreshDetail(animate);
    }

    void RefreshDetail(bool animate)
    {
        if (detail == null) return;

        int coins = _stats != null ? _stats.Coins : 0;
        var skill = _focusedSkill;

        if (skill == null || _handler == null)
        {
            var emptyMode = _tab == BlacksmithTab.Equip ? BlacksmithRowMode.Equip : BlacksmithRowMode.Upgrade;
            detail.Show(emptyMode, null, 0, 0, true, coins, true, animate);
            return;
        }

        var mode = ModeFor(skill);
        if (mode == BlacksmithRowMode.Buy)
            detail.Show(mode, skill, 0, _handler.GetPurchaseCost(skill), false, coins, _handler.CanBuy(skill, _stats), animate);
        else
            detail.Show(mode, skill, _handler.GetLevel(skill), _handler.GetUpgradeCost(skill), _handler.IsMaxLevel(skill),
                coins, _handler.CanUpgrade(skill, _stats), animate);
    }

    void OnRowAction(BlacksmithSkillRowUI row)
    {
        if (row == null || row.Skill == null || _handler == null) return;

        if (row.Mode == BlacksmithRowMode.Buy) BuyRow(row);
        else UpgradeRow(row);
    }

    void BuyRow(BlacksmithSkillRowUI row)
    {
        var skill = row.Skill;
        int cost = _handler.GetPurchaseCost(skill);
        if (!_handler.TryBuy(skill, _stats)) return;

        _focusedSkill = skill;
        RenderRows(false);
        _focused?.PlayUpgraded();

        if (detail != null)
        {
            detail.PlayUpgraded();
            detail.PlayWalletDelta(-cost);
        }

        RenderSlots();
        int equippedSlot = _handler.IndexOf(skill);
        if (equippedSlot >= 0 && equippedSlot < slots.Count && slots[equippedSlot] != null)
            slots[equippedSlot].PlayEquipped();
    }

    void EquipRowInSlot(BlacksmithSkillRowUI row, int slot)
    {
        if (row == null || row.Skill == null || _handler == null) return;

        if (row != _focused) SetFocus(row, false);
        if (!_handler.Equip(slot, row.Skill)) return;

        RefreshRows();
        RenderSlots();
        RefreshDetail(false);

        row.PlaySlotEquipped(slot);
        if (slot >= 0 && slot < slots.Count && slots[slot] != null)
            slots[slot].PlayEquipped();
    }

    void UpgradeRow(BlacksmithSkillRowUI row)
    {
        var skill = row.Skill;
        int cost = _handler.GetUpgradeCost(skill);
        if (!_handler.TryUpgrade(skill, _stats)) return;

        if (row != _focused) SetFocus(row, false);

        RefreshRows();
        row.PlayUpgraded();

        RefreshDetail(true);
        if (detail != null)
        {
            detail.PlayUpgraded();
            detail.PlayWalletDelta(-cost);
        }

        RenderSlots();
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
