using System;
using UnityEngine;
using StarterAssets;

public class LifeSystem : MonoBehaviour
{
    [Header("Fighting")]
    [SerializeField] private int life = 100;

    public event Action<int> OnDamaged;
    public event Action<GameObject> OnDeath;
    public static event Action<GameObject> OnAnyDeath;

    PlayerStatsAggregator _playerStats;
    bool _dead;

    public bool IsDead => _dead;

    bool BelongsToPlayer => _playerStats != null;

    void Awake()
    {
        _playerStats = GetComponent<PlayerStatsAggregator>();
    }

    public void Damage(int damage)
    {
        if (_dead) return;

        OnDamaged?.Invoke(damage);

        if (BelongsToPlayer) DamagePlayer(damage);
        else DamageEnemy(damage);
    }

    void DamagePlayer(int damage)
    {
        _playerStats.HP -= damage;
        if (_playerStats.HP <= 0) Die();
    }

    void DamageEnemy(int damage)
    {
        life -= damage;
        if (life > 0) return;

        Die();
        Destroy(gameObject);
    }

    void Die()
    {
        _dead = true;
        OnDeath?.Invoke(gameObject);
        OnAnyDeath?.Invoke(gameObject);
    }
}
