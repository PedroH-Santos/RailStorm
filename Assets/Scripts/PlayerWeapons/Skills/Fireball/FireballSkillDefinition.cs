using System.Collections.Generic;
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

    public override IReadOnlyList<ESkillStatTarget> DisplayStats => StatsShownToPlayer;

    public override void Cast(SkillCastContext context, SkillLevel level, SkillVariantDefinition variant)
    {
        if (projectilePrefab == null || context.FirePoint == null) return;

        var stats = GetLevelStats(level);
        var hit = new SkillHit(stats.damage, context.Burn, burnStacksPerHit);
        Vector3 forward = context.AimForward;

        SpawnMuzzleFlash(context.FirePoint.position, forward);

        if (variant is FireballTripleVariant triple)
            LaunchSpread(context, stats, hit, forward, triple);
        else
            LaunchProjectile(context, stats, forward, hit);
    }

    void SpawnMuzzleFlash(Vector3 position, Vector3 forward)
    {
        if (muzzlePrefab != null) Instantiate(muzzlePrefab, position, Quaternion.LookRotation(forward));
    }

    void LaunchSpread(SkillCastContext context, FireballLevelData stats, SkillHit hit, Vector3 forward, FireballTripleVariant triple)
    {
        var hitPerProjectile = hit.WithDamage(triple.DamagePerProjectile(stats.damage));

        foreach (var direction in triple.SpreadDirections(forward))
            LaunchProjectile(context, stats, direction, hitPerProjectile);
    }

    void LaunchProjectile(SkillCastContext context, FireballLevelData stats, Vector3 direction, SkillHit hit)
    {
        var projectile = Instantiate(projectilePrefab, context.FirePoint.position, Quaternion.LookRotation(direction));
        projectile.Launch(direction, stats, hit);
    }
}
