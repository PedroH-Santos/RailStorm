using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeaponSkill", menuName = "Car/Weapon Skill Definition")]
public class WeaponSkillDefinition : ScriptableObject, IDrawable
{
    public string skillName = "Nova Skill de Arma";
    public Sprite icon;
    [TextArea] public string description = "";

    public EWeaponType weaponType;
    public EWeaponStatTarget statTarget;

    public List<SkillLevelData> levels = new()
    {
        new SkillLevelData { statValue = 10f, isMultiplier = true },
        new SkillLevelData { statValue = 20f, isMultiplier = true },
        new SkillLevelData { statValue = 30f, isMultiplier = true },
    };

    public string DisplayName => skillName;
    public Sprite Icon => icon;

    public int LevelCount => levels.Count;
    public int MaxRarity => levels.Count - 1;

    public SkillLevelData GetLevelForRarity(int rarityIndex)
    {
        if (levels.Count == 0) return new SkillLevelData();
        return levels[Mathf.Clamp(rarityIndex, 0, levels.Count - 1)];
    }
}
