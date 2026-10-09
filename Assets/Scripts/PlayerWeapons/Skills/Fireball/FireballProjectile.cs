using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    const int MaxCollidersInHitBox = 32;
    const float MinPathLengthSqr = 0.000001f;

    [Header("Acerto")]
    [Tooltip("Meia largura da faixa que acerta inimigos, para os lados do voo.")]
    [SerializeField] private float hitRadius = 0.5f;
    [Tooltip("Quanto abaixo do voo ainda acerta. Cobre inimigos em terreno mais baixo que o vagão.")]
    [SerializeField] private float hitDepthBelow = 2.5f;
    [Tooltip("Quanto acima do voo ainda acerta.")]
    [SerializeField] private float hitHeightAbove = 0.5f;

    [Header("Perseguição do alvo da mira")]
    [Tooltip("Graus por segundo que a bola vira na direção do inimigo escolhido pela ajuda na mira. 0 = voa reto.")]
    [SerializeField] private float homingTurnDegreesPerSecond = 270f;

    [Header("Visual")]
    [SerializeField] private GameObject impactPrefab;
    [Tooltip("Filhos soltos do projétil ao sumir, para o rastro terminar de desaparecer sozinho.")]
    [SerializeField] private ParticleSystem[] detachOnDestroy;
    [SerializeField] private float detachedLifetime = 0.6f;

    readonly Collider[] _collidersInHitBox = new Collider[MaxCollidersInHitBox];

    Rigidbody _body;
    Vector3 _launchPosition;
    Vector3 _lastCheckedPosition;
    float _range;
    float _speed;
    SkillHit _hit;
    LifeSystem _target;
    bool _exploded;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, FireballLevelData stats, SkillHit hit, LifeSystem target)
    {
        _launchPosition = transform.position;
        _lastCheckedPosition = _launchPosition;
        _range = stats.range;
        _speed = stats.speed;
        _hit = hit;
        _target = target;

        FlyTowards(direction);
    }

    void Update()
    {
        if (!_exploded && HasTraveledFullRange()) Explode();
    }

    void FixedUpdate()
    {
        if (_exploded) return;

        TurnTowardsTarget(Time.fixedDeltaTime);

        Vector3 position = _body.position;
        if (TryFindEnemyAlongPath(_lastCheckedPosition, position, out var enemy))
        {
            _hit.ApplyTo(enemy);
            Explode();
            return;
        }

        _lastCheckedPosition = position;
    }

    void TurnTowardsTarget(float deltaTime)
    {
        if (_target == null || _target.IsDead || homingTurnDegreesPerSecond <= 0f) return;

        Vector3 currentDirection = _body.linearVelocity.normalized;
        Vector3 toTarget = EnemyTargeting.BodyCenter(_target) - _body.position;
        bool targetIsBehind = Vector3.Dot(currentDirection, toTarget) <= 0f;
        if (targetIsBehind) return;

        float maxTurnRadians = homingTurnDegreesPerSecond * Mathf.Deg2Rad * deltaTime;
        FlyTowards(Vector3.RotateTowards(currentDirection, toTarget.normalized, maxTurnRadians, 0f));
    }

    void FlyTowards(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        _body.linearVelocity = direction.normalized * _speed;
    }

    bool TryFindEnemyAlongPath(Vector3 from, Vector3 to, out LifeSystem enemy)
    {
        enemy = null;
        Vector3 pathOnGround = to - from;
        pathOnGround.y = 0f;
        float pathLength = pathOnGround.magnitude;

        Quaternion pathRotation = pathOnGround.sqrMagnitude > MinPathLengthSqr ? Quaternion.LookRotation(pathOnGround) : Quaternion.identity;
        Vector3 center = (from + to) * 0.5f + Vector3.up * ((hitHeightAbove - hitDepthBelow) * 0.5f);
        Vector3 halfExtents = new Vector3(hitRadius, (hitHeightAbove + hitDepthBelow) * 0.5f, pathLength * 0.5f + hitRadius);

        int found = Physics.OverlapBoxNonAlloc(center, halfExtents, _collidersInHitBox, pathRotation, ~0, QueryTriggerInteraction.Collide);
        float closestDistance = float.MaxValue;

        for (int i = 0; i < found; i++)
        {
            if (!EnemyTargeting.TryGetLivingEnemy(_collidersInHitBox[i], out var candidate)) continue;

            float distance = Vector3.Distance(from, candidate.transform.position);
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            enemy = candidate;
        }

        return enemy != null;
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
