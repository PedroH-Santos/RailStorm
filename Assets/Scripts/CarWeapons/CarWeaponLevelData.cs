using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class CarWeaponLevelData
{
    static readonly ECarWeaponStatTarget[] BaseDisplayTargets =
    {
        ECarWeaponStatTarget.Damage, ECarWeaponStatTarget.AttackRate, ECarWeaponStatTarget.Range
    };

    public int damage = 10;
    public float attackRate = 1f;
    public float range = 15f;

    public abstract CarWeaponLevelData Clone();

    public virtual IReadOnlyList<ECarWeaponStatTarget> DisplayStats => BaseDisplayTargets;

    public virtual float GetStatValue(ECarWeaponStatTarget target)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.Damage: return damage;
            case ECarWeaponStatTarget.AttackRate: return attackRate;
            case ECarWeaponStatTarget.Range: return range;
            default: return 0f;
        }
    }

    public virtual void ApplyModifier(ECarWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.Damage:
                damage = isMultiplier ? Mathf.RoundToInt(damage * (1f + value / 100f)) : damage + (int)value;
                break;
            case ECarWeaponStatTarget.AttackRate:
                attackRate = isMultiplier ? attackRate * (1f + value / 100f) : attackRate + value;
                break;
            case ECarWeaponStatTarget.Range:
                range = isMultiplier ? range * (1f + value / 100f) : range + value;
                break;
        }
    }
}

[Serializable]
public class ArrowLevelData : CarWeaponLevelData
{
    static readonly ECarWeaponStatTarget[] ArrowDisplayTargets =
    {
        ECarWeaponStatTarget.Damage, ECarWeaponStatTarget.AttackRate, ECarWeaponStatTarget.Range,
        ECarWeaponStatTarget.ArrowCount, ECarWeaponStatTarget.Speed
    };

    public float speed = 20f;
    public int arrowCount = 1;

    public override CarWeaponLevelData Clone() => new ArrowLevelData
    {
        damage = damage,
        attackRate = attackRate,
        range = range,
        speed = speed,
        arrowCount = arrowCount
    };

    public override IReadOnlyList<ECarWeaponStatTarget> DisplayStats => ArrowDisplayTargets;

    public override float GetStatValue(ECarWeaponStatTarget target)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.ArrowCount: return arrowCount;
            case ECarWeaponStatTarget.Speed: return speed;
            default: return base.GetStatValue(target);
        }
    }

    public override void ApplyModifier(ECarWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.Speed:
                speed = isMultiplier ? speed * (1f + value / 100f) : speed + value;
                return;
            case ECarWeaponStatTarget.ArrowCount:
                arrowCount = isMultiplier ? Mathf.RoundToInt(arrowCount * (1f + value / 100f)) : arrowCount + (int)value;
                return;
        }
        base.ApplyModifier(target, value, isMultiplier);
    }
}

[Serializable]
public class MagicLevelData : CarWeaponLevelData
{
    static readonly ECarWeaponStatTarget[] MagicDisplayTargets =
    {
        ECarWeaponStatTarget.Damage, ECarWeaponStatTarget.AttackRate, ECarWeaponStatTarget.Range,
        ECarWeaponStatTarget.Area, ECarWeaponStatTarget.CastTime
    };

    public float area = 3f;
    public float castTime = 0.5f;

    public override CarWeaponLevelData Clone() => new MagicLevelData
    {
        damage = damage,
        attackRate = attackRate,
        range = range,
        area = area,
        castTime = castTime
    };

    public override IReadOnlyList<ECarWeaponStatTarget> DisplayStats => MagicDisplayTargets;

    public override float GetStatValue(ECarWeaponStatTarget target)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.Area: return area;
            case ECarWeaponStatTarget.CastTime: return castTime;
            default: return base.GetStatValue(target);
        }
    }

    public override void ApplyModifier(ECarWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case ECarWeaponStatTarget.Area:
                area = isMultiplier ? area * (1f + value / 100f) : area + value;
                return;
            case ECarWeaponStatTarget.CastTime:
                castTime = isMultiplier ? castTime * (1f + value / 100f) : castTime + value;
                return;
        }
        base.ApplyModifier(target, value, isMultiplier);
    }
}