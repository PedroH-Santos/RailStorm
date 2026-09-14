using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;
    public GameObject dividerObject;

    [Tooltip("Desligue quando a cor/fonte das linhas já vem configurada no prefab (ex.: variante em madeira do baú).")]
    [SerializeField] private bool applyTheme = true;

    public void Setup(string label, string value)
    {
        if (dividerObject != null) dividerObject.SetActive(false);

        var theme = applyTheme ? UIThemeConfig.Instance : null;

        if (labelText != null)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;
            theme?.ApplyStatLabel(labelText);
        }

        if (valueText != null)
        {
            valueText.gameObject.SetActive(true);
            valueText.text = value;
            theme?.ApplyStatValue(valueText);
        }
    }

    public void SetAsDivider()
    {
        if (labelText != null) labelText.gameObject.SetActive(false);
        if (valueText != null) valueText.gameObject.SetActive(false);
        if (dividerObject != null) dividerObject.SetActive(true);
    }
}
