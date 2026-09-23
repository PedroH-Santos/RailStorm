using UnityEngine.Serialization;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPerk", menuName = "Perks/Perk Definition")]
public class PerkDefinition : ScriptableObject, IDrawable
{
    [FormerlySerializedAs("skillName")] public string perkName = "Novo Perk";
    public Sprite icon;
    [TextArea] public string description = "";
    public EStatTarget statTarget;

    public List<PerkLevelData> levels = new()
    {
        new PerkLevelData { statValue = 5f,  isMultiplier = false },
        new PerkLevelData { statValue = 10f, isMultiplier = false },
        new PerkLevelData { statValue = 15f, isMultiplier = false },
        new PerkLevelData { statValue = 20f, isMultiplier = false },
        new PerkLevelData { statValue = 25f, isMultiplier = false },
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
