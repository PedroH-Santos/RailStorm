using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChestRouletteAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform revealSlot;
    [SerializeField] private Image iconPlate;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image panelBorder;
    [SerializeField] private Image rarityTagPlate;
    [SerializeField] private ChestSparksEmitter sparksEmitter;

    [SerializeField] private float tickStart = 0.04f;
    [SerializeField] private float tickGrowth = 1.16f;
    [SerializeField] private float tickEnd = 0.32f;
    [SerializeField] private float tickPop = 1.12f;
    [SerializeField] private float landPunch = 1.25f;
    [SerializeField] private float punchDuration = 0.18f;
    [SerializeField] private int sparkBurstCount = 14;

    Sequence _sequence;

    public bool IsSpinning { get; private set; }

    public void Play(ItemDefinition finalItem, int finalRarityIndex, IReadOnlyList<ItemDefinition> pool, Action onComplete)
    {
        Stop();

        IsSpinning = true;
        sparksEmitter?.Play();

        _sequence = DOTween.Sequence().SetUpdate(true);

        ItemDefinition previous = null;
        float interval = tickStart;

        while (interval < tickEnd)
        {
            var candidate = PickTick(pool, previous, finalItem);
            _sequence.AppendCallback(() => ApplyTick(candidate, candidate.rarity, tickPop, punchDuration));
            _sequence.AppendInterval(interval);
            previous = candidate;
            interval *= tickGrowth;
        }

        _sequence.AppendCallback(() => Land(finalItem, finalRarityIndex));
        _sequence.OnComplete(() =>
        {
            IsSpinning = false;
            onComplete?.Invoke();
        });
    }

    ItemDefinition PickTick(IReadOnlyList<ItemDefinition> pool, ItemDefinition previous, ItemDefinition finalItem)
    {
        if (pool == null || pool.Count == 0) return finalItem;
        if (pool.Count == 1) return pool[0];

        ItemDefinition candidate;
        int guard = 0;
        do
        {
            candidate = pool[Random.Range(0, pool.Count)];
            guard++;
        } while (candidate == previous && guard < 8);

        return candidate;
    }

    void ApplyTick(ItemDefinition item, int rarityIndex, float punchScale, float duration)
    {
        if (item == null) return;

        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (iconPlate != null)
        {
            iconPlate.sprite = RarityHelper.IconPlate(rarityIndex);
            iconPlate.color = RarityHelper.Color(rarityIndex);
        }

        if (panelBorder != null) panelBorder.color = RarityHelper.Color(rarityIndex);
        if (rarityTagPlate != null) rarityTagPlate.color = RarityHelper.Color(rarityIndex);
        sparksEmitter?.SetColor(RarityHelper.GlowColor(rarityIndex));

        if (revealSlot != null)
        {
            revealSlot.DOKill(true);
            revealSlot.localScale = Vector3.one;
            revealSlot.DOPunchScale(Vector3.one * (punchScale - 1f), duration).SetUpdate(true);
        }
    }

    void Land(ItemDefinition finalItem, int finalRarityIndex)
    {
        ApplyTick(finalItem, finalRarityIndex, landPunch, 0.28f);
        sparksEmitter?.SetSpinning(false);
        sparksEmitter?.Burst(sparkBurstCount);
    }

    public void Stop()
    {
        _sequence?.Kill();
        _sequence = null;
        IsSpinning = false;
        revealSlot?.DOKill(true);
        sparksEmitter?.Stop();
    }

    void OnDisable() => Stop();
}
