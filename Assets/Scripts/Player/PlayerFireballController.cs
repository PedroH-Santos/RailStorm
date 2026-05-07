using System;
using UnityEngine;

public class PlayerFireballController : MonoBehaviour
{
    [Header("Movement")]
    private float speed = 25f;
    private float range = 15f;
    private int damage = 15;


    private Vector3 _startPosition;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Init(Vector3 direction, float speedTmp, float rangeTmp, int damageTmp)
    {
        _startPosition = transform.position;

        speed = speedTmp;
        range = rangeTmp;
        damage = damageTmp;

        _rb.linearVelocity = direction * speed;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

    }

    void Update()
    {
        float traveledDistance = Vector3.Distance(_startPosition, transform.position);

        if (traveledDistance >= range)
        {
            Destroy(gameObject);
        }

    }

    void OnTriggerEnter(Collider other)
    {

        var target = other.gameObject;

        if(target.CompareTag("Enemy"))
        {
            
            if (!target.TryGetComponent<LifeSystem>(out var lifeSystem))
            {
                return;
            }

            lifeSystem.Damage(damage);
            Destroy(gameObject);

        }

    }
}
