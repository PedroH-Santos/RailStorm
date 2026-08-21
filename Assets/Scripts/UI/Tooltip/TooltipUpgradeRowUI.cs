using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUpgradeRowUI : MonoBehaviour
{
    public Image bullet;
    public TMP_Text nameText;
    public TMP_Text effectText;
    public TMP_Text levelText;

    public void Setup(TooltipUpgradeLine line)
    {
        var theme = UIThemeConfig.Instance;

        if (bullet != null && theme != null)
            bullet.color = theme.panelBorder;

        if (nameText != null)
        {
            nameText.text = line.Name;
            theme?.ApplyTitle(nameText);
        }

        if (effectText != null)
        {
            effectText.text = line.Effect;
            theme?.ApplyBody(effectText);
        }

        if (levelText != null)
        {
            levelText.text = line.LevelLabel;
            theme?.ApplyBody(levelText);
        }
    }
}
