using System;
using UnityEngine;

[Serializable]
public class EmberRainLevelData
{
    [Tooltip("Dano de cada pulso em cada inimigo dentro da área.")]
    public int damagePerPulse = 6;
    [Min(0f)] public float cooldown = 6f;
    [Min(0.1f)] public float radius = 3f;
    [Tooltip("Segundos de chuva por lançamento.")]
    [Min(0.1f)] public float duration = 3f;
    [Tooltip("Distância máxima do vagão até onde o marcador chega.")]
    [Min(0f)] public float range = 12f;
    [Tooltip("Moedas para subir deste nível para o seguinte. Ignorado no último nível.")]
    [Min(0)] public int upgradeCost = 30;
}
