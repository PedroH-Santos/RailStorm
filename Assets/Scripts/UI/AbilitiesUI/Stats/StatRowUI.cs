using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;
    public GameObject dividerObject;

    public void Setup(string label, string value)
    {
        if (dividerObject != null) dividerObject.SetActive(false);

        var theme = UIThemeConfig.Instance;

        if (labelText != null)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;
            theme?.ApplyBody(labelText);
        }

        if (valueText != null)
        {
            valueText.gameObject.SetActive(true);
            valueText.text = value;
            theme?.ApplyTitle(valueText);
        }
    }

    public void SetAsDivider()
    {
        if (labelText != null) labelText.gameObject.SetActive(false);
        if (valueText != null) valueText.gameObject.SetActive(false);
        if (dividerObject != null) dividerObject.SetActive(true);
    }
}
