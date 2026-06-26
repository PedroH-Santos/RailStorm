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

    // [SerializeReference] é o que permite subclasses concretas aqui
    [SerializeReference]
    public List<WeaponLevelData> levels = new();

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public int CurrentRarity => currentRarity;
    public bool IsAcquired => currentRarity >= 0;
    public bool CanUpgrade => IsAcquired && currentRarity < levels.Count - 1;
    public int NextRarity => currentRarity + 1;

    public WeaponLevelData CurrentStats => GetStatsForRarity(Mathf.Max(currentRarity, 0));
    public WeaponLevelData NextStats => GetStatsForRarity(Mathf.Min(currentRarity + 1, levels.Count - 1));

    // Cast tipado — use nos controllers que sabem o tipo
    public T GetStats<T>(int rarityIndex) where T : WeaponLevelData
        => GetStatsForRarity(rarityIndex) as T;

    public WeaponLevelData GetStatsForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return null;
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public void Acquire(int rarityIndex)
        => currentRarity = Mathf.Clamp(rarityIndex, 0, levels.Count - 1);

    public bool Upgrade(int targetRarity)
    {
        if (!CanUpgrade || targetRarity <= currentRarity || targetRarity >= levels.Count)
            return false;
        currentRarity = targetRarity;
        return true;
    }

    public void ResetForNewRun() => currentRarity = -1;
}