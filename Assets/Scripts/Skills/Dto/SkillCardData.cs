
public class SkillCardData
{
    public IDrawable drawable;

    public int targetLevel;

    public string DisplayName => drawable.DisplayName;
    public ESkillRarity Rarity => drawable.Rarity;

    public SkillCardData(SkillDefinition skill, int targetLevel)
    {
        this.drawable = skill;
        this.targetLevel = targetLevel;
    }

    public SkillCardData(WeaponDefinition weapon)
    {
        this.drawable = weapon;
        this.targetLevel = 0;
    }
}