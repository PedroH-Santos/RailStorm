using UnityEngine;

[CreateAssetMenu(fileName = "NewFireballTripleVariant", menuName = "Player Weapons/Skill Variants/Fireball Triple")]
public class FireballTripleVariant : SkillVariantDefinition
{
    [Min(2)] public int projectileCount = 3;
    [Min(0f)] public float spreadAngle = 18f;

    public int DamagePerProjectile(int levelDamage)
        => Mathf.Max(1, Mathf.RoundToInt(levelDamage / (float)Mathf.Max(1, projectileCount)));
}
