using Assets.Scripts.Systems.Rarity;

public class PerkCardData
{
    public IDrawable drawable;
    public int targetRarity;
    public bool isUpgrade;
    public bool isNew;
    public CarWeaponDefinition targetWeapon;

    public string DisplayName => drawable.DisplayName;
    public string RarityDisplay => RarityHelper.DisplayName(targetRarity);

    public PerkCardData(PerkDefinition skill, int targetRarity)
    {
        drawable = skill;
        this.targetRarity = targetRarity;
        isUpgrade = false;
    }

    public PerkCardData(CarWeaponDefinition weapon, int targetRarity, bool upgrade = false)
    {
        drawable = weapon;
        this.targetRarity = targetRarity;
        isUpgrade = upgrade;
    }

    public PerkCardData(WeaponPerkDefinition weaponPerk, int targetRarity, CarWeaponDefinition owner)
    {
        drawable = weaponPerk;
        this.targetRarity = targetRarity;
        isUpgrade = false;
        targetWeapon = owner;
    }
}