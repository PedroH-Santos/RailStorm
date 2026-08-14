using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellCartItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;

    public ItemDefinition Item { get; private set; }

    public void Setup(ItemDefinition item, int sellPrice)
    {
        Item = item;

        gameObject.SetActive(true);

        if (icon != null) icon.sprite = item.icon;
        if (priceText != null) priceText.text = sellPrice.ToString();
    }

    public void Clear()
    {
        Item = null;
        gameObject.SetActive(false);
    }
}
