using Assets.Scripts.Systems.Rarity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityCardUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public GameObject selectedBorder;
    public Image cardBackground;

    Action _onClick;
    Button _selfButton;

    public void Setup(AbilityCardData data, Action onClick)
    {
        _onClick = onClick;

        IDrawable d = data.drawable;
        int ri = data.targetRarity;

        nameText.text = d.DisplayName;

        if (iconImage != null && d.Icon != null)
            iconImage.sprite = d.Icon;

        if (rarityText != null) rarityText.text = RarityHelper.DisplayName(ri);
        if (cardBackground != null) cardBackground.color = RarityHelper.Color(ri);

        if (d is SkillDefinition skill)
        {
            descriptionText.text = skill.description;
        }
        else if (d is WeaponDefinition weapon && !data.isUpgrade)
        {
            var stats = weapon.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(weapon.description)
                ? weapon.description
                : $"DMG {stats.damage}  |  {stats.fireRate:F1}/s  |  Alc. {stats.range:F0}m";
        }
        else if (d is WeaponDefinition wu && data.isUpgrade)
        {
            var prev = wu.CurrentStats;
            var next = wu.NextStats;
            descriptionText.text = !string.IsNullOrEmpty(wu.description)
                ? wu.description
                : $"DMG {prev.damage}→{next.damage}" +
                  $"  |  {prev.fireRate:F1}→{next.fireRate:F1}/s" +
                  $"  |  Alc. {prev.range:F0}→{next.range:F0}m";
        }

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
