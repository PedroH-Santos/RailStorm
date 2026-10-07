public class SkillSlot
{
    public int Index { get; }
    public bool IsUnlocked { get; private set; }
    public OwnedSkill Skill { get; private set; }
    public SkillCooldown Cooldown { get; private set; } = new();
    public SkillCharges Charges { get; private set; } = new();

    public SkillSlot(int index)
    {
        Index = index;
    }

    public bool IsEmpty => Skill == null;
    public bool IsReady => IsUnlocked && !IsEmpty && !Cooldown.IsCoolingDown;
    public bool Holds(SkillDefinition definition) => Skill != null && Skill.Definition == definition;

    public void Unlock() => IsUnlocked = true;

    public void Equip(OwnedSkill skill)
    {
        Skill = skill;
        Cooldown.Reset();
        Charges.Clear();
    }

    public void Empty() => Equip(null);

    public void SwapContentsWith(SkillSlot other)
    {
        (Skill, other.Skill) = (other.Skill, Skill);
        (Cooldown, other.Cooldown) = (other.Cooldown, Cooldown);
        (Charges, other.Charges) = (other.Charges, Charges);
    }

    public void Tick(float deltaTime)
    {
        Cooldown.Tick(deltaTime);

        bool unusedChargesExpired = Charges.RanOutOfTime(deltaTime);
        if (unusedChargesExpired) StartCooldown();
    }

    public void Cast(SkillCastContext context)
    {
        Skill.Cast(context);

        if (Skill.Definition is IMultiChargeSkill multiCharge) SpendCharge(multiCharge);
        else StartCooldown();
    }

    void SpendCharge(IMultiChargeSkill multiCharge)
    {
        bool isFirstChargeOfBurst = !Charges.HasChargesLeft;
        if (isFirstChargeOfBurst) Charges.StartBurst(multiCharge.GetChargeCount(Skill.Level, Skill.ActiveVariant));

        Charges.SpendOne(multiCharge.ChargeWindowSeconds);

        if (!Charges.HasChargesLeft) StartCooldown();
    }

    void StartCooldown()
    {
        if (Skill != null) Cooldown.Start(Skill.Cooldown);
    }
}
