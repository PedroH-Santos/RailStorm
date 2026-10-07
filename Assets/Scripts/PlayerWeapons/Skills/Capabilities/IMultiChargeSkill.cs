public interface IMultiChargeSkill
{
    int GetChargeCount(SkillLevel level, SkillVariantDefinition variant);
    float ChargeWindowSeconds { get; }
}
