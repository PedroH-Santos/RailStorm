using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class WeaponDefinition : ScriptableObject, IDrawable
{
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea] public string description = "";
    public EWeaponType weaponType = EWeaponType.None;

    [SerializeReference]
    public List<WeaponLevelData> levels = new();

    public string DisplayName => weaponName;
    public Sprite Icon => icon;

    public int LevelCount => levels.Count;
    public int MaxRarity => levels.Count - 1;

    public WeaponLevelData GetStatsForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return null;
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public T GetStats<T>(int rarityIndex) where T : WeaponLevelData
        => GetStatsForRarity(rarityIndex) as T;
}
