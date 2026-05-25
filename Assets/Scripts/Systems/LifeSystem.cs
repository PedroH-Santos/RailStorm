using System;
using UnityEngine;
using StarterAssets;

public class LifeSystem : MonoBehaviour
{
    [Header("Fighting")]
    [SerializeField] private int life = 100;

    public event Action<GameObject> OnDeath;

    PlayerStatsAggregator _stats;

    void Awake()
    {
        _stats = GetComponent<PlayerStatsAggregator>();
    }

    public void Damage(int damage)
    {
        if (_stats != null)
        {
            _stats.HP -= damage;

            if (_stats.HP <= 0)
                OnDeath?.Invoke(gameObject);
        }
        else
        {
            life -= damage;

            if (life <= 0)
            {
                OnDeath?.Invoke(gameObject);
                Destroy(gameObject);
            }
        }
    }
}