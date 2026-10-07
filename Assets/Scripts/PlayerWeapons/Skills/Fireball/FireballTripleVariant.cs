using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireballTripleVariant", menuName = "Player Weapons/Skill Variants/Fireball Triple")]
public class FireballTripleVariant : SkillVariantDefinition
{
    [Min(2)] public int projectileCount = 3;
    [Min(0f)] public float spreadAngle = 18f;

    public int DamagePerProjectile(int levelDamage)
        => Mathf.Max(1, Mathf.RoundToInt(levelDamage / (float)Mathf.Max(1, projectileCount)));

    public IEnumerable<Vector3> SpreadDirections(Vector3 forward)
    {
        float leftmostAngle = -spreadAngle * 0.5f;
        float rightmostAngle = spreadAngle * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float positionInFan = i / (projectileCount - 1f);
            float angle = Mathf.Lerp(leftmostAngle, rightmostAngle, positionInFan);
            yield return Quaternion.AngleAxis(angle, Vector3.up) * forward;
        }
    }
}
