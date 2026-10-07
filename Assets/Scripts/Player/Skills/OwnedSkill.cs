public class OwnedSkill
{
    public SkillDefinition Definition { get; }
    public SkillLevel Level { get; private set; }
    public SkillVariantDefinition ActiveVariant { get; private set; }

    public OwnedSkill(SkillDefinition definition)
    {
        Definition = definition;
        Level = SkillLevel.First;
    }

    public bool IsAtMaxLevel => Definition.IsLastLevel(Level);
    public int UpgradeCost => IsAtMaxLevel ? 0 : Definition.GetUpgradeCost(Level);
    public float Cooldown => Definition.GetCooldown(Level);
    public bool HasCooldown => Definition.HasCooldown(Level);
    public int Rarity => Definition.RarityAt(Level);

    public bool IsUsing(SkillVariantDefinition variant) => variant != null && ActiveVariant == variant;

    public void LevelUp() => Level = Level.Next;
    public void UseVariant(SkillVariantDefinition variant) => ActiveVariant = variant;
    public void UseBaseForm() => ActiveVariant = null;

    public void Cast(SkillCastContext context) => Definition.Cast(context, Level, ActiveVariant);
}
