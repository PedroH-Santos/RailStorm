// SkillCardUI.cs
using Assets.Scripts.Systems.Rarity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour
{
    [Header("Referências")]
    public Image iconImage;
    public TMP_Text skillNameText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text rarityText;

    [Header("Seleção")]
    public GameObject selectedBorder;
    public Image cardBackground;

    Action _onClick;
    Button _selfButton;

    public void Setup(SkillCardData data, Action onClick)
    {
        _onClick = onClick;

        IDrawable d = data.drawable;
        int ri = data.targetRarityHelper;

        // ── Nome e ícone ──────────────────────────────────────────────────────
        skillNameText.text = d.DisplayName;

        if (iconImage != null && d.Icon != null)
            iconImage.sprite = d.Icon;

        // ── Raridade — tudo via RarityConfig, sem hardcode ───────────────────
        if (rarityText != null) rarityText.text = RarityHelper.DisplayName(ri);
        if (cardBackground != null) cardBackground.color = RarityHelper.Color(ri);

        // ── Conteúdo por tipo ─────────────────────────────────────────────────
        if (d is SkillDefinition skill)
        {
            descriptionText.text = skill.GetLevelForRarity(ri).description;

            // Descobre a raridade atual da skill para mostrar a progressão
            var handler = FindFirstObjectByType<StarterAssets.PlayerSkillHandler>();
            int currentRi = handler != null ? handler.GetSkillRarityHelper(skill) : -1;

            levelText.text = currentRi < 0
                ? $"Novo  •  {RarityHelper.DisplayName(ri)}"
                : $"{RarityHelper.DisplayName(currentRi)} → {RarityHelper.DisplayName(ri)}";
        }
        else if (d is WeaponDefinition weapon && !data.isWeaponUpgrade)
        {
            var stats = weapon.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(stats.description)
                ? stats.description
                : $"DMG {stats.damage}  |  {stats.fireRate:F1}/s  |  Alc. {stats.range:F0}m";

            levelText.text = $"Nova arma  •  {RarityHelper.DisplayName(ri)}";
        }
        else if (d is WeaponDefinition wu && data.isWeaponUpgrade)
        {
            var prev = wu.CurrentStats;
            var next = wu.NextStats;
            int prevRi = wu.RarityHelper;

            descriptionText.text = !string.IsNullOrEmpty(next.description)
                ? next.description
                : $"DMG {prev.damage}→{next.damage}" +
                  $"  |  {prev.fireRate:F1}→{next.fireRate:F1}/s" +
                  $"  |  Alc. {prev.range:F0}→{next.range:F0}m";

            levelText.text = $"{RarityHelper.DisplayName(prevRi)} → {RarityHelper.DisplayName(ri)}";
        }

        // ── Botão ─────────────────────────────────────────────────────────────
        if (_selfButton == null) _selfButton = GetComponent<Button>();
        _selfButton.onClick.RemoveAllListeners();
        _selfButton.onClick.AddListener(() => _onClick?.Invoke());

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null) selectedBorder.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData e) => _onClick?.Invoke();
}