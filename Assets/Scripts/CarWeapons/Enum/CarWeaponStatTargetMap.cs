using System.Collections.Generic;

public static class CarWeaponStatTargetMap
{
    static readonly ECarWeaponStatTarget[] Common =
    {
        ECarWeaponStatTarget.Damage,
        ECarWeaponStatTarget.AttackRate,
        ECarWeaponStatTarget.Range,
    };

    static readonly Dictionary<ECarWeaponType, ECarWeaponStatTarget[]> Specific = new()
    {
        { ECarWeaponType.None,  new ECarWeaponStatTarget[0] },
        { ECarWeaponType.Arrow, new[] { ECarWeaponStatTarget.Speed, ECarWeaponStatTarget.ArrowCount } },
        { ECarWeaponType.Magic, new[] { ECarWeaponStatTarget.Area, ECarWeaponStatTarget.CastTime } },
    };

    public static ECarWeaponStatTarget[] GetAllowed(ECarWeaponType type)
    {
        var result = new List<ECarWeaponStatTarget>();

        if (type != ECarWeaponType.None)
            result.AddRange(Common);

        if (Specific.TryGetValue(type, out var extra))
            result.AddRange(extra);

        return result.ToArray();
    }
}