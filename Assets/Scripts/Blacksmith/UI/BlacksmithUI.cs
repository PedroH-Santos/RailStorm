using System.Collections.Generic;
using DG.Tweening;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithUI : MonoBehaviour
{
    [Header("Tela")]
    public GameObject root;
    public Image dimBackground;
    public CanvasGroup panel;

    [Header("Slots")]
    public List<BlacksmithSlotUI> slots = new();

    [Header("Lista")]
    public Transform rowsContainer;
    public BlacksmithSkillRowUI rowPrefab;
    public ScrollRect scrollRect;
    public float rowEnterStagger = 0.05f;

    [Header("Detalhe")]
    public BlacksmithDetailUI detail;
    public string equipFormat = "EQUIPAR EM {0}";
    public string equippedFormat = "EQUIPADA EM {0}";

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
    int _targetSlot;
    float _dimAlpha;
    Sequence _closeSequence;

    void Awake()
    {
        if (dimBackground != null) _dimAlpha = dimBackground.color.a;

        if (rowsContainer != null)
            for (int i = rowsContainer.childCount - 1; i >= 0; i--)
                Destroy(rowsContainer.GetChild(i).gameObject);

        if (detail != null && detail.equipButton != null)
            detail.equipButton.onClick.AddListener(EquipFocused);
    }

    public void Open(PlayerStatsAggregator stats, PlayerSkillHandler handler, PlayerSkillCaster caster, System.Action onBack)
    {
        _stats = stats;
        _handler = handler;
        _caster = caster;
        _focusedSkill = null;
        _targetSlot = DefaultTargetSlot();

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

    int DefaultTargetSlot()
    {
        if (_handler == null) return 0;

        for (int i = 0; i < _handler.UnlockedSlots; i++)
            if (_handler.GetSlot(i) == null) return i;

        return 0;
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
            slot.Setup(i, skill, _handler.GetLevel(skill), _handler.IsSlotUnlocked(i), KeyLabel(i), i == _targetSlot, SelectTargetSlot);
        }
    }

    void SelectTargetSlot(int slot)
    {
        if (_handler == null || !_handler.IsSlotUnlocked(slot)) return;

        _targetSlot = slot;
        RenderSlots();
        RefreshDetail(false);
    }

    void RenderRows(bool animateEnter)
    {
        var owned = _handler != null ? _handler.Owned : (IReadOnlyList<SkillDefinition>)System.Array.Empty<SkillDefinition>();

        while (_rows.Count < owned.Count)
            _rows.Add(Instantiate(rowPrefab, rowsContainer));

        BlacksmithSkillRowUI focusTarget = null;

        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            if (i >= owned.Count)
            {
                row.gameObject.SetActive(false);
                continue;
            }

            SetupRow(row, owned[i]);
            if (animateEnter) row.PlayEnter(i * rowEnterStagger);
            if (owned[i] == _focusedSkill) focusTarget = row;
        }

        if (focusTarget == null && owned.Count > 0) focusTarget = _rows[0];

        _focused = null;
        SetFocus(focusTarget, false);
    }

    void SetupRow(BlacksmithSkillRowUI row, SkillDefinition skill)
    {
        int slot = _handler.IndexOf(skill);
        row.Setup(
            skill,
            _handler.GetLevel(skill),
            _handler.GetUpgradeCost(skill),
            _handler.IsMaxLevel(skill),
            _handler.CanUpgrade(skill, _stats),
            slot >= 0 ? KeyLabel(slot) : null,
            FocusRow,
            UpgradeRow);
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

        RefreshDetail(animate);
    }

    void RefreshDetail(bool animate)
    {
        if (detail == null) return;

        int coins = _stats != null ? _stats.Coins : 0;
        var skill = _focusedSkill;

        if (skill == null || _handler == null)
        {
            detail.Show(null, 0, 0, true, coins, true, animate);
            return;
        }

        detail.Show(skill, _handler.GetLevel(skill), _handler.GetUpgradeCost(skill), _handler.IsMaxLevel(skill),
            coins, _handler.CanUpgrade(skill, _stats), animate);

        bool alreadyThere = _handler.GetSlot(_targetSlot) == skill;
        string key = KeyLabel(_targetSlot);
        detail.SetEquip(string.Format(alreadyThere ? equippedFormat : equipFormat, key),
            !alreadyThere && _handler.IsSlotUnlocked(_targetSlot));
    }

    void UpgradeRow(BlacksmithSkillRowUI row)
    {
        if (row == null || row.Skill == null || _handler == null) return;

        var skill = row.Skill;
        int cost = _handler.GetUpgradeCost(skill);
        if (!_handler.TryUpgrade(skill, _stats)) return;

        if (row != _focused) SetFocus(row, false);

        foreach (var other in _rows)
            if (other.gameObject.activeSelf && other.Skill != null)
                SetupRow(other, other.Skill);

        _focused?.SetFocused(true);
        row.PlayUpgraded();

        RefreshDetail(true);
        if (detail != null)
        {
            detail.PlayUpgraded();
            detail.PlayWalletDelta(-cost);
        }

        RenderSlots();
    }

    void EquipFocused()
    {
        if (_handler == null || _focusedSkill == null) return;
        if (!_handler.Equip(_targetSlot, _focusedSkill)) return;

        RenderSlots();
        if (_targetSlot >= 0 && _targetSlot < slots.Count && slots[_targetSlot] != null)
            slots[_targetSlot].PlayEquipped();

        foreach (var other in _rows)
            if (other.gameObject.activeSelf && other.Skill != null)
                SetupRow(other, other.Skill);

        _focused?.SetFocused(true);
        RefreshDetail(false);
        detail?.PlayEquipped();
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
