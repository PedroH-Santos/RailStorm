using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject impactPrefab;
    [Tooltip("Filhos soltos do projétil ao sumir, para o rastro terminar de desaparecer sozinho.")]
    [SerializeField] private ParticleSystem[] detachOnDestroy;
    [SerializeField] private float detachedLifetime = 0.6f;

    float _range = 15f;
    int _damage = 15;
    BurnDefinition _burn;
    int _burnStacks;
    Vector3 _startPosition;
    Rigidbody _rb;
    bool _finished;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Init(Vector3 direction, float speed, float range, int damage, BurnDefinition burn, int burnStacks)
    {
        _startPosition = transform.position;
        _range = range;
        _damage = damage;
        _burn = burn;
        _burnStacks = burnStacks;

        _rb.linearVelocity = direction * speed;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        if (_finished) return;

        if (Vector3.Distance(_startPosition, transform.position) >= _range)
            Finish();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_finished) return;

        var target = other.gameObject;
        if (!target.CompareTag("Enemy")) return;
        if (!target.TryGetComponent<LifeSystem>(out var lifeSystem)) return;

        lifeSystem.Damage(_damage);
        if (!lifeSystem.IsDead) BurnReceiver.ApplyStack(target, _burn, _burnStacks);

        Finish();
    }

    void Finish()
    {
        _finished = true;

        if (impactPrefab != null)
            Instantiate(impactPrefab, transform.position, transform.rotation);

        foreach (var system in detachOnDestroy)
        {
            if (system == null) continue;

            system.transform.SetParent(null, true);
            system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(system.gameObject, detachedLifetime);
        }

        Destroy(gameObject);
    }
}
