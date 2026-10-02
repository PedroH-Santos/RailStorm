using UnityEngine;

public readonly struct SkillCastContext
{
    public readonly Transform FirePoint;
    public readonly Vector3 AimDirection;
    public readonly Transform Owner;
    public readonly BurnDefinition Burn;

    public SkillCastContext(Transform firePoint, Vector3 aimDirection, Transform owner, BurnDefinition burn)
    {
        FirePoint = firePoint;
        AimDirection = aimDirection;
        Owner = owner;
        Burn = burn;
    }
}
