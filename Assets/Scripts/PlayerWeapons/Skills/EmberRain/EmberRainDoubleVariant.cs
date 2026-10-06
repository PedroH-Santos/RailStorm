using UnityEngine;

[CreateAssetMenu(fileName = "NewEmberRainDoubleVariant", menuName = "Player Weapons/Skill Variants/Ember Rain Double")]
public class EmberRainDoubleVariant : SkillVariantDefinition
{
    [Min(2)] public int chargeCount = 2;

    public float DurationPerCharge(float levelDuration) => levelDuration / Mathf.Max(1, chargeCount);
}
