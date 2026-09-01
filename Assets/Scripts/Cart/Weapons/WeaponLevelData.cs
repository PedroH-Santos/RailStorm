using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class WeaponLevelData
{
    static readonly EWeaponStatTarget[] BaseDisplayTargets =
    {
        EWeaponStatTarget.Damage, EWeaponStatTarget.AttackRate, EWeaponStatTarget.Range
    };

    public int damage = 10;
    public float attackRate = 1f;
    public float range = 15f;

    public abstract WeaponLevelData Clone();

    public virtual IReadOnlyList<EWeaponStatTarget> DisplayStats => BaseDisplayTargets;

    public virtual float GetStatValue(EWeaponStatTarget target)
    {
        switch (target)
        {
            case EWeaponStatTarget.Damage: return damage;
            case EWeaponStatTarget.AttackRate: return attackRate;
            case EWeaponStatTarget.Range: return range;
            default: return 0f;
        }
    }

    public virtual void ApplyModifier(EWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case EWeaponStatTarget.Damage:
                damage = isMultiplier ? Mathf.RoundToInt(damage * (1f + value / 100f)) : damage + (int)value;
                break;
            case EWeaponStatTarget.AttackRate:
                attackRate = isMultiplier ? attackRate * (1f + value / 100f) : attackRate + value;
                break;
            case EWeaponStatTarget.Range:
                range = isMultiplier ? range * (1f + value / 100f) : range + value;
                break;
        }
    }
}

[Serializable]
public class ArrowLevelData : WeaponLevelData
{
    static readonly EWeaponStatTarget[] ArrowDisplayTargets =
    {
        EWeaponStatTarget.Damage, EWeaponStatTarget.AttackRate, EWeaponStatTarget.Range,
        EWeaponStatTarget.ArrowCount, EWeaponStatTarget.Speed
    };

    public float speed = 20f;
    public int arrowCount = 1;

    public override WeaponLevelData Clone() => new ArrowLevelData
    {
        damage = damage,
        attackRate = attackRate,
        range = range,
        speed = speed,
        arrowCount = arrowCount
    };

    public override IReadOnlyList<EWeaponStatTarget> DisplayStats => ArrowDisplayTargets;

    public override float GetStatValue(EWeaponStatTarget target)
    {
        switch (target)
        {
            case EWeaponStatTarget.ArrowCount: return arrowCount;
            case EWeaponStatTarget.Speed: return speed;
            default: return base.GetStatValue(target);
        }
    }

    public override void ApplyModifier(EWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case EWeaponStatTarget.Speed:
                speed = isMultiplier ? speed * (1f + value / 100f) : speed + value;
                return;
            case EWeaponStatTarget.ArrowCount:
                arrowCount = isMultiplier ? Mathf.RoundToInt(arrowCount * (1f + value / 100f)) : arrowCount + (int)value;
                return;
        }
        base.ApplyModifier(target, value, isMultiplier);
    }
}

[Serializable]
public class MagicLevelData : WeaponLevelData
{
    static readonly EWeaponStatTarget[] MagicDisplayTargets =
    {
        EWeaponStatTarget.Damage, EWeaponStatTarget.AttackRate, EWeaponStatTarget.Range,
        EWeaponStatTarget.Area, EWeaponStatTarget.CastTime
    };

    public float area = 3f;
    public float castTime = 0.5f;

    public override WeaponLevelData Clone() => new MagicLevelData
    {
        damage = damage,
        attackRate = attackRate,
        range = range,
        area = area,
        castTime = castTime
    };

    public override IReadOnlyList<EWeaponStatTarget> DisplayStats => MagicDisplayTargets;

    public override float GetStatValue(EWeaponStatTarget target)
    {
        switch (target)
        {
            case EWeaponStatTarget.Area: return area;
            case EWeaponStatTarget.CastTime: return castTime;
            default: return base.GetStatValue(target);
        }
    }

    public override void ApplyModifier(EWeaponStatTarget target, float value, bool isMultiplier)
    {
        switch (target)
        {
            case EWeaponStatTarget.Area:
                area = isMultiplier ? area * (1f + value / 100f) : area + value;
                return;
            case EWeaponStatTarget.CastTime:
                castTime = isMultiplier ? castTime * (1f + value / 100f) : castTime + value;
                return;
        }
        base.ApplyModifier(target, value, isMultiplier);
    }
}