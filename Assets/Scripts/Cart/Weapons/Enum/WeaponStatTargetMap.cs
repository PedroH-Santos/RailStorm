using System.Collections.Generic;

public static class WeaponStatTargetMap
{
    static readonly EWeaponStatTarget[] Common =
    {
        EWeaponStatTarget.Damage,
        EWeaponStatTarget.AttackRate,
        EWeaponStatTarget.Range,
    };

    static readonly Dictionary<EWeaponType, EWeaponStatTarget[]> Specific = new()
    {
        { EWeaponType.None,  new EWeaponStatTarget[0] },
        { EWeaponType.Arrow, new[] { EWeaponStatTarget.Speed, EWeaponStatTarget.ArrowCount } },
        { EWeaponType.Magic, new[] { EWeaponStatTarget.Area, EWeaponStatTarget.CastTime } },
    };

    public static EWeaponStatTarget[] GetAllowed(EWeaponType type)
    {
        var result = new List<EWeaponStatTarget>();

        if (type != EWeaponType.None)
            result.AddRange(Common);

        if (Specific.TryGetValue(type, out var extra))
            result.AddRange(extra);

        return result.ToArray();
    }
}