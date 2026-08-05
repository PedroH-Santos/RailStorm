using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHordeEventConfig", menuName = "Events/Horde Event Config")]
public class HordeEventConfig : ScriptableObject
{
    [Header("Inimigos")]
    public List<HordeEnemyEntry> enemies = new();
    public int totalEnemies = 20;
    public float spawnInterval = 1f;
    public int spawnBatchSize = 6;

    [Header("Dificuldade")]
    [Tooltip("Multiplicador aplicado ao dano dos inimigos da horda (ex: 1.25 = +25%).")]
    public float damageMultiplier = 1.25f;

    [Header("Recompensa")]
    public int coinsPerKill = 2;
    public int coinsOnComplete = 50;

    [Header("Segurança")]
    [Tooltip("Tempo máximo da horda em segundos, mesmo que ainda haja inimigos vivos. 0 = sem limite.")]
    public float eventDuration = 0f;
}
