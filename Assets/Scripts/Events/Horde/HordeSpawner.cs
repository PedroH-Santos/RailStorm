using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;

public class HordeSpawner : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private HordeEventConfig config;

    [Header("Área de spawn (independente do EnemySpawner)")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20, 0, 20);

    [Header("Referências")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerStatsAggregator playerStatsAggregator;

    int _hordeAliveEnemies = 0;
    bool _hordeActive = false;

    public bool IsActive => _hordeActive;

    public static event System.Action OnHordeStarted;
    public static event System.Action OnHordeEnded;

    public void TriggerHorde()
    {
        if (_hordeActive || config == null) return;
        StartCoroutine(RunHorde());
    }

    IEnumerator RunHorde()
    {
        _hordeActive = true;
        HordeDamageMultiplier.Active = true;
        HordeDamageMultiplier.Multiplier = config.damageMultiplier;
        OnHordeStarted?.Invoke();

        List<GameObject> pool = BuildPool(config);

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int spawned = 0;
        while (spawned < pool.Count)
        {
            int batch = Mathf.Min(config.spawnBatchSize, pool.Count - spawned);
            for (int i = 0; i < batch; i++)
                SpawnEnemy(pool[spawned++]);

            if (spawned < pool.Count)
                yield return new WaitForSeconds(config.spawnInterval);
        }

        if (config.eventDuration > 0f)
        {
            float elapsed = 0f;
            while (_hordeAliveEnemies > 0 && elapsed < config.eventDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitUntil(() => _hordeAliveEnemies <= 0);
        }

        FinishHorde();
    }

    List<GameObject> BuildPool(HordeEventConfig cfg)
    {
        List<GameObject> pool = new();
        if (cfg.enemies == null || cfg.enemies.Count == 0) return pool;

        Dictionary<HordeEnemyEntry, int> allocated = new();

        int usedSlots = 0;
        foreach (var entry in cfg.enemies)
        {
            allocated[entry] = entry.minCount;
            usedSlots += entry.minCount;
        }

        int remainingSlots = Mathf.Max(0, cfg.totalEnemies - usedSlots);
        float totalWeight = cfg.enemies.Sum(e => e.weight);
        HordeEnemyEntry heaviest = cfg.enemies.OrderByDescending(e => e.weight).First();

        int distributed = 0;
        foreach (var entry in cfg.enemies)
        {
            if (entry == heaviest) continue;

            int extra = totalWeight > 0f ? Mathf.FloorToInt((entry.weight / totalWeight) * remainingSlots) : 0;
            allocated[entry] += extra;
            distributed += extra;
        }
        allocated[heaviest] += remainingSlots - distributed;

        foreach (var entry in cfg.enemies)
            for (int i = 0; i < allocated[entry]; i++)
                pool.Add(entry.enemyPrefab);

        return pool;
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            0f,
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2));

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        _hordeAliveEnemies++;

        if (enemy.TryGetComponent<Enemy>(out var enemyScript) && player != null)
            enemyScript.SetTarget(player);

        if (enemy.TryGetComponent<LifeSystem>(out var ls))
            ls.OnDeath += OnHordeEnemyDeath;
    }

    void OnHordeEnemyDeath(GameObject entity)
    {
        if (entity.TryGetComponent<LifeSystem>(out var ls))
            ls.OnDeath -= OnHordeEnemyDeath;

        if (!entity.CompareTag("Enemy")) return;

        _hordeAliveEnemies--;

        if (playerStatsAggregator != null)
            playerStatsAggregator.Coins += config.coinsPerKill;
    }

    void FinishHorde()
    {
        HordeDamageMultiplier.Active = false;

        bool playerAlive = playerStatsAggregator == null || playerStatsAggregator.HP > 0;
        if (playerAlive && playerStatsAggregator != null)
            playerStatsAggregator.Coins += config.coinsOnComplete;

        _hordeActive = false;
        OnHordeEnded?.Invoke();
    }
}
