using UnityEngine;

public readonly struct SkillCastContext
{
    public readonly Transform FirePoint;
    public readonly Vector3 AimDirection;
    public readonly Transform Owner;

    public SkillCastContext(Transform firePoint, Vector3 aimDirection, Transform owner)
    {
        FirePoint = firePoint;
        AimDirection = aimDirection;
        Owner = owner;
    }
}
