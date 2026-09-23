using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireballSkill", menuName = "Player Weapons/Skills/Fireball")]
public class FireballSkillDefinition : SkillDefinition
{
    static readonly ESkillStatTarget[] Targets =
    {
        ESkillStatTarget.Damage,
        ESkillStatTarget.Cooldown,
        ESkillStatTarget.ProjectileCount,
        ESkillStatTarget.Range,
        ESkillStatTarget.Speed,
    };

    public FireballProjectile projectilePrefab;

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
            case ESkillStatTarget.ProjectileCount: return data.projectileCount;
            case ESkillStatTarget.Spread: return data.spreadAngle;
            default: return 0f;
        }
    }

    public override void Cast(SkillCastContext context, int level)
    {
        if (projectilePrefab == null || context.FirePoint == null) return;

        var data = GetLevel(level);
        int count = Mathf.Max(1, data.projectileCount);
        Vector3 forward = context.AimDirection.sqrMagnitude > 0.0001f ? context.AimDirection.normalized : context.FirePoint.forward;

        for (int i = 0; i < count; i++)
        {
            float angle = count > 1 ? Mathf.Lerp(-data.spreadAngle * 0.5f, data.spreadAngle * 0.5f, i / (count - 1f)) : 0f;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            var projectile = Instantiate(projectilePrefab, context.FirePoint.position, Quaternion.LookRotation(direction));
            projectile.Init(direction, data.speed, data.range, data.damage);
        }
    }
}
