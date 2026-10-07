using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EmberRainArea : MonoBehaviour
{
    const int MaxCollidersPerPulse = 64;

    [Header("Visual")]
    [Tooltip("Visual do chão escalado pelo raio. Escala 1 deve cobrir raio 1.")]
    [SerializeField] private Transform areaRoot;
    [Tooltip("Emissores cuja forma (shape) cresce com o raio, sem mudar altura nem tamanho das partículas. A forma deve cobrir raio 1.")]
    [SerializeField] private ParticleSystem[] radiusScaledEmitters;
    [Tooltip("Efeitos que rodam enquanto a chuva dura e param de emitir no fim.")]
    [SerializeField] private ParticleSystem[] continuousEffects;
    [SerializeField] private ParticleSystem pulseBurst;
    [SerializeField] private int pulseBurstCount = 6;
    [Tooltip("Anel do chão que some no fim da chuva.")]
    [SerializeField] private Transform groundRing;
    [SerializeField] private float lingerSeconds = 1f;

    readonly Collider[] _collidersInRadius = new Collider[MaxCollidersPerPulse];
    readonly HashSet<LifeSystem> _enemiesHitThisPulse = new();

    EmberRainStorm _storm;
    SkillHit _hitPerPulse;
    float _elapsed;
    int _pulsesDone;
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

        while (HasPulseDue()) Pulse();

        if (_elapsed >= _storm.Duration) End();
    }

    bool HasPulseDue()
    {
        int nextPulse = _pulsesDone + 1;
        return nextPulse <= _storm.TotalPulses && _elapsed >= _storm.TimeOfPulse(nextPulse);
    }

    void Pulse()
    {
        _pulsesDone++;
        if (pulseBurst != null) pulseBurst.Emit(pulseBurstCount);
        HitEnemiesInRadius();
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
