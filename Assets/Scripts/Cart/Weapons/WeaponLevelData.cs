using UnityEngine;

public abstract class WeaponLevelData
{
    public int damage = 10;
    public float attackRate = 1f;
    public float range = 15f;

    public abstract WeaponLevelData Clone();

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

[System.Serializable]
public class ArrowLevelData : WeaponLevelData
{
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

[System.Serializable]
public class MagicLevelData : WeaponLevelData
{
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