using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipStatRowUI : MonoBehaviour
{
    public Image bullet;
    public TMP_Text labelText;
    public TMP_Text valueText;

    public void Setup(TooltipStatLine line)
    {
        var theme = UIThemeConfig.Instance;

        if (bullet != null && theme != null)
            bullet.color = theme.panelBorder;

        if (labelText != null)
        {
            labelText.text = line.Label;
            theme?.ApplyBody(labelText);
        }

        if (valueText != null)
        {
            valueText.text = line.Value;
            theme?.ApplyTitle(valueText);
        }
    }
}
