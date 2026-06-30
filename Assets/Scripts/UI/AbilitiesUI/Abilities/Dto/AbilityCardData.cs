using Assets.Scripts.Systems.Rarity;

public class AbilityCardData
{
    public IDrawable drawable;
    public int targetRarity;
    public bool isUpgrade;
    public WeaponDefinition targetWeapon;

    public string DisplayName => drawable.DisplayName;
    public string RarityDisplay => RarityHelper.DisplayName(targetRarity);

    public AbilityCardData(SkillDefinition skill, int targetRarity)
    {
        drawable = skill;
        this.targetRarity = targetRarity;
        isUpgrade = false;
    }

    public AbilityCardData(WeaponDefinition weapon, int targetRarity, bool upgrade = false)
    {
        drawable = weapon;
        this.targetRarity = targetRarity;
        isUpgrade = upgrade;
    }

    public AbilityCardData(WeaponSkillDefinition weaponSkill, int targetRarity, WeaponDefinition owner)
    {
        drawable = weaponSkill;
        this.targetRarity = targetRarity;
        isUpgrade = false;
        targetWeapon = owner;
    }
}