public interface IMultiChargeSkill
{
    int GetChargeCount(int level, SkillVariantDefinition variant);
    float ChargeWindowSeconds { get; }
}
