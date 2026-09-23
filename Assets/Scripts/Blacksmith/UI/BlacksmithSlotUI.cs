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
    public GameObject selectionBrackets;
    public TooltipTrigger tooltip;
    public CanvasGroup group;

    [Header("Textos")]
    public string emptyText = "Vazio";
    public string lockedText = "Bloqueado";

    [Range(0f, 1f)]
    [Tooltip("Opacidade do slot bloqueado.")]
    public float lockedAlpha = 0.55f;

    public int Index { get; private set; }

    Action<int> _onClick;
    bool _unlocked;

    public void Setup(int index, SkillDefinition skill, int level, bool unlocked, string key, bool selected, Action<int> onClick)
    {
        Index = index;
        _onClick = onClick;
        _unlocked = unlocked;

        bool hasSkill = unlocked && skill != null;

        if (iconImage != null)
        {
            iconImage.enabled = hasSkill && skill.icon != null;
            if (hasSkill) iconImage.sprite = skill.icon;
        }

        if (iconPlate != null)
        {
            var plate = RarityHelper.IconPlate(0);
            if (plate != null) iconPlate.sprite = plate;
            iconPlate.color = RarityHelper.Color(0);
        }

        if (iconGlow != null)
        {
            var glow = RarityHelper.IconGlow(0);
            iconGlow.enabled = hasSkill && glow != null;
            if (glow != null) iconGlow.sprite = glow;
            iconGlow.color = RarityHelper.GlowColor(0);
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
            if (hasSkill) tooltip.SetSource(skill, 0);
        }

        SetSelected(selected && unlocked);
    }

    public void SetSelected(bool selected)
    {
        if (selectionBrackets != null) selectionBrackets.SetActive(selected);
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
