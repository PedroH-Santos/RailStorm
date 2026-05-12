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

    [Header("Cores de raridade")]
    public Color colorCommon = new Color(0.91f, 0.96f, 0.87f);
    public Color colorUncommon = new Color(0.90f, 0.95f, 0.98f);
    public Color colorRare = new Color(0.93f, 0.93f, 0.99f);

    Action _onClick;
    Button _selfButton;

    public void Setup(SkillCardData data, Action onClick)
    {
        _onClick = onClick;

        skillNameText.text = data.skill.skillName;
        descriptionText.text = data.skill.GetLevel(data.targetLevel).description;

        int current = data.targetLevel - 1;
        levelText.text = current == 0
            ? "Novo"
            : $"Lv {current} → {data.targetLevel}";

        if (rarityText != null)
            rarityText.text = GetRarityLabel(data.skill.rarity);

        if (data.skill.icon != null && iconImage != null)
            iconImage.sprite = data.skill.icon;

        if (cardBackground != null)
            cardBackground.color = GetRarityColor(data.skill.rarity);

        if (_selfButton == null)
            _selfButton = GetComponent<Button>();

        _selfButton.onClick.RemoveAllListeners();
        _selfButton.onClick.AddListener(() => _onClick?.Invoke());

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

    string GetRarityLabel(SkillRarity rarity)
    {
        switch (rarity)
        {
            case SkillRarity.Common: return "Comum";
            case SkillRarity.Uncommon: return "Incomum";
            case SkillRarity.Rare: return "Raro";
            default: return "Desconhecida";
        }
    }

    Color GetRarityColor(SkillRarity rarity)
    {
        switch (rarity)
        {
            case SkillRarity.Common: return colorCommon;
            case SkillRarity.Uncommon: return colorUncommon;
            case SkillRarity.Rare: return colorRare;
            default: return colorCommon;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();   
    }
}