using UnityEngine;

public readonly struct SkillCastContext
{
    public readonly Transform FirePoint;
    public readonly Vector3 AimDirection;
    public readonly BurnDefinition Burn;
    public readonly Vector3 AimPoint;
    public readonly bool HasAimPoint;

    SkillCastContext(Transform firePoint, Vector3 aimDirection, BurnDefinition burn, Vector3 aimPoint, bool hasAimPoint)
    {
        FirePoint = firePoint;
        AimDirection = aimDirection;
        Burn = burn;
        AimPoint = aimPoint;
        HasAimPoint = hasAimPoint;
    }

    public static SkillCastContext TowardsAim(Transform firePoint, Vector3 aimDirection, BurnDefinition burn)
        => new(firePoint, aimDirection, burn, Vector3.zero, false);

    public static SkillCastContext AtGroundPoint(Transform firePoint, Vector3 aimDirection, BurnDefinition burn, Vector3 groundPoint)
        => new(firePoint, aimDirection, burn, groundPoint, true);

    public Vector3 AimForward
        => AimDirection.sqrMagnitude > 0.0001f ? AimDirection.normalized : FirePoint.forward;
}
