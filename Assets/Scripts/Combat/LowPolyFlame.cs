using DG.Tweening;
using UnityEngine;

public class LowPolyFlame : MonoBehaviour
{
    [Header("Corpo")]
    [SerializeField] private MeshFilter body;
    [SerializeField] private Mesh[] frames;
    [Tooltip("Trocas de forma por segundo (efeito quadro a quadro).")]
    [SerializeField, Min(0.1f)] private float framesPerSecond = 10f;
    [Tooltip("Giro aleatório em Y a cada troca de forma, em graus para cada lado.")]
    [SerializeField, Min(0f)] private float yawJitterDegrees = 20f;

    [Header("Balanço")]
    [SerializeField, Min(0f)] private float wobbleSpeed = 11f;
    [Tooltip("Quanto o corpo estica na vertical (e afina nos lados) no pico do balanço.")]
    [SerializeField, Range(0f, 0.5f)] private float stretchAmount = 0.08f;
    [SerializeField, Min(0f)] private float swayDegrees = 5f;

    [Header("Faíscas")]
    [SerializeField] private ParticleSystem sparks;

    [Header("Acender e apagar")]
    [SerializeField, Min(0.01f)] private float igniteSeconds = 0.18f;
    [SerializeField, Min(0.01f)] private float extinguishSeconds = 0.15f;

    Transform _bodyTransform;
    MeshRenderer _bodyRenderer;
    Vector3 _bodyBaseScale;
    Tween _sizeTween;
    float _size;
    float _secondsUntilNextFrame;
    int _frameIndex;
    float _yaw;
    float _wobblePhase;

    public bool IsLit { get; private set; }

    void Awake()
    {
        _bodyTransform = body.transform;
        _bodyRenderer = body.GetComponent<MeshRenderer>();
        _bodyBaseScale = _bodyTransform.localScale;
        _wobblePhase = Random.value * 100f;
        _bodyRenderer.enabled = false;
        if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDisable()
    {
        _sizeTween?.Kill();
        _size = 0f;
        IsLit = false;
        if (_bodyRenderer != null) _bodyRenderer.enabled = false;
        if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Ignite()
    {
        if (IsLit) return;
        IsLit = true;

        if (sparks != null) sparks.Play(true);
        AnimateSizeTo(1f, igniteSeconds, Ease.OutBack);
    }

    public void Extinguish()
    {
        if (!IsLit) return;
        IsLit = false;

        if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        AnimateSizeTo(0f, extinguishSeconds, Ease.InBack);
    }

    void AnimateSizeTo(float target, float seconds, Ease ease)
    {
        _sizeTween?.Kill();
        _sizeTween = DOTween.To(() => _size, size => _size = size, target, seconds)
            .SetEase(ease)
            .SetLink(gameObject);
    }

    void Update()
    {
        bool visible = _size > 0.001f;
        _bodyRenderer.enabled = visible;
        if (!visible) return;

        SwapFrameWhenDue();
        Wobble();
    }

    void SwapFrameWhenDue()
    {
        _secondsUntilNextFrame -= Time.deltaTime;
        if (_secondsUntilNextFrame > 0f) return;

        _secondsUntilNextFrame += 1f / framesPerSecond;
        if (_secondsUntilNextFrame < 0f) _secondsUntilNextFrame = 1f / framesPerSecond;

        _frameIndex = NextFrameIndex();
        if (frames.Length > 0) body.sharedMesh = frames[_frameIndex];
        _yaw = Random.Range(-yawJitterDegrees, yawJitterDegrees);
    }

    int NextFrameIndex()
    {
        if (frames.Length < 2) return 0;

        int next = Random.Range(0, frames.Length - 1);
        return next >= _frameIndex ? next + 1 : next;
    }

    void Wobble()
    {
        float time = Time.time * wobbleSpeed + _wobblePhase;
        float stretch = stretchAmount * Mathf.Sin(time);
        Vector3 wobbleScale = new Vector3(1f - stretch * 0.5f, 1f + stretch, 1f - stretch * 0.5f);

        _bodyTransform.localScale = Vector3.Scale(_bodyBaseScale, wobbleScale) * _size;
        _bodyTransform.localRotation = Quaternion.Euler(
            swayDegrees * Mathf.Sin(time * 0.7f),
            _yaw,
            swayDegrees * Mathf.Cos(time * 0.9f));
    }
}
