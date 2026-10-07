using System;
using UnityEngine;

[Serializable]
public abstract class SkillLevelData
{
    [Tooltip("Segundos entre usos. 0 = sem recarga.")]
    [Min(0f)] public float cooldown;
    [Tooltip("Moedas para subir deste nível para o seguinte. Ignorado no último nível.")]
    [Min(0)] public int upgradeCost;

    public abstract float GetStat(ESkillStatTarget stat);
}
