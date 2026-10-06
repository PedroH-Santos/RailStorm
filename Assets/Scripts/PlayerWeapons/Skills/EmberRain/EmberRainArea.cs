using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EmberRainArea : MonoBehaviour
{
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

    readonly HashSet<LifeSystem> _hitThisPulse = new();
    readonly Collider[] _overlapBuffer = new Collider[64];

    int _damagePerPulse;
    float _radius;
    float _duration;
    float _pulseInterval;
    BurnDefinition _burn;
    int _burnStacksPerPulse;
    float _elapsed;
    float _nextPulseAt;
    bool _finished;

    public void Init(int damagePerPulse, float radius, float duration, float pulseInterval, BurnDefinition burn, int burnStacksPerPulse)
    {
        _damagePerPulse = damagePerPulse;
        _radius = radius;
        _duration = duration;
        _pulseInterval = Mathf.Max(0.05f, pulseInterval);
        _burn = burn;
        _burnStacksPerPulse = burnStacksPerPulse;
        _nextPulseAt = _pulseInterval;

        if (areaRoot != null) areaRoot.localScale = Vector3.one * radius;

        foreach (var emitter in radiusScaledEmitters)
        {
            if (emitter == null) continue;
            var shape = emitter.shape;
            shape.scale = Vector3.one * radius;
        }

        if (groundRing != null)
        {
            groundRing.localScale = Vector3.zero;
            groundRing.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
        }
    }

    void Update()
    {
        if (_finished) return;

        _elapsed += Time.deltaTime;

        while (_elapsed >= _nextPulseAt && _nextPulseAt <= _duration + 0.0001f)
        {
            Pulse();
            _nextPulseAt += _pulseInterval;
        }

        if (_elapsed >= _duration) Finish();
    }

    void Pulse()
    {
        if (pulseBurst != null) pulseBurst.Emit(pulseBurstCount);

        _hitThisPulse.Clear();
        int count = Physics.OverlapSphereNonAlloc(transform.position, _radius, _overlapBuffer, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            var collider = _overlapBuffer[i];
            if (collider == null) continue;

            var life = collider.GetComponentInParent<LifeSystem>();
            if (life == null || life.IsDead || !life.CompareTag("Enemy")) continue;
            if (!_hitThisPulse.Add(life)) continue;

            life.Damage(_damagePerPulse);
            if (!life.IsDead) BurnReceiver.ApplyStack(life.gameObject, _burn, _burnStacksPerPulse);
        }
    }

    void Finish()
    {
        _finished = true;

        foreach (var effect in continuousEffects)
            if (effect != null) effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (groundRing != null)
            groundRing.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetLink(gameObject);

        Destroy(gameObject, lingerSeconds);
    }
}
