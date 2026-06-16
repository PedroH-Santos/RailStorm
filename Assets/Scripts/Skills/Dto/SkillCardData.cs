using Assets.Scripts.Systems.Rarity;

public class SkillCardData
{
    public IDrawable drawable;
    public int targetRarityHelper;
    public bool isWeaponUpgrade;

    public string DisplayName => drawable.DisplayName;
    public string RarityDisplay => RarityHelper.DisplayName(targetRarityHelper);

    public SkillCardData(SkillDefinition skill, int targetRarityHelper)
    {
        drawable = skill;
        this.targetRarityHelper = targetRarityHelper;
        isWeaponUpgrade = false;
    }

    public SkillCardData(WeaponDefinition weapon, int targetRarityHelper)
    {
        drawable = weapon;
        this.targetRarityHelper = targetRarityHelper;
        isWeaponUpgrade = false;
    }

    public SkillCardData(WeaponDefinition weapon, int nextRarityHelper, bool upgrade)
    {
        drawable = weapon;
        targetRarityHelper = nextRarityHelper;
        isWeaponUpgrade = upgrade;
    }
}