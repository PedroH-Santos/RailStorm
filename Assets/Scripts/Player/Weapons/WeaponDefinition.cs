using UnityEngine;


[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class WeaponDefinition : ScriptableObject, IDrawable
{
    [Header("Info")]
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea]
    public string description = "";

    [Header("Raridade  (mesmo enum das Skills)")]
    public ESkillRarity rarity;

    [Header("Stats de Combate")]
    public int damage = 10;
    public float fireRate = 1f; 
    public float range = 15f;

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public ESkillRarity Rarity => rarity;

    static readonly float WeightCommon = 70f;
    static readonly float WeightUncommon = 20f;
    static readonly float WeightRare = 10f;

    public float GetWeight(float luckPercent)
    {
        float common = Mathf.Max(0f, WeightCommon - luckPercent * 0.6f);
        float uncommon = WeightUncommon + luckPercent * 0.3f;
        float rare = WeightRare + luckPercent * 0.3f;

        return rarity switch
        {
            ESkillRarity.Common => common,
            ESkillRarity.Uncommon => uncommon,
            ESkillRarity.Rare => rare,
            _ => 0f
        };
    }
}