using System;
using UnityEngine;
using StarterAssets;

public class LifeSystem : MonoBehaviour
{
    [Header("Fighting")]
    [SerializeField] private int life = 100;

    public event Action<GameObject> OnDeath;
    public static event Action<GameObject> OnAnyDeath;

    PlayerStatsAggregator _stats;
    bool _dead;

    public bool IsDead => _dead;

    void Awake()
    {
        _stats = GetComponent<PlayerStatsAggregator>();
    }

    public void Damage(int damage)
    {
        if (_dead) return;

        if (_stats != null)
        {
            _stats.HP -= damage;

            if (_stats.HP <= 0)
                Die();
        }
        else
        {
            life -= damage;

            if (life <= 0)
            {
                Die();
                Destroy(gameObject);
            }
        }
    }

    void Die()
    {
        _dead = true;
        OnDeath?.Invoke(gameObject);
        OnAnyDeath?.Invoke(gameObject);
    }
}
