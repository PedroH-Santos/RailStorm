using System;
using UnityEngine;

[Serializable]
public class EmberRainLevelData : SkillLevelData
{
    [Tooltip("Dano de cada pulso em cada inimigo dentro da área.")]
    public int damagePerPulse = 6;
    [Min(0.1f)] public float radius = 3f;
    [Tooltip("Segundos de chuva por lançamento.")]
    [Min(0.1f)] public float duration = 3f;
    [Tooltip("Distância máxima do vagão até onde o marcador chega.")]
    [Min(0f)] public float range = 12f;

    public EmberRainLevelData()
    {
        cooldown = 6f;
        upgradeCost = 30;
    }

    public override float GetStat(ESkillStatTarget stat) => stat switch
    {
        ESkillStatTarget.Damage => damagePerPulse,
        ESkillStatTarget.Cooldown => cooldown,
        ESkillStatTarget.Radius => radius,
        ESkillStatTarget.Duration => duration,
        ESkillStatTarget.Range => range,
        _ => 0f,
    };
}
