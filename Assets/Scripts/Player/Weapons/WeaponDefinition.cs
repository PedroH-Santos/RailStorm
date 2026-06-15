using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Car/Weapon Definition")]
public class WeaponDefinition : ScriptableObject, IDrawable
{
    [Header("Info")]
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Nível atual (0 = não adquirida, 1-5 = Common→Legendary)")]
    [SerializeField, Range(0, 4)]
    private int currentLevelIndex = 0; 

    [Header("5 Níveis de Stats (índice 0=Common … 4=Legendary)")]
    public List<WeaponLevelData> levels = new();
    public int CurrentLevel => currentLevelIndex + 1;
    public bool IsAcquired => currentLevelIndex >= 0;
    public bool CanUpgrade => currentLevelIndex < 4; 
    public int NextLevel => Mathf.Min(currentLevelIndex + 2, 5);

    public WeaponLevelData CurrentStats => GetLevelData(currentLevelIndex);
    public WeaponLevelData NextStats => GetLevelData(Mathf.Min(currentLevelIndex + 1, 4));

    // ── IDrawable ────────────────────────────────────────────────────────────

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public ESkillRarity Rarity => ESkillRarityExtensions.FromLevel(CurrentLevel);

    public float GetWeight(int targetLevel, float luckPercent) =>
        WeightTable.GetWeight(targetLevel, luckPercent);


    public void Acquire()
    {
        currentLevelIndex = 0;
        MarkDirty();
    }

    public bool Upgrade()
    {
        if (!CanUpgrade) return false;
        currentLevelIndex++;
        MarkDirty();
        return true;
    }

    public void Reset() { currentLevelIndex = 0; MarkDirty(); }

    // ── Helpers ──────────────────────────────────────────────────────────────

    public WeaponLevelData GetLevelData(int index) =>
        levels.Count > 0
            ? levels[Mathf.Clamp(index, 0, levels.Count - 1)]
            : new WeaponLevelData();

    public ESkillRarity RarityForLevel(int humanLevel) =>
        ESkillRarityExtensions.FromLevel(humanLevel);

    void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        while (levels.Count < 5)
        {
            int next = levels.Count + 1;
            levels.Add(new WeaponLevelData
            {
                damage = next * 5,
                fireRate = 1f + (next - 1) * 0.2f,
                range = 15f + (next - 1) * 3f,
                description = $"Nível {next} — {ESkillRarityExtensions.FromLevel(next).DisplayName()}"
            });
        }
        if (levels.Count > 5) levels.RemoveRange(5, levels.Count - 5);
    }
#endif
}

[System.Serializable]
public class WeaponLevelData
{
    public int damage = 10;
    public float fireRate = 1f;
    public float range = 15f;
    [TextArea(1, 2)] public string description = "";
}
