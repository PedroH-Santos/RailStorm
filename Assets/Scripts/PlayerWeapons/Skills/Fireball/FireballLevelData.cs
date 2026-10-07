using System;

[Serializable]
public class FireballLevelData : SkillLevelData
{
    public int damage = 15;
    public float speed = 25f;
    public float range = 15f;

    public FireballLevelData()
    {
        cooldown = 0.3f;
        upgradeCost = 20;
    }

    public override float GetStat(ESkillStatTarget stat) => stat switch
    {
        ESkillStatTarget.Damage => damage,
        ESkillStatTarget.Cooldown => cooldown,
        ESkillStatTarget.Range => range,
        ESkillStatTarget.Speed => speed,
        _ => 0f,
    };
}
