using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;
    public GameObject dividerObject;

    [Tooltip("Mantém fonte, cor e material dos textos como estão no prefab, sem aplicar o estilo padrão de estatística do tema.")]
    public bool keepSceneTextStyle;

    [Tooltip("Força do pulo do valor quando ele muda com a tela aberta.")]
    public float valueChangePunch = 0.25f;

    public void Setup(string label, string value)
    {
        if (dividerObject != null) dividerObject.SetActive(false);

        var theme = keepSceneTextStyle ? null : UIThemeConfig.Instance;

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

    public void SetValue(string value)
    {
        if (valueText == null || valueText.text == value) return;

        valueText.text = value;

        var target = valueText.transform;
        target.DOKill(true);
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * valueChangePunch, 0.3f, 6, 0.5f).AsUI(valueText.gameObject);
    }

    public void SetAsDivider()
    {
        if (labelText != null) labelText.gameObject.SetActive(false);
        if (valueText != null) valueText.gameObject.SetActive(false);
        if (dividerObject != null) dividerObject.SetActive(true);
    }
}
