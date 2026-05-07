using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text levelText;
    public TMP_Text nameText;
    public GameObject lockedOverlay; 

    public void Setup(SkillDefinition skill, int currentLevel)
    {
        if (lockedOverlay != null) lockedOverlay.SetActive(false);

        if (iconImage != null && skill.icon != null)
            iconImage.sprite = skill.icon;

        if (levelText != null)
            levelText.text = $"Lv{currentLevel}";

        if (nameText != null)
            nameText.text = skill.skillName;
    }

    public void SetLocked()
    {
        if (lockedOverlay != null) lockedOverlay.SetActive(true);
        if (iconImage != null) iconImage.sprite = null;
        if (levelText != null) levelText.text = "";
        if (nameText != null) nameText.text = "";
    }
}