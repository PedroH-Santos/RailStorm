using UnityEngine;

[System.Serializable]
public class SkillLevel
{
    [TextArea] public string description;
    public float statValue;
    public bool isMultiplier;
}