using System;
using UnityEngine;
using UnityEngine.UI;

public class SellInventorySlotUI : MonoBehaviour
{
    public Image icon;
    public Button selectButton;
    public GameObject selectedBackground;

    public ItemDefinition Item { get; private set; }
    public bool IsSelected { get; private set; }

    Action<ItemDefinition> _onToggle;

    public void Setup(ItemDefinition item, bool isSelected, Action<ItemDefinition> onToggle)
    {
        Item = item;
        _onToggle = onToggle;
        IsSelected = isSelected;

        gameObject.SetActive(true);

        if (icon != null) icon.sprite = item.icon;

        if (selectedBackground != null)
            selectedBackground.SetActive(isSelected);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onToggle?.Invoke(Item));
        }
    }

    public void Clear()
    {
        Item = null;
        IsSelected = false;
        if (selectedBackground != null) selectedBackground.SetActive(false);
        gameObject.SetActive(false);
    }
}
