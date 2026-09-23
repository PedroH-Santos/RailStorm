using UnityEngine.Serialization;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeaponPerk", menuName = "Car/Weapon Perk Definition")]
public class WeaponPerkDefinition : ScriptableObject, IDrawable
{
    [FormerlySerializedAs("skillName")] public string perkName = "Novo Perk de Arma";
    public Sprite icon;
    [TextArea] public string description = "";

    public ECarWeaponType weaponType;
    public ECarWeaponStatTarget statTarget;

    public List<PerkLevelData> levels = new()
    {
        new PerkLevelData { statValue = 10f, isMultiplier = true },
        new PerkLevelData { statValue = 20f, isMultiplier = true },
        new PerkLevelData { statValue = 30f, isMultiplier = true },
    };

    public string DisplayName => perkName;
    public Sprite Icon => icon;

    public int LevelCount => levels.Count;
    public int MaxRarity => levels.Count - 1;

    public PerkLevelData GetLevelForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return new PerkLevelData();
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }
}
