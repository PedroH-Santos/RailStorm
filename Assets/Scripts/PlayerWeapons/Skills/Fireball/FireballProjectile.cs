using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject impactPrefab;
    [Tooltip("Filhos soltos do projétil ao sumir, para o rastro terminar de desaparecer sozinho.")]
    [SerializeField] private ParticleSystem[] detachOnDestroy;
    [SerializeField] private float detachedLifetime = 0.6f;

    Rigidbody _body;
    Vector3 _launchPosition;
    float _range;
    SkillHit _hit;
    bool _exploded;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, FireballLevelData stats, SkillHit hit)
    {
        _launchPosition = transform.position;
        _range = stats.range;
        _hit = hit;

        transform.rotation = Quaternion.LookRotation(direction);
        _body.linearVelocity = direction * stats.speed;
    }

    void Update()
    {
        if (!_exploded && HasTraveledFullRange()) Explode();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_exploded || !EnemyTargeting.TryGetLivingEnemy(other, out var enemy)) return;

        _hit.ApplyTo(enemy);
        Explode();
    }

    bool HasTraveledFullRange() => Vector3.Distance(_launchPosition, transform.position) >= _range;

    void Explode()
    {
        _exploded = true;
        SpawnImpact();
        LetTrailFadeOutAlone();
        Destroy(gameObject);
    }

    void SpawnImpact()
    {
        if (impactPrefab != null) Instantiate(impactPrefab, transform.position, transform.rotation);
    }

    void LetTrailFadeOutAlone()
    {
        foreach (var trail in detachOnDestroy)
        {
            if (trail == null) continue;

            trail.transform.SetParent(null, true);
            trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(trail.gameObject, detachedLifetime);
        }
    }
}
