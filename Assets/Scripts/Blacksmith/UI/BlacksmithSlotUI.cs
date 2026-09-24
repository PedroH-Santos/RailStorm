using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlacksmithSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public Image iconPlate;
    public Image iconGlow;
    public TMP_Text levelLabel;
    public TMP_Text keyText;
    public GameObject keyBadge;
    public GameObject lockIcon;
    public Image slotPlate;
    public TooltipTrigger tooltip;
    public CanvasGroup group;

    [Header("Textos")]
    public string emptyText = "Vazio";
    public string lockedText = "Bloqueado";

    [Range(0f, 1f)]
    [Tooltip("Opacidade do slot bloqueado.")]
    public float lockedAlpha = 0.55f;

    [Header("Slot alvo")]
    [Tooltip("Cor da bandeja quando o slot não é o alvo do EQUIPAR.")]
    public Color normalPlate = new Color(0.047f, 0.133f, 0.220f, 1f);

    [Tooltip("Cor da bandeja do slot que vai receber a skill ao clicar em EQUIPAR.")]
    public Color selectedPlate = new Color(0.325f, 0.192f, 0.078f, 1f);

    public int Index { get; private set; }

    Action<int> _onClick;
    bool _unlocked;

    public void Setup(int index, SkillDefinition skill, int level, bool unlocked, string key, bool selected, Action<int> onClick)
    {
        Index = index;
        _onClick = onClick;
        _unlocked = unlocked;

        bool hasSkill = unlocked && skill != null;
        int rarity = hasSkill ? skill.RarityForLevel(level) : 0;

        if (iconImage != null)
        {
            iconImage.enabled = hasSkill && skill.icon != null;
            if (hasSkill) iconImage.sprite = skill.icon;
        }

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(rarity);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = RarityHelper.Color(rarity);
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(rarity);
            iconGlow.enabled = hasSkill && glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(rarity);
        }

        if (levelLabel != null)
            levelLabel.text = !unlocked ? lockedText : hasSkill ? $"LVL {level + 1}" : emptyText;

        if (keyText != null) keyText.text = key;
        if (keyBadge != null) keyBadge.SetActive(unlocked);
        if (lockIcon != null) lockIcon.SetActive(!unlocked);
        if (group != null) group.alpha = unlocked ? 1f : lockedAlpha;

        if (tooltip != null)
        {
            tooltip.enabled = hasSkill;
            if (hasSkill) tooltip.SetSource(skill, rarity);
        }

        SetSelected(selected && unlocked);
    }

    public void SetSelected(bool selected)
    {
        if (slotPlate != null) slotPlate.color = selected ? selectedPlate : normalPlate;
    }

    public void PlayEquipped()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.18f, 0.35f, 6, 0.5f).AsUI(gameObject);
    }

    public void PlayDenied()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOShakeRotation(0.25f, new Vector3(0f, 0f, 8f), 18, 90f, false).AsUI(gameObject)
            .OnComplete(() => transform.localRotation = Quaternion.identity);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_unlocked)
        {
            PlayDenied();
            return;
        }

        _onClick?.Invoke(Index);
    }
}
