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

    [SerializeReference]
    public List<WeaponLevelData> levels = new();

    [SerializeField] private List<WeaponSkillDefinition> _appliedSkills = new();

    WeaponLevelData _effectiveCache;
    int _effectiveCacheRarity = int.MinValue;

    public string DisplayName => weaponName;
    public Sprite Icon => icon;
    public int CurrentRarity => currentRarity;
    public bool IsAcquired => currentRarity >= 0;
    public bool CanUpgrade => IsAcquired && currentRarity < levels.Count - 1;
    public int NextRarity => currentRarity + 1;

    public IReadOnlyList<WeaponSkillDefinition> AppliedSkills => _appliedSkills;

    public WeaponLevelData GetStatsForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return null;
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }

    public T GetStats<T>(int rarityIndex) where T : WeaponLevelData
        => GetStatsForRarity(rarityIndex) as T;

    public WeaponLevelData GetEffectiveStats()
    {
        if (_effectiveCache != null && _effectiveCacheRarity == currentRarity)
            return _effectiveCache;

        var baseStats = GetStatsForRarity(Mathf.Max(currentRarity, 0));
        var stats = baseStats?.Clone();

        if (stats != null)
        {
            foreach (var skill in _appliedSkills)
            {
                if (!skill.IsAcquired) continue;
                var data = skill.GetLevelForRarity(skill.CurrentRarity);
                stats.ApplyModifier(skill.statTarget, data.statValue, data.isMultiplier);
            }
        }

        _effectiveCache = stats;
        _effectiveCacheRarity = currentRarity;
        return _effectiveCache;
    }

    public T GetEffectiveStats<T>() where T : WeaponLevelData
        => GetEffectiveStats() as T;

    public WeaponLevelData CurrentStats => GetStatsForRarity(Mathf.Max(currentRarity, 0));
    public WeaponLevelData NextStats => GetStatsForRarity(Mathf.Min(currentRarity + 1, levels.Count - 1));

    public void Acquire(int rarityIndex)
    {
        currentRarity = Mathf.Clamp(rarityIndex, 0, levels.Count - 1);
        InvalidateCache();
    }

    public bool Upgrade(int targetRarity)
    {
        if (!CanUpgrade || targetRarity <= currentRarity || targetRarity >= levels.Count)
            return false;
        currentRarity = targetRarity;
        InvalidateCache();
        return true;
    }

    public bool ApplyWeaponSkill(WeaponSkillDefinition skill, int rarityIndex)
    {
        if (skill == null || skill.weaponType != weaponType) return false;

        if (rarityIndex <= skill.CurrentRarity) return false;

        if (!skill.IsAcquired) skill.Acquire(rarityIndex);
        else skill.Upgrade(rarityIndex);

        if (!_appliedSkills.Contains(skill))
            _appliedSkills.Add(skill);

        InvalidateCache();
        return true;
    }

    public bool HasSkill(WeaponSkillDefinition skill) => _appliedSkills.Contains(skill);

    void InvalidateCache() => _effectiveCache = null;

    public void ResetForNewRun()
    {
        currentRarity = -1;
        _appliedSkills.Clear();
        InvalidateCache();
    }
}