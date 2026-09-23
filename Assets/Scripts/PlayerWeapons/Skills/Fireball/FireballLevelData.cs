using System;
using UnityEngine;

[Serializable]
public class FireballLevelData
{
    public int damage = 15;
    public float speed = 25f;
    public float range = 15f;
    [Tooltip("Segundos entre disparos. 0 = sem recarga.")]
    [Min(0f)] public float cooldown = 0.3f;
    [Min(1)] public int projectileCount = 1;
    [Min(0f)] public float spreadAngle = 0f;
    [Tooltip("Moedas para subir deste nível para o seguinte. Ignorado no último nível.")]
    [Min(0)] public int upgradeCost = 20;
}
