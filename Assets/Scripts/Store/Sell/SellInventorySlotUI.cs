using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellInventorySlotUI : MonoBehaviour
{
    public Image icon;
    public Image rarityBackground;
    public Button selectButton;
    public TMP_Text priceText;

    [Range(0f, 1f)] public float selectedWhiteBlend = 0.35f;

    public ItemDefinition Item { get; private set; }
    public bool IsSelected { get; private set; }

    Action<ItemDefinition> _onToggle;
    Color _baseRarityColor;

    public void Setup(ItemDefinition item, int sellPrice, bool isSelected, Action<ItemDefinition> onToggle)
    {
        Item = item;
        _onToggle = onToggle;
        IsSelected = isSelected;

        gameObject.SetActive(true);

        if (icon != null) icon.sprite = item.icon;
        if (priceText != null) priceText.text = sellPrice.ToString();

        if (rarityBackground != null)
        {
            rarityBackground.gameObject.SetActive(true);
            _baseRarityColor = RarityHelper.Color(item.rarity);
            ApplySelectionColor();
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onToggle?.Invoke(Item));
        }
    }

    void ApplySelectionColor()
    {
        rarityBackground.color = IsSelected
            ? Color.Lerp(_baseRarityColor, Color.white, selectedWhiteBlend)
            : _baseRarityColor;
    }

    public void Clear()
    {
        Item = null;
        IsSelected = false;
        gameObject.SetActive(false);
    }
}
