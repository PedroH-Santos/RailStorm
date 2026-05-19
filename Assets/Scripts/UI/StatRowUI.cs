using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;
    public GameObject dividerObject;

    public void Setup(string label, string value)
    {
        if (dividerObject != null) dividerObject.SetActive(false);

        if (labelText != null)
        {
            Debug.Log($"Setting stat row label: {label}");
            labelText.gameObject.SetActive(true);
            labelText.text = label;
        }

        if (valueText != null)
        {
            Debug.Log($"Setting stat row value: {value}");
            valueText.gameObject.SetActive(true);
            valueText.text = value;
        }
    }

    public void SetAsDivider()
    {
        if (labelText != null) labelText.gameObject.SetActive(false);
        if (valueText != null) valueText.gameObject.SetActive(false);
        if (dividerObject != null) dividerObject.SetActive(true);
    }
}