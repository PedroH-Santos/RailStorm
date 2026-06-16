using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class WeaponDefinition : ScriptableObject, IDrawable
{
    [Header("Info")]
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Estado atual (gerenciado em runtime — reseta com a cena)")]
    [SerializeField] private int currentRarityHelper = -1; 

    [Header("Stats por raridade (ordem segue o RarityConfig)")]
    public List<WeaponLevelData> levels = new();

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public int RarityHelper => Mathf.Max(currentRarityHelper, 0);

    public bool IsAcquired => currentRarityHelper >= 0;
    public bool CanUpgrade => IsAcquired && currentRarityHelper < levels.Count - 1;
    public int NextRarityHelper => currentRarityHelper + 1;

    public WeaponLevelData CurrentStats =>
        GetStatsForRarity(Mathf.Max(currentRarityHelper, 0));

    public WeaponLevelData NextStats =>
        GetStatsForRarity(Mathf.Min(currentRarityHelper + 1, levels.Count - 1));

    public void Acquire(int RarityHelper)
    {
        currentRarityHelper = Mathf.Clamp(RarityHelper, 0, levels.Count - 1);
    }

    public bool Upgrade()
    {
        if (!CanUpgrade) return false;
        currentRarityHelper++;
        return true;
    }

    public void ResetForNewRun()
    {
        currentRarityHelper = -1;
    }

    public WeaponLevelData GetStatsForRarity(int RarityHelper)
    {
        if (levels.Count == 0) return new WeaponLevelData();
        return levels[Mathf.Clamp(RarityHelper, 0, levels.Count - 1)];
    }
}

[System.Serializable]
public class WeaponLevelData
{
    public int damage = 10;
    public float fireRate = 1f;
    public float range = 15f;
    [TextArea(1, 2)] public string description = "";
}