// ArrowProjectile.cs
// Handles a single arrow: forward movement, range limit and damage on hit.
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArrowProjectile : MonoBehaviour
{
    private Rigidbody _rb;
    private int _damage;
    private float _speed;
    private float _range;
    private Vector3 _origin;
    private bool _initialized;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Update()
    {
        if (!_initialized) return;

        if (Vector3.Distance(_origin, transform.position) >= _range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent<LifeSystem>(out var lifeSystem))
            lifeSystem.Damage(_damage);

        Destroy(gameObject);
    }

    /// <summary>
    /// Puts the arrow in motion. Must be called immediately after Instantiate.
    /// All values come from <see cref="WeaponLevelData"/> via ArrowWeaponBehaviour.
    /// </summary>
    public void Init(Vector3 direction, float speed, float range, int damage)
    {
        _speed = speed;
        _range = range;
        _damage = damage;
        _origin = transform.position;
        _initialized = true;

        Vector3 dir = direction.sqrMagnitude < 0.0001f ? transform.forward : direction.normalized;
        dir.y = 0f;

        _rb.linearVelocity = dir * _speed;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}