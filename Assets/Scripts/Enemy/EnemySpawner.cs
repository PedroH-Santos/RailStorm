using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WaveDefinition
{
    public string waveName = "Wave";
    public int totalEnemies = 10;
    public List<EnemySpawnEntry> enemies = new();
}

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;
    public int minCount = 1;
    [Range(0f, 1f)]
    public float weight = 0.5f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Waves")]
    public List<WaveDefinition> waves = new();
    public float spawnInterval = 2f;
    public int spawnBatchSize = 4;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(20, 0, 20);

    [Header("References")]
    public Transform player;
    public StarterAssets.PlayerController playerController;

    int _currentWave = 0;
    int _aliveEnemies = 0;
    bool _waveInProgress = false;

    public int CurrentWave => _currentWave;
    public bool WaveInProgress => _waveInProgress;

    public static event System.Action OnWaveCleared;

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (_currentWave < waves.Count)
        {
            yield return StartCoroutine(SpawnWave(waves[_currentWave]));

            yield return new WaitUntil(() => _aliveEnemies <= 0);

            OnWaveFinished();

            if (_currentWave < waves.Count)
            {
                Debug.Log($"[Waves] Next wave in {timeBetweenWaves}s...");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        Debug.Log("[Waves] All waves complete!");
    }

    IEnumerator SpawnWave(WaveDefinition wave)
    {
        _waveInProgress = true;
        Debug.Log($"[Waves] Starting: {wave.waveName}");

        List<GameObject> pool = BuildPool(wave);

        // Fisher-Yates shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int spawned = 0;
        while (spawned < pool.Count)
        {
            int batch = Mathf.Min(spawnBatchSize, pool.Count - spawned);
            for (int i = 0; i < batch; i++)
                SpawnEnemy(pool[spawned++]);

            if (spawned < pool.Count)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    List<GameObject> BuildPool(WaveDefinition wave)
    {
        List<GameObject> pool = new();

        Dictionary<EnemySpawnEntry, int> allocated = new();

        // 1. Garante o mínimo de cada tipo
        int usedSlots = 0;
        foreach (var entry in wave.enemies)
        {
            allocated[entry] = entry.minCount;
            usedSlots += entry.minCount;
        }

        // 2. Distribui os slots restantes pelo weight
        int remainingSlots = Mathf.Max(0, wave.totalEnemies - usedSlots);
        float totalWeight = wave.enemies.Sum(e => e.weight);

        // Distribui proporcionalmente
        int distributed = 0;
        EnemySpawnEntry heaviest = wave.enemies.OrderByDescending(e => e.weight).First();

        foreach (var entry in wave.enemies)
        {
            if (entry == heaviest) continue; // deixa o mais pesado absorver o resto

            int extra = Mathf.FloorToInt((entry.weight / totalWeight) * remainingSlots);
            allocated[entry] += extra;
            distributed += extra;
        }

        // O mais pesado absorve qualquer resto de arredondamento
        allocated[heaviest] += remainingSlots - distributed;

        // 3. Monta a pool final
        foreach (var entry in wave.enemies)
        {
            for (int i = 0; i < allocated[entry]; i++)
                pool.Add(entry.enemyPrefab);
        }

        return pool;
    }

    void SpawnEnemy(GameObject prefab)
    {
        Vector3 pos = GetRandomPosition();
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        _aliveEnemies++;

        if (enemy.TryGetComponent<Enemy>(out var enemyScript))
            enemyScript.SetTarget(player);

        if (enemy.TryGetComponent<LifeSystem>(out var ls))
            ls.OnDeath += OnEnemyDeath;
    }

    void OnEnemyDeath(GameObject entity)
    {
        if (entity.TryGetComponent<LifeSystem>(out var ls))
            ls.OnDeath -= OnEnemyDeath;

        if (entity.CompareTag("Enemy"))
            _aliveEnemies--;
    }

    void OnWaveFinished()
    {
        _waveInProgress = false;
        _currentWave++;
        OnWaveCleared?.Invoke();
        if (playerController != null)
        {
            playerController.Coins += 10;
            Debug.Log($"[Waves] Wave cleared! +10 coins → {playerController.Coins}");
        }
    }

    Vector3 GetRandomPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            0f,
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }

}