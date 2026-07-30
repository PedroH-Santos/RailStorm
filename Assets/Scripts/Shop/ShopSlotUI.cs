using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public TMP_Text rarityText;
    public Image rarityBorder;   
    public Button selectButton;
    public GameObject selectedFrame; 

    public ItemDefinition Item { get; private set; }
    public bool IsSelected { get; private set; }

    Action<ItemDefinition> _onToggle;
    bool _locked; 
    public void Setup(ItemDefinition item, PlayerStatsAggregator stats, PlayerItemHandler itemHandler, Action<ItemDefinition> onToggle)
    {
        Item = item;
        _onToggle = onToggle;

        gameObject.SetActive(true);

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (priceText != null) priceText.text = item.price.ToString();

        Color rarityColor = RarityHelper.Color(item.rarity);

        if (rarityText != null)
        {
            rarityText.text = RarityHelper.DisplayName(item.rarity);
            rarityText.color = rarityColor;
        }

        if (rarityBorder != null)
            rarityBorder.color = rarityColor;

        bool alreadyOwned = itemHandler != null && itemHandler.HasItem(item);
        bool affordableAlone = stats != null && stats.Coins >= item.price;
        _locked = alreadyOwned || !affordableAlone;

        if (selectButton != null)
        {
            selectButton.interactable = !_locked;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(HandleClicked);
        }

        SetSelected(false);
    }

    void HandleClicked()
    {
        if (_locked) return;
        _onToggle?.Invoke(Item);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectedFrame != null) selectedFrame.SetActive(selected);
    }

    public void Clear()
    {
        Item = null;
        IsSelected = false;
        gameObject.SetActive(false);
    }
}