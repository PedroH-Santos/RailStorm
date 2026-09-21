using System;
using UnityEngine;

public class RunTracker : MonoBehaviour
{
    public static RunTracker Instance { get; private set; }

    [Tooltip("Canvas do HUD, ligado por este componente no início da run.")]
    [SerializeField] private GameObject hudRoot;

    public float ElapsedSeconds { get; private set; }
    public int EnemiesKilled { get; private set; }

    public event Action OnKillsChanged;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable() => LifeSystem.OnAnyDeath += HandleDeath;

    void OnDisable() => LifeSystem.OnAnyDeath -= HandleDeath;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    void Update()
    {
        ElapsedSeconds += Time.deltaTime;
    }

    void HandleDeath(GameObject entity)
    {
        if (entity == null || !entity.CompareTag("Enemy")) return;

        EnemiesKilled++;
        OnKillsChanged?.Invoke();
    }

    public void ResetForNewRun()
    {
        ElapsedSeconds = 0f;
        EnemiesKilled = 0;
        OnKillsChanged?.Invoke();
    }
}
