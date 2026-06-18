using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class WeaponDefinition : ScriptableObject, IDrawable
{
    public string weaponName = "Nova Arma";
    
    public Sprite icon;
    
    [TextArea] public string description = "";

    public EWeaponType weaponType = EWeaponType.None;

    [SerializeField] private int currentRarity = -1;

    public List<WeaponLevelData> levels = new()
    {
        new WeaponLevelData { damage =  5, fireRate = 1.0f, range = 15f },
        new WeaponLevelData { damage = 10, fireRate = 1.2f, range = 18f },
        new WeaponLevelData { damage = 15, fireRate = 1.4f, range = 21f },
        new WeaponLevelData { damage = 20, fireRate = 1.6f, range = 24f },
        new WeaponLevelData { damage = 25, fireRate = 1.8f, range = 27f },
    };

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public int CurrentRarity => currentRarity;

    public bool IsAcquired => currentRarity >= 0;
    public bool CanUpgrade => IsAcquired && currentRarity < levels.Count - 1;
    public int NextRarity => currentRarity + 1;

    public WeaponLevelData CurrentStats => GetStatsForRarity(Mathf.Max(currentRarity, 0));
    public WeaponLevelData NextStats => GetStatsForRarity(Mathf.Min(currentRarity + 1, levels.Count - 1));

    public WeaponLevelData GetStatsForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return new WeaponLevelData();
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public void Acquire(int rarityIndex)
    {
        currentRarity = Mathf.Clamp(rarityIndex, 0, levels.Count - 1);
    }

    public bool Upgrade(int targetRarity)
    {
        if (!CanUpgrade) return false;
        if (targetRarity <= currentRarity || targetRarity >= levels.Count) return false;
        currentRarity = targetRarity;
        return true;
    }

    public void ResetForNewRun()
    {
        currentRarity = -1;
    }
}
