using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class CarWeaponDefinition : ScriptableObject, IDrawable
{
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea] public string description = "";
    public ECarWeaponType weaponType = ECarWeaponType.None;

    [SerializeReference]
    public List<CarWeaponLevelData> levels = new();

    public string DisplayName => weaponName;
    public Sprite Icon => icon;

    public int LevelCount => levels.Count;
    public int MaxRarity => levels.Count - 1;

    public CarWeaponLevelData GetStatsForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return null;
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public T GetStats<T>(int rarityIndex) where T : CarWeaponLevelData
        => GetStatsForRarity(rarityIndex) as T;
}
