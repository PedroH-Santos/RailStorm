public readonly struct SkillHit
{
    public readonly int Damage;
    public readonly BurnDefinition Burn;
    public readonly int BurnStacks;

    public SkillHit(int damage, BurnDefinition burn, int burnStacks)
    {
        Damage = damage;
        Burn = burn;
        BurnStacks = burnStacks;
    }

    public SkillHit WithDamage(int damage) => new SkillHit(damage, Burn, BurnStacks);

    public void ApplyTo(LifeSystem enemy)
    {
        enemy.Damage(Damage);

        bool survivedTheHit = !enemy.IsDead;
        if (survivedTheHit) BurnReceiver.AddStacks(enemy.gameObject, Burn, BurnStacks);
    }
}
