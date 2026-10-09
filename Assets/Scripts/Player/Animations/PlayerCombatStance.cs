using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerCombatStance : MonoBehaviour
{
    PlayerAnimationController _animation;

    void Awake()
    {
        _animation = GetComponent<PlayerAnimationController>();
    }

    void OnEnable()
    {
        EnemySpawner.OnWaveStarted += _animation.EnterCombatStance;
        EnemySpawner.OnWaveCleared += _animation.LeaveCombatAndCelebrate;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= _animation.EnterCombatStance;
        EnemySpawner.OnWaveCleared -= _animation.LeaveCombatAndCelebrate;
    }

    void Start()
    {
        var spawner = FindAnyObjectByType<EnemySpawner>();
        bool joinedDuringAWave = spawner != null && spawner.WaveInProgress;
        if (joinedDuringAWave) _animation.EnterCombatStance();
    }
}
