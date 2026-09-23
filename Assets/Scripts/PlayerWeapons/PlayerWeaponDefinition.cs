using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerWeapon", menuName = "Player Weapons/Player Weapon Definition")]
public class PlayerWeaponDefinition : ScriptableObject, IDrawable
{
    public string weaponName = "Nova Arma";
    public Sprite icon;
    [TextArea] public string description = "";
    public List<SkillDefinition> availableSkills = new();

    public string DisplayName => weaponName;
    public Sprite Icon => icon;

    public bool Supports(SkillDefinition skill)
        => skill != null && (availableSkills.Count == 0 || availableSkills.Contains(skill));
}
