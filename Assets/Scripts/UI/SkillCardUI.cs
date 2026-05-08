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
    public GameObject selectedBorder;  // outline ou painel que aparece ao selecionar
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
            rarityText.text = GetRarityLabel(data.skill.weight);

        if (data.skill.icon != null && iconImage != null)
            iconImage.sprite = data.skill.icon;

        if (cardBackground != null)
            cardBackground.color = GetRarityColor(data.skill.weight);

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

    string GetRarityLabel(float weight)
    {
        if (weight >= 0.7f) return "Comum";
        if (weight >= 0.4f) return "Incomum";
        return "Raro";
    }

    Color GetRarityColor(float weight)
    {
        if (weight >= 0.7f) return colorCommon;
        if (weight >= 0.4f) return colorUncommon;
        return colorRare;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();   
    }
}