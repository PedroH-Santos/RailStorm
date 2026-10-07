using UnityEngine;

public readonly struct SkillCastContext
{
    public readonly Transform FirePoint;
    public readonly Vector3 AimDirection;
    public readonly Transform Owner;
    public readonly BurnDefinition Burn;
    public readonly Vector3 AimPoint;
    public readonly bool HasAimPoint;

    public Vector3 AimForward
        => AimDirection.sqrMagnitude > 0.0001f ? AimDirection.normalized : FirePoint.forward;

    public SkillCastContext(Transform firePoint, Vector3 aimDirection, Transform owner, BurnDefinition burn)
        : this(firePoint, aimDirection, owner, burn, Vector3.zero, false)
    {
    }

    public SkillCastContext(Transform firePoint, Vector3 aimDirection, Transform owner, BurnDefinition burn, Vector3 aimPoint)
        : this(firePoint, aimDirection, owner, burn, aimPoint, true)
    {
    }

    SkillCastContext(Transform firePoint, Vector3 aimDirection, Transform owner, BurnDefinition burn, Vector3 aimPoint, bool hasAimPoint)
    {
        FirePoint = firePoint;
        AimDirection = aimDirection;
        Owner = owner;
        Burn = burn;
        AimPoint = aimPoint;
        HasAimPoint = hasAimPoint;
    }
}
