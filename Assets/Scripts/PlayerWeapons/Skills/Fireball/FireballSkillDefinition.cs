using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireballSkill", menuName = "Player Weapons/Skills/Fireball")]
public class FireballSkillDefinition : SkillDefinition
{
    static readonly ESkillStatTarget[] Targets =
    {
        ESkillStatTarget.Damage,
        ESkillStatTarget.Cooldown,
        ESkillStatTarget.Range,
        ESkillStatTarget.Speed,
    };

    public FireballProjectile projectilePrefab;
    public GameObject muzzlePrefab;
    [Min(0)] public int burnStacksPerHit = 1;

    public List<FireballLevelData> levels = new()
    {
        new FireballLevelData(),
    };

    public override int LevelCount => levels.Count;
    public override IReadOnlyList<ESkillStatTarget> DisplayStats => Targets;

    public FireballLevelData GetLevel(int level)
    {
        if (levels.Count == 0) return new FireballLevelData();
        return levels[Mathf.Clamp(level, 0, levels.Count - 1)];
    }

    public override int GetUpgradeCost(int level) => GetLevel(level).upgradeCost;
    public override float GetCooldown(int level) => GetLevel(level).cooldown;

    public override float GetStatValue(int level, ESkillStatTarget target)
    {
        var data = GetLevel(level);
        switch (target)
        {
            case ESkillStatTarget.Damage: return data.damage;
            case ESkillStatTarget.Cooldown: return data.cooldown;
            case ESkillStatTarget.Range: return data.range;
            case ESkillStatTarget.Speed: return data.speed;
            default: return 0f;
        }
    }

    public override void Cast(SkillCastContext context, int level, SkillVariantDefinition variant)
    {
        if (projectilePrefab == null || context.FirePoint == null) return;

        var stats = GetLevel(level);
        var hit = new SkillHit(stats.damage, context.Burn, burnStacksPerHit);
        Vector3 forward = context.AimDirection.sqrMagnitude > 0.0001f ? context.AimDirection.normalized : context.FirePoint.forward;

        if (muzzlePrefab != null)
            Instantiate(muzzlePrefab, context.FirePoint.position, Quaternion.LookRotation(forward));

        if (variant is FireballTripleVariant triple)
        {
            var splitHit = hit.WithDamage(triple.DamagePerProjectile(stats.damage));
            for (int i = 0; i < triple.projectileCount; i++)
            {
                float angle = Mathf.Lerp(-triple.spreadAngle * 0.5f, triple.spreadAngle * 0.5f, i / (triple.projectileCount - 1f));
                Launch(context, stats, Quaternion.AngleAxis(angle, Vector3.up) * forward, splitHit);
            }

            return;
        }

        Launch(context, stats, forward, hit);
    }

    void Launch(SkillCastContext context, FireballLevelData stats, Vector3 direction, SkillHit hit)
    {
        var projectile = Instantiate(projectilePrefab, context.FirePoint.position, Quaternion.LookRotation(direction));
        projectile.Launch(direction, stats, hit);
    }
}
