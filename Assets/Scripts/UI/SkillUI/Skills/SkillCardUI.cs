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

        IDrawable d = data.drawable;

        skillNameText.text = d.DisplayName;

        if (iconImage != null && d.Icon != null)
            iconImage.sprite = d.Icon;

        if (rarityText != null) rarityText.text = GetRarityLabel(d.Rarity);
        if (cardBackground != null) cardBackground.color = GetRarityColor(d.Rarity);

        if (d is SkillDefinition skill)
        {
            descriptionText.text = skill.GetLevel(data.targetLevel).description;

            int current = data.targetLevel - 1;
            levelText.text = current == 0
                ? "Novo"
                : $"Lv {current} → {data.targetLevel}";
        }
        else if (d is WeaponDefinition weapon)
        {
            descriptionText.text = weapon.description != ""
                ? weapon.description
                : $"DMG {weapon.damage}  |  {weapon.fireRate:F1}/s  |  Alc. {weapon.range:F0}m";

            levelText.text = "Arma do Carro";
        }

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


    string GetRarityLabel(ESkillRarity rarity) => rarity switch
    {
        ESkillRarity.Common => "Comum",
        ESkillRarity.Uncommon => "Incomum",
        ESkillRarity.Rare => "Raro",
        _ => "Desconhecida"
    };

    Color GetRarityColor(ESkillRarity rarity) => rarity switch
    {
        ESkillRarity.Common => colorCommon,
        ESkillRarity.Uncommon => colorUncommon,
        ESkillRarity.Rare => colorRare,
        _ => colorCommon
    };

    public void OnPointerClick(PointerEventData eventData) => _onClick?.Invoke();
}