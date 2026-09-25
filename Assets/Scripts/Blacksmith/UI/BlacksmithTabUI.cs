using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BlacksmithTab
{
    Shop,
    Equip,
}

public class BlacksmithTabUI : MonoBehaviour
{
    public BlacksmithTab tab;
    public Button button;
    public Image plate;
    public TMP_Text label;

    [Header("Cores")]
    public Color activeColor = new Color(0.737f, 0.384f, 0.106f, 1f);
    public Color inactiveColor = new Color(0.365f, 0.514f, 0.608f, 1f);
    [Range(0f, 1f)] public float inactiveLabelAlpha = 0.75f;

    [Header("Animação")]
    public float selectPunch = 0.1f;

    Action<BlacksmithTab> _onSelect;

    void Awake()
    {
        if (button != null) button.onClick.AddListener(() => _onSelect?.Invoke(tab));
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public void Bind(Action<BlacksmithTab> onSelect) => _onSelect = onSelect;

    public void SetActive(bool active, bool animate)
    {
        if (plate != null) plate.color = active ? activeColor : inactiveColor;
        if (label != null) label.alpha = active ? 1f : inactiveLabelAlpha;

        if (!animate || !active) return;

        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * selectPunch, 0.25f, 8, 0.6f).AsUI(gameObject);
    }
}
