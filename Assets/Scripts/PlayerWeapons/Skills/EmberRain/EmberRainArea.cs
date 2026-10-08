using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EmberRainArea : MonoBehaviour
{
    const int MaxCollidersPerPulse = 64;
    const float MinFallSeconds = 0.05f;
    const float LifetimeBeyondLanding = 2f;

    [Header("Visual")]
    [Tooltip("Visual do chão escalado pelo raio. Escala 1 deve cobrir raio 1.")]
    [SerializeField] private Transform areaRoot;
    [Tooltip("Emissores cuja forma (shape) cresce com o raio, sem mudar altura nem tamanho das partículas. A forma deve cobrir raio 1.")]
    [SerializeField] private ParticleSystem[] radiusScaledEmitters;
    [Tooltip("Efeitos que rodam enquanto a chuva dura e param de emitir no fim.")]
    [SerializeField] private ParticleSystem[] continuousEffects;
    [SerializeField] private ParticleSystem pulseBurst;
    [SerializeField] private int pulseBurstCount = 6;
    [Tooltip("Tranco de escala do visual do chão a cada pulso, em fração do raio.")]
    [SerializeField] private float pulsePunch = 0.06f;

    [Header("Rajada de brasas (cai junto com cada pulso de dano)")]
    [Tooltip("Emissor dos meteoritos. Simula em World e some ao colidir com o plano do chão; o impacto vem dos sub-emissores de colisão.")]
    [SerializeField] private ParticleSystem volley;
    [SerializeField] private int ballsPerVolley = 9;
    [Tooltip("Altura de onde os meteoritos caem.")]
    [SerializeField] private float volleyDropHeight = 8f;
    [Tooltip("Deslocamento lateral da queda, em unidades por unidade de altura, no sentido da direita da tela.")]
    [SerializeField] private float slantTowardsScreenRight = 0.35f;
    [Tooltip("Segundos de queda. A rajada é lançada esse tempo antes do pulso, para tocar o chão junto com o dano.")]
    [SerializeField] private float volleyFallSeconds = 0.35f;
    [Tooltip("Fração do raio onde as brasas podem cair, para não baterem em cima do anel.")]
    [Range(0.1f, 1f)] [SerializeField] private float volleySpread = 0.85f;
    [SerializeField] private Vector2 ballSizeRange = new(0.45f, 1.1f);
    [Tooltip("Quanto maior, mais meteoritos pequenos e menos grandes. 1 = distribuição uniforme.")]
    [Range(1f, 4f)] [SerializeField] private float smallSizeBias = 2f;
    [Tooltip("Anel do chão que some no fim da chuva.")]
    [SerializeField] private Transform groundRing;
    [SerializeField] private float lingerSeconds = 1f;

    readonly Collider[] _collidersInRadius = new Collider[MaxCollidersPerPulse];
    readonly HashSet<LifeSystem> _enemiesHitThisPulse = new();

    EmberRainStorm _storm;
    SkillHit _hitPerPulse;
    float _elapsed;
    int _pulsesDone;
    int _volleysLaunched;
    bool _ended;

    public void Begin(EmberRainStorm storm, SkillHit hitPerPulse)
    {
        _storm = storm;
        _hitPerPulse = hitPerPulse;

        ScaleVisualsToRadius(storm.Radius);
        GrowGroundRing();
    }

    void Update()
    {
        if (_ended) return;

        _elapsed += Time.deltaTime;

        while (HasVolleyDue()) LaunchVolley();
        while (HasPulseDue()) Pulse();

        if (_elapsed >= _storm.Duration) End();
    }

    bool HasPulseDue()
    {
        int nextPulse = _pulsesDone + 1;
        return nextPulse <= _storm.TotalPulses && _elapsed >= _storm.TimeOfPulse(nextPulse);
    }

    bool HasVolleyDue()
    {
        int nextVolley = _volleysLaunched + 1;
        return nextVolley <= _storm.TotalPulses && _elapsed >= _storm.TimeOfPulse(nextVolley) - volleyFallSeconds;
    }

    void LaunchVolley()
    {
        _volleysLaunched++;
        if (volley == null) return;

        float fallSeconds = Mathf.Max(MinFallSeconds, volleyFallSeconds);
        Vector3 fallPath = Vector3.down * volleyDropHeight + ScreenRightOnGround() * (slantTowardsScreenRight * volleyDropHeight);
        var emitParams = new ParticleSystem.EmitParams
        {
            applyShapeToPosition = false,
            startLifetime = fallSeconds * LifetimeBeyondLanding,
            velocity = fallPath / fallSeconds,
        };

        for (int i = 0; i < ballsPerVolley; i++)
        {
            Vector2 landingOffset = Random.insideUnitCircle * (_storm.Radius * volleySpread);
            Vector3 landingPoint = transform.position + new Vector3(landingOffset.x, 0f, landingOffset.y);
            emitParams.position = landingPoint - fallPath;
            emitParams.startSize = RandomMeteorSize();
            volley.Emit(emitParams, 1);
        }
    }

    void Pulse()
    {
        _pulsesDone++;
        if (pulseBurst != null) pulseBurst.Emit(pulseBurstCount);
        PunchGroundVisual();
        HitEnemiesInRadius();
    }

    float RandomMeteorSize()
    {
        float biasedRoll = Mathf.Pow(Random.value, smallSizeBias);
        return Mathf.Lerp(ballSizeRange.x, ballSizeRange.y, biasedRoll);
    }

    void PunchGroundVisual()
    {
        if (groundRing == null || pulsePunch <= 0f) return;

        groundRing.DOComplete();
        groundRing.DOPunchScale(Vector3.one * pulsePunch, 0.2f, 4, 0.5f).SetLink(gameObject);
    }

    static Vector3 ScreenRightOnGround()
    {
        var camera = Camera.main;
        Vector3 right = camera != null ? camera.transform.right : Vector3.right;
        right.y = 0f;
        return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
    }

    void HitEnemiesInRadius()
    {
        _enemiesHitThisPulse.Clear();
        int found = Physics.OverlapSphereNonAlloc(transform.position, _storm.Radius, _collidersInRadius, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < found; i++)
        {
            if (!EnemyTargeting.TryGetLivingEnemy(_collidersInRadius[i], out var enemy)) continue;

            bool isFirstColliderOfThisEnemy = _enemiesHitThisPulse.Add(enemy);
            if (isFirstColliderOfThisEnemy) _hitPerPulse.ApplyTo(enemy);
        }
    }

    void End()
    {
        _ended = true;
        StopContinuousEffects();
        ShrinkGroundRing();
        Destroy(gameObject, lingerSeconds);
    }

    void ScaleVisualsToRadius(float radius)
    {
        if (areaRoot != null) areaRoot.localScale = Vector3.one * radius;

        foreach (var emitter in radiusScaledEmitters)
        {
            if (emitter == null) continue;
            var shape = emitter.shape;
            shape.scale = Vector3.one * radius;
        }
    }

    void StopContinuousEffects()
    {
        foreach (var effect in continuousEffects)
            if (effect != null) effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void GrowGroundRing()
    {
        if (groundRing == null) return;

        groundRing.localScale = Vector3.zero;
        groundRing.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    void ShrinkGroundRing()
    {
        if (groundRing != null) groundRing.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetLink(gameObject);
    }
}
