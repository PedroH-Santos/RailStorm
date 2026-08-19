using System;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public TMP_Text levelText;
    public Image cardBackground;
    public Image cardBorder;

    [Header("Seleção")]
    [Tooltip("Container das cantoneiras que marcam o card sob o cursor. Fica oculto até o mouse entrar no card.")]
    public GameObject selectionBrackets;

    Action _onClick;
    Button _selfButton;

    void OnDisable() => SetSelected(false);

    public void OnPointerEnter(PointerEventData eventData) => SetSelected(true);

    public void OnPointerExit(PointerEventData eventData) => SetSelected(false);

    void SetSelected(bool selected)
    {
        if (selectionBrackets != null)
            selectionBrackets.SetActive(selected);
    }

    public void Setup(AbilityCardData data, Action onClick)
    {
        _onClick = onClick;
        SetSelected(false);

        IDrawable d = data.drawable;
        int ri = data.targetRarity;

        var theme = UIThemeConfig.Instance;
        theme?.ApplyTitle(nameText);
        theme?.ApplyBody(descriptionText);
        theme?.ApplyBody(rarityText);
        theme?.ApplyBody(levelText);
        if (theme != null && cardBorder != null) cardBorder.color = theme.panelBorder;

        nameText.text = d.DisplayName;

        if (iconImage != null && d.Icon != null)
            iconImage.sprite = d.Icon;

        if (rarityText != null) rarityText.text = RarityHelper.DisplayName(ri);
        if (cardBackground != null) cardBackground.color = RarityHelper.Color(ri);
        if (levelText != null) levelText.text = $"Nível {ri + 1}";

        if (d is SkillDefinition skill)
        {
            descriptionText.text = skill.description;
        }
        else if (d is WeaponDefinition weapon && !data.isUpgrade)
        {
            var stats = weapon.GetStatsForRarity(ri);
            descriptionText.text = !string.IsNullOrEmpty(weapon.description)
                ? weapon.description
                : $"DMG {stats.damage}  |  {stats.attackRate:F1}/s  |  Alc. {stats.range:F0}m";
        }
        else if (d is WeaponDefinition wu && data.isUpgrade)
        {
            var prev = wu.CurrentStats;
            var next = wu.NextStats;
            descriptionText.text = !string.IsNullOrEmpty(wu.description)
                ? wu.description
                : $"DMG {prev.damage}→{next.damage}" +
                  $"  |  {prev.attackRate:F1}→{next.attackRate:F1}/s" +
                  $"  |  Alc. {prev.range:F0}→{next.range:F0}m";
        }

        if (_selfButton == null) _selfButton = GetComponent<Button>();
        _selfButton.onClick.RemoveAllListeners();
        _selfButton.onClick.AddListener(() => _onClick?.Invoke());

    }



    public void OnPointerClick(PointerEventData e) => _onClick?.Invoke();
}