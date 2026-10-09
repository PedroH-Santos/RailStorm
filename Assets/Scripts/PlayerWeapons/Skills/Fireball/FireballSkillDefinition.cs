using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireballSkill", menuName = "Player Weapons/Skills/Fireball")]
public class FireballSkillDefinition : LeveledSkillDefinition<FireballLevelData>
{
    static readonly ESkillStatTarget[] StatsShownToPlayer =
    {
        ESkillStatTarget.Damage,
        ESkillStatTarget.Cooldown,
        ESkillStatTarget.Range,
        ESkillStatTarget.Speed,
    };

    public FireballProjectile projectilePrefab;
    public GameObject muzzlePrefab;
    [Min(0)] public int burnStacksPerHit = 1;

    [Header("Ajuda na mira")]
    [Tooltip("Inimigo dentro deste ângulo da mira, e dentro do alcance, vira o alvo: a bola sai na direção dele e o persegue. 0 = sem ajuda. A variante Tripla usa o ângulo dela, por projétil.")]
    [Range(0f, 90f)] public float aimAssistAngle = 25f;

    public override IReadOnlyList<ESkillStatTarget> DisplayStats => StatsShownToPlayer;

    public override void Cast(SkillCastContext context, SkillLevel level, SkillVariantDefinition variant)
    {
        if (projectilePrefab == null || context.FirePoint == null) return;

        var stats = GetLevelStats(level);
        var hit = new SkillHit(stats.damage, context.Burn, burnStacksPerHit);
        Vector3 origin = context.FirePoint.position;
        Vector3 forward = context.AimForward;

        SpawnMuzzleFlash(origin, forward);

        if (variant is FireballTripleVariant triple)
            LaunchSpread(context, stats, hit, forward, triple);
        else
            LaunchTowardsEnemyNearAim(context, stats, hit, forward);
    }

    void SpawnMuzzleFlash(Vector3 position, Vector3 forward)
    {
        if (muzzlePrefab != null) Instantiate(muzzlePrefab, position, Quaternion.LookRotation(forward));
    }

    void LaunchTowardsEnemyNearAim(SkillCastContext context, FireballLevelData stats, SkillHit hit, Vector3 forward)
    {
        Vector3 origin = context.FirePoint.position;
        EnemyTargeting.TryFindEnemyNearAim(origin, forward, stats.range, aimAssistAngle, out var target);
        LaunchProjectile(context, stats, DirectionTo(target, origin, forward), hit, target);
    }

    void LaunchSpread(SkillCastContext context, FireballLevelData stats, SkillHit hit, Vector3 forward, FireballTripleVariant triple)
    {
        var hitPerProjectile = hit.WithDamage(triple.DamagePerProjectile(stats.damage));
        Vector3 origin = context.FirePoint.position;

        var directions = triple.SpreadDirections(forward).ToArray();
        var targets = EnemyTargeting.AssignDistinctEnemies(origin, directions, stats.range, triple.aimAssistAnglePerProjectile);

        for (int i = 0; i < directions.Length; i++)
            LaunchProjectile(context, stats, DirectionTo(targets[i], origin, directions[i]), hitPerProjectile, targets[i]);
    }

    static Vector3 DirectionTo(LifeSystem target, Vector3 origin, Vector3 fallback)
        => target != null ? (EnemyTargeting.BodyCenter(target) - origin).normalized : fallback;

    void LaunchProjectile(SkillCastContext context, FireballLevelData stats, Vector3 direction, SkillHit hit, LifeSystem target)
    {
        var projectile = Instantiate(projectilePrefab, context.FirePoint.position, Quaternion.LookRotation(direction));
        projectile.Launch(direction, stats, hit, target);
    }
}
