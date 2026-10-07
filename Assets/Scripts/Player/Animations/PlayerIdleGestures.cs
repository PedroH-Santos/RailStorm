using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerIdleGestures : MonoBehaviour
{
    [Tooltip("Segundos parado na Idle relaxada até um gesto (sorteado entre os dois valores).")]
    [SerializeField] private Vector2 gestureInterval = new(8f, 15f);
    [Tooltip("Quantos gestos o Animator tem (parâmetro gestureIndex).")]
    [SerializeField] private int gestureCount = 2;

    PlayerAnimationController _animation;
    float _secondsUntilGesture;
    bool _wasInCombat;

    void Awake()
    {
        _animation = GetComponent<PlayerAnimationController>();
        RestartWait();
    }

    void Update()
    {
        bool gameIsRunning = Time.timeScale > 0f;
        if (!gameIsRunning) return;

        RestartWaitWhenCombatEnds();

        if (!_animation.IsInRelaxedIdle)
        {
            KeepAtLeastHalfTheWait();
            return;
        }

        _secondsUntilGesture -= Time.deltaTime;
        if (_secondsUntilGesture > 0f) return;

        _animation.PlayGesture(Random.Range(0, Mathf.Max(1, gestureCount)));
        RestartWait();
    }

    void RestartWaitWhenCombatEnds()
    {
        bool combatJustEnded = _wasInCombat && !_animation.InCombat;
        _wasInCombat = _animation.InCombat;
        if (combatJustEnded) RestartWait();
    }

    void KeepAtLeastHalfTheWait()
    {
        if (_secondsUntilGesture < gestureInterval.x * 0.5f) RestartWait();
    }

    void RestartWait()
    {
        _secondsUntilGesture = Random.Range(gestureInterval.x, gestureInterval.y);
    }
}
