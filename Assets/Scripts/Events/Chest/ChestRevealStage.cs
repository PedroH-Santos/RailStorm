using System;
using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChestRevealStage : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private RectTransform slot;
    [SerializeField] private Image plate;
    [SerializeField] private Image pattern;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text rarityText;

    [Header("Fundo")]
    [SerializeField] private Image backdrop;
    [SerializeField] private Image rays;
    [SerializeField] private Image glow;
    [SerializeField] private Image ring;
    [SerializeField] private Image shockwave;
    [SerializeField] private ChestSparksEmitter sparks;

    [Header("Card")]
    [SerializeField] private Image cardFill;
    [SerializeField] private Image cardBorder;
    [SerializeField] private GameObject selectionBrackets;

    [Header("Roleta")]
    [SerializeField] private float tickStart = 0.04f;
    [SerializeField] private float tickGrowth = 1.16f;
    [SerializeField] private float tickEnd = 0.32f;

    [Header("Intensidade")]
    [SerializeField, Range(0f, 1f)] private float raysAlpha = 0.22f;
    [SerializeField] private float spinBoost = 9f;

    Tween _raysSpin;
    Tween _ringSpin;
    Sequence _roulette;
    Vector2 _iconRest;

    void Awake() => _iconRest = icon.rectTransform.anchoredPosition;

    void OnEnable()
    {
        _raysSpin = rays.rectTransform.DOLocalRotate(new Vector3(0f, 0f, -360f), 16f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear).SetLoops(-1).AsUI(gameObject);
        _ringSpin = ring.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 360f), 22f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear).SetLoops(-1).AsUI(gameObject);

        glow.rectTransform.localScale = Vector3.one;
        glow.rectTransform.DOScale(1.06f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).AsUI(gameObject);

        shockwave.gameObject.SetActive(false);
    }

    public void PlayRoulette(ItemDefinition result, IReadOnlyList<ItemDefinition> pool, Action onLanded)
    {
        selectionBrackets.SetActive(false);
        sparks.SetSpinning(true);

        _roulette?.Kill();
        _roulette = DOTween.Sequence().AsUI(gameObject);

        ItemDefinition previous = null;
        for (float interval = tickStart; interval < tickEnd; interval *= tickGrowth)
        {
            var item = PickDifferent(pool, previous) ?? result;
            float speed = 1f - Mathf.InverseLerp(tickStart, tickEnd, interval);
            float duration = interval;
            _roulette.AppendCallback(() => Tick(item, speed, duration));
            _roulette.AppendInterval(interval);
            previous = item;
        }

        _roulette.AppendCallback(() => Land(result));
        _roulette.AppendInterval(0.15f);
        _roulette.OnComplete(() => onLanded?.Invoke());
    }

    public void PlayBurst(float punch)
    {
        Punch(slot, punch, 0.32f);
        sparks.Burst(16);

        var wave = shockwave.rectTransform;
        wave.DOKill();
        shockwave.DOKill();
        shockwave.gameObject.SetActive(true);
        wave.localScale = Vector3.one * 0.6f;
        shockwave.color = WithAlpha(glow.color, 1f);
        wave.DOScale(2.2f, 0.45f).SetEase(Ease.OutCubic).AsUI(gameObject);
        shockwave.DOFade(0f, 0.45f).AsUI(gameObject).OnComplete(() => shockwave.gameObject.SetActive(false));
    }

    public void Paint(Color main, Color light, float blend)
    {
        var theme = UIThemeConfig.Instance;
        Color recess = theme != null ? theme.panelRecess : Color.black;
        Color panel = theme != null ? theme.panelBackground : Color.black;

        Tint(backdrop, Color.Lerp(recess, Shade(main, 0.35f), 0.45f), blend);
        Tint(cardFill, Color.Lerp(panel, Shade(main, 0.3f), 0.7f), blend);
        Tint(cardBorder, main, blend);
        Tint(ring, WithAlpha(main, 0.55f), blend);
        Tint(rays, WithAlpha(light, raysAlpha), blend);
        Tint(glow, WithAlpha(light, 0.8f), blend);
        sparks.Color = light;
    }

    void Tick(ItemDefinition item, float speed, float duration)
    {
        ShowItem(item, Mathf.Min(duration, 0.08f));
        SetSpinSpeed(speed);

        var iconRect = icon.rectTransform;
        iconRect.DOKill(true);
        iconRect.anchoredPosition = _iconRest + Vector2.up * 48f;
        iconRect.DOAnchorPos(_iconRest, duration * 0.8f).SetEase(Ease.OutQuad).AsUI(gameObject);

        Punch(slot, 0.1f, Mathf.Max(duration, 0.14f));
    }

    void Land(ItemDefinition item)
    {
        ShowItem(item, 0f);
        icon.rectTransform.DOKill();
        icon.rectTransform.anchoredPosition = _iconRest;

        SetSpinSpeed(0f);
        sparks.SetSpinning(false);
        selectionBrackets.SetActive(true);
        PlayBurst(0.3f);
    }

    void ShowItem(ItemDefinition item, float blend)
    {
        int rarity = item.rarity;
        Color main = RarityHelper.Color(rarity);
        Color light = RarityHelper.GlowColor(rarity);

        icon.sprite = item.icon;
        plate.sprite = RarityHelper.IconPlate(rarity);
        plate.color = main;
        pattern.sprite = RarityHelper.IconGlow(rarity);
        pattern.color = light;
        glow.sprite = pattern.sprite;
        rarityText.text = RarityHelper.DisplayName(rarity);
        rarityText.color = main;

        Paint(main, light, blend);
    }

    void SetSpinSpeed(float speed01)
    {
        float timeScale = Mathf.Lerp(1f, spinBoost, speed01);
        if (_raysSpin != null) _raysSpin.timeScale = timeScale;
        if (_ringSpin != null) _ringSpin.timeScale = timeScale;
    }

    void Punch(Transform target, float strength, float duration)
    {
        target.DOKill(true);
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * strength, duration, 6, 0.6f).AsUI(gameObject);
    }

    void Tint(Image image, Color color, float duration)
    {
        image.DOKill();
        if (duration <= 0f) image.color = color;
        else image.DOColor(color, duration).AsUI(gameObject);
    }

    static ItemDefinition PickDifferent(IReadOnlyList<ItemDefinition> pool, ItemDefinition previous)
    {
        if (pool == null || pool.Count == 0) return null;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            var candidate = pool[Random.Range(0, pool.Count)];
            if (candidate != previous) return candidate;
        }

        return pool[0];
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

    static Color Shade(Color color, float factor) => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
}
