using System;
using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    [Header("Fighting")]
    [SerializeField]
    private int life = 100;

    public event Action<GameObject> OnDeath;


    void Start()
    {
    }

    public void Damage(int damage)
    {
        life -= damage;



        if (life <= 0)
        {
            OnDeath?.Invoke(gameObject);

            if (gameObject.CompareTag("Player"))
            {
                // Debug.Log("Player has died. Game Over.");
            }

            if (gameObject.CompareTag("Enemy"))
            {
                // Destroy(gameObject);
            }
        }
    }
}
