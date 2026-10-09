using System.Collections.Generic;
using UnityEngine;

public static class EnemyTargeting
{
    const string EnemyTag = "Enemy";
    const int MaxNearbyColliders = 64;

    static readonly Collider[] NearbyColliders = new Collider[MaxNearbyColliders];

    public static bool TryGetLivingEnemy(Collider collider, out LifeSystem enemy)
    {
        enemy = collider != null ? collider.GetComponentInParent<LifeSystem>() : null;
        return enemy != null && !enemy.IsDead && enemy.CompareTag(EnemyTag);
    }

    public static bool TryFindEnemyNearAim(Vector3 origin, Vector3 aimDirection, float range, float maxAngle, out LifeSystem enemy)
    {
        enemy = AssignDistinctEnemies(origin, new[] { aimDirection }, range, maxAngle)[0];
        return enemy != null;
    }

    public static LifeSystem[] AssignDistinctEnemies(Vector3 origin, IReadOnlyList<Vector3> aimDirections, float range, float maxAngle)
    {
        var assigned = new LifeSystem[aimDirections.Count];
        if (maxAngle <= 0f) return assigned;

        var pairs = new List<(int aim, LifeSystem enemy, float angle)>();
        foreach (var enemy in LivingEnemiesInRange(origin, range))
        {
            Vector3 toEnemy = OnGround(BodyCenter(enemy) - origin);
            for (int aim = 0; aim < aimDirections.Count; aim++)
            {
                float angle = Vector3.Angle(OnGround(aimDirections[aim]), toEnemy);
                if (angle <= maxAngle) pairs.Add((aim, enemy, angle));
            }
        }

        pairs.Sort((a, b) => a.angle.CompareTo(b.angle));

        var taken = new HashSet<LifeSystem>();
        foreach (var (aim, enemy, _) in pairs)
        {
            if (assigned[aim] != null || taken.Contains(enemy)) continue;

            assigned[aim] = enemy;
            taken.Add(enemy);
        }

        return assigned;
    }

    public static Vector3 BodyCenter(LifeSystem enemy)
    {
        var body = enemy.GetComponentInChildren<Collider>();
        return body != null ? body.bounds.center : enemy.transform.position;
    }

    static HashSet<LifeSystem> LivingEnemiesInRange(Vector3 origin, float range)
    {
        var enemies = new HashSet<LifeSystem>();
        int found = Physics.OverlapSphereNonAlloc(origin, range, NearbyColliders, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < found; i++)
        {
            if (!TryGetLivingEnemy(NearbyColliders[i], out var enemy)) continue;
            if (OnGround(BodyCenter(enemy) - origin).magnitude <= range) enemies.Add(enemy);
        }

        return enemies;
    }

    static Vector3 OnGround(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
