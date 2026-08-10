using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellCartItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;
    public Button removeButton;

    public ItemDefinition Item { get; private set; }

    Action _onRemove;

    public void Setup(ItemDefinition item, int sellPrice, Action onRemove)
    {
        Item = item;
        _onRemove = onRemove;

        gameObject.SetActive(true);

        if (icon != null) icon.sprite = item.icon;
        if (priceText != null) priceText.text = sellPrice.ToString();

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => _onRemove?.Invoke());
        }
    }

    public void Clear()
    {
        Item = null;
        gameObject.SetActive(false);
    }
}
