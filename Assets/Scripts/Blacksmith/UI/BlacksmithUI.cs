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
                _instance = FindAnyObjectByType<BlacksmithUI>(FindObjectsInactive.Include);

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

    readonly struct RowEntry
    {
        public readonly SkillDefinition Skill;
        public readonly SkillVariantDefinition Variant;

        public RowEntry(SkillDefinition skill, SkillVariantDefinition variant)
        {
            Skill = skill;
            Variant = variant;
        }
    }

    readonly List<BlacksmithSkillRowUI> _rows = new();
    BlacksmithSkillRowUI _focused;
    SkillDefinition _focusedSkill;
    SkillVariantDefinition _focusedVariant;
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
            var slotView = slots[i];
            if (slotView == null) continue;

            var slot = _handler != null ? _handler.GetSlot(i) : null;
            slotView.gameObject.SetActive(slot != null);
            if (slot == null) continue;

            bool holdsFocusedSkill = slot.Holds(_focusedSkill);
            slotView.Setup(slot, KeyLabel(i), holdsFocusedSkill, OnInventorySlotClicked);
        }
    }

    List<string> SlotKeys()
    {
        var keys = new List<string>();
        int count = _handler != null ? _handler.Slots.Count : 0;
        for (int i = 0; i < count; i++) keys.Add(KeyLabel(i));
        return keys;
    }

    void OnInventorySlotClicked(int slotIndex)
    {
        if (_handler == null) return;

        var slot = _handler.GetSlot(slotIndex);
        if (slot != null && !slot.IsEmpty) _focusedSkill = slot.Skill.Definition;

        if (_tab != BlacksmithTab.Equip)
        {
            SelectTab(BlacksmithTab.Equip);
            return;
        }

        RenderRows(false);
    }

    List<RowEntry> ListedEntries()
    {
        var list = new List<RowEntry>();
        if (_handler == null) return list;

        if (_tab == BlacksmithTab.Runes)
        {
            foreach (var owned in _handler.Owned)
                foreach (var variant in owned.Definition.variants)
                    if (variant != null) list.Add(new RowEntry(owned.Definition, variant));

            return list;
        }

        foreach (var owned in _handler.Owned)
            list.Add(new RowEntry(owned.Definition, null));

        if (_tab == BlacksmithTab.Shop)
            foreach (var skill in _handler.Catalog)
                if (!_handler.Owns(skill)) list.Add(new RowEntry(skill, null));

        return list;
    }

    BlacksmithRowMode ModeFor(SkillDefinition skill)
    {
        if (_tab == BlacksmithTab.Runes) return BlacksmithRowMode.Rune;
        if (_tab == BlacksmithTab.Equip) return BlacksmithRowMode.Equip;
        return _handler.Owns(skill) ? BlacksmithRowMode.Upgrade : BlacksmithRowMode.Buy;
    }

    void RenderRows(bool animateEnter)
    {
        var listed = ListedEntries();

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
            if (focusTarget == null && listed[i].Skill == _focusedSkill) focusTarget = row;
        }

        if (focusTarget == null && listed.Count > 0) focusTarget = _rows[0];

        _focused = null;
        SetFocus(focusTarget, false);
    }

    void RefreshRows()
    {
        foreach (var row in _rows)
            if (row.gameObject.activeSelf && row.Skill != null)
                SetupRow(row, new RowEntry(row.Skill, row.Variant));

        _focused?.SetFocused(true);
    }

    void SetupRow(BlacksmithSkillRowUI row, RowEntry entry)
    {
        var owned = _handler.FindOwned(entry.Skill);

        if (entry.Variant != null)
        {
            row.SetupRune(owned, entry.Variant, _handler.IsVariantUnlocked(entry.Variant), _handler.GetVariantCost(entry.Variant),
                _handler.CanActivateVariant(owned, entry.Variant, _stats), FocusRow, OnRowAction);
            return;
        }

        var equippedSlot = _handler.SlotHolding(entry.Skill);
        int equippedIndex = equippedSlot != null ? equippedSlot.Index : -1;
        string equippedKey = equippedSlot != null ? KeyLabel(equippedIndex) : null;

        switch (ModeFor(entry.Skill))
        {
            case BlacksmithRowMode.Buy:
                row.SetupBuy(entry.Skill, _handler.CanBuy(entry.Skill, _stats), FocusRow, OnRowAction);
                break;

            case BlacksmithRowMode.Equip:
                row.SetupEquip(owned, equippedIndex, SlotKeys(), _handler.UnlockedSlotCount, FocusRow, EquipRowInSlot);
                break;

            default:
                row.SetupUpgrade(owned, _handler.CanUpgrade(owned, _stats), equippedKey, FocusRow, OnRowAction);
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
        _focusedVariant = row != null ? row.Variant : null;

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
            detail.ShowNothing(_tab == BlacksmithTab.Shop, coins);
            return;
        }

        var owned = _handler.FindOwned(skill);

        if (_focusedVariant != null)
        {
            detail.ShowVariant(owned, _focusedVariant, _handler.IsVariantUnlocked(_focusedVariant),
                _handler.GetVariantCost(_focusedVariant), _handler.CanActivateVariant(owned, _focusedVariant, _stats), animate);
            return;
        }

        switch (ModeFor(skill))
        {
            case BlacksmithRowMode.Buy:
                detail.ShowForSale(skill, coins, _handler.CanBuy(skill, _stats), animate);
                break;

            case BlacksmithRowMode.Equip:
                detail.ShowForEquip(owned, animate);
                break;

            default:
                detail.ShowForUpgrade(owned, coins, _handler.CanUpgrade(owned, _stats), animate);
                break;
        }
    }

    void OnRowAction(BlacksmithSkillRowUI row)
    {
        if (row == null || row.Skill == null || _handler == null) return;

        if (row.Mode == BlacksmithRowMode.Rune) ToggleVariantRow(row);
        else if (row.Mode == BlacksmithRowMode.Buy) BuyRow(row);
        else UpgradeRow(row);
    }

    void ToggleVariantRow(BlacksmithSkillRowUI row)
    {
        var owned = _handler.FindOwned(row.Skill);
        var variant = row.Variant;
        if (owned == null || variant == null) return;

        bool changed = owned.IsUsing(variant)
            ? _handler.TryUseBaseForm(owned)
            : _handler.TryActivateVariant(owned, variant, _stats);
        if (!changed) return;

        if (row != _focused) SetFocus(row, false);

        RefreshRows();
        row.PlayUpgraded();

        RefreshDetail(true);
        if (detail != null)
        {
            detail.PlayUpgraded();
        }
    }

    void BuyRow(BlacksmithSkillRowUI row)
    {
        var skill = row.Skill;
        int cost = skill.purchaseCost;
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
        var equippedSlot = _handler.SlotHolding(skill);
        if (equippedSlot != null) PlaySlotEquipped(equippedSlot.Index);
    }

    void PlaySlotEquipped(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex] != null)
            slots[slotIndex].PlayEquipped();
    }

    void EquipRowInSlot(BlacksmithSkillRowUI row, int slot)
    {
        if (row == null || row.Skill == null || _handler == null) return;

        if (row != _focused) SetFocus(row, false);
        if (!_handler.Equip(slot, _handler.FindOwned(row.Skill))) return;

        RefreshRows();
        RenderSlots();
        RefreshDetail(false);

        row.PlaySlotEquipped(slot);
        PlaySlotEquipped(slot);
    }

    void UpgradeRow(BlacksmithSkillRowUI row)
    {
        var owned = _handler.FindOwned(row.Skill);
        if (owned == null) return;

        int cost = owned.UpgradeCost;
        if (!_handler.TryUpgrade(owned, _stats)) return;

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
