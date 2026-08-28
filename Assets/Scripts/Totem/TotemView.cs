using System.Collections;
using UnityEngine;

public class TotemView : MonoBehaviour
{
    [Header("Particulas")]
    [SerializeField] private ParticleSystem idleParticles;
    [SerializeField] private ParticleSystem selectParticles;
    [SerializeField] private ParticleSystem unlockBurstParticles;

    [Header("Emissao do totem")]
    [Tooltip("Renderers que recebem a cor do caminho como emissao: cristais, emblema, runas.")]
    [SerializeField] private Renderer[] accentRenderers;
    [SerializeField] private float idleEmissionIntensity = 1f;
    [SerializeField] private float selectedEmissionIntensity = 2.5f;
    [SerializeField] private float unlockEmissionIntensity = 4f;

    [Header("Selo world-space")]
    [SerializeField] private ChooseWayTotemBadge badge;

    [Header("Desbloqueio")]
    [SerializeField] private float anticipationScale = 0.92f;
    [SerializeField] private float anticipationDuration = 0.12f;
    [SerializeField] private float dischargeScale = 1.1f;
    [SerializeField] private float dischargeDuration = 0.18f;
    [SerializeField] private float vanishDuration = 0.3f;
    [SerializeField] private float vanishSinkDepth = 1.5f;
    [SerializeField] private float vanishSpin = 35f;

    [Header("Poeira magica (idleParticles)")]
    [SerializeField] private float dustRateIdle = 5f;
    [SerializeField] private float dustRateSelected = 22f;
    [SerializeField] private float dustOrbitalSpeedIdle = 0.25f;
    [SerializeField] private float dustOrbitalSpeedSelected = 0.9f;
    [SerializeField] private float dustNoiseStrengthIdle = 0.12f;
    [SerializeField] private float dustNoiseStrengthSelected = 0.22f;

    const float IdleParticleWhiteMix = 0.4f;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock _mpb;

    Color _baseEmissionColor = Color.white;
    Vector3 _baseScale;
    Coroutine _routine;

    public bool IsVanishing { get; private set; }

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _baseScale = transform.localScale;

        ForceUnscaledTime(idleParticles);
        ForceUnscaledTime(selectParticles);
        ForceUnscaledTime(unlockBurstParticles);

        if (idleParticles != null) idleParticles.Play();
        ApplyDustIntensity(selected: false);

        if (badge != null) badge.Hide();
    }

    void OnEnable()
    {
        IsVanishing = false;
    }

    void ForceUnscaledTime(ParticleSystem ps)
    {
        if (ps == null) return;
        var main = ps.main;
        main.useUnscaledTime = true;
    }

    public void SetThemeColor(Color color)
    {
        _baseEmissionColor = color;
        ApplyEmission(color, idleEmissionIntensity);
        SetParticlesColor(idleParticles, Color.Lerp(color, Color.white, IdleParticleWhiteMix));
        SetParticlesColor(selectParticles, color);
        SetParticlesColor(unlockBurstParticles, color);
        if (badge != null) badge.SetAccent(color);
    }

    public void Bind(SplineEntry entry, bool affordable)
    {
        if (badge != null) badge.Bind(entry, affordable);
    }

    public void Show()
    {
        if (badge != null) badge.Show();
    }

    public void Hide()
    {
        if (badge != null) badge.Hide();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (badge != null) badge.SetSelected(selected);

        if (selected)
        {
            if (selectParticles != null) selectParticles.Play();
            ApplyEmission(_baseEmissionColor, selectedEmissionIntensity);
        }
        else
        {
            if (selectParticles != null) selectParticles.Stop();
            ApplyEmission(_baseEmissionColor, idleEmissionIntensity);
        }

        ApplyDustIntensity(selected);
    }

    void ApplyDustIntensity(bool selected)
    {
        if (idleParticles == null) return;

        var emission = idleParticles.emission;
        emission.rateOverTime = selected ? dustRateSelected : dustRateIdle;

        var velocityOverLifetime = idleParticles.velocityOverLifetime;
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(selected ? dustOrbitalSpeedSelected : dustOrbitalSpeedIdle);

        var noise = idleParticles.noise;
        noise.strength = new ParticleSystem.MinMaxCurve(selected ? dustNoiseStrengthSelected : dustNoiseStrengthIdle);
    }

    public void SetDustVisible(bool visible)
    {
        if (idleParticles == null) return;

        if (visible)
        {
            if (!idleParticles.isPlaying) idleParticles.Play();
        }
        else
        {
            idleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void PlayCharge()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        Vector3 startScale = _baseScale;
        Vector3 crouchScale = _baseScale * anticipationScale;
        Vector3 peakScale = _baseScale * dischargeScale;

        float t = 0f;
        while (t < anticipationDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / anticipationDuration);
            transform.localScale = Vector3.Lerp(startScale, crouchScale, Easing.CubicOut(p));
            yield return null;
        }

        if (unlockBurstParticles != null) unlockBurstParticles.Play();
        if (badge != null) badge.PlayUnlocked();

        t = 0f;
        while (t < dischargeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dischargeDuration);
            transform.localScale = Vector3.LerpUnclamped(crouchScale, peakScale, Easing.BackOut(p));
            ApplyEmission(_baseEmissionColor, Mathf.Lerp(selectedEmissionIntensity, unlockEmissionIntensity, p));
            yield return null;
        }

        transform.localScale = peakScale;
        ApplyEmission(_baseEmissionColor, unlockEmissionIntensity);
        _routine = null;
    }

    public void PlayVanish()
    {
        if (IsVanishing) return;

        IsVanishing = true;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(VanishRoutine());
    }

    IEnumerator VanishRoutine()
    {
        if (badge != null) badge.PlayDismiss(vanishDuration);

        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.localPosition;
        Vector3 startEuler = transform.localEulerAngles;

        float t = 0f;
        while (t < vanishDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / vanishDuration);
            float eased = Easing.CubicIn(p);

            transform.localScale = new Vector3(
                Mathf.Lerp(startScale.x, startScale.x * 0.85f, eased),
                Mathf.Lerp(startScale.y, 0f, eased),
                Mathf.Lerp(startScale.z, startScale.z * 0.85f, eased));

            transform.localPosition = startPosition - Vector3.up * vanishSinkDepth * eased;
            transform.localEulerAngles = startEuler + new Vector3(0f, 0f, vanishSpin * eased);

            ApplyEmission(_baseEmissionColor, Mathf.Lerp(unlockEmissionIntensity, 0f, eased));
            yield return null;
        }

        SetDustVisible(false);
        if (badge != null) badge.Hide();

        transform.localScale = _baseScale;
        transform.localPosition = startPosition;
        transform.localEulerAngles = startEuler;
        ApplyEmission(_baseEmissionColor, idleEmissionIntensity);

        _routine = null;
        gameObject.SetActive(false);
    }

    public void PlayDeniedEffect()
    {
        if (IsVanishing) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        Vector3 basePos = transform.localPosition;
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            float offset = Mathf.Sin(t * 60f) * 0.05f;
            transform.localPosition = basePos + new Vector3(offset, 0f, 0f);
            yield return null;
        }
        transform.localPosition = basePos;
        _routine = null;
    }

    void ApplyEmission(Color color, float intensity)
    {
        if (accentRenderers == null) return;

        foreach (var renderer in accentRenderers)
            ApplyEmissionTo(renderer, color, intensity);
    }

    void ApplyEmissionTo(Renderer target, Color color, float intensity)
    {
        if (target == null) return;
        target.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionColorId, color * intensity);
        target.SetPropertyBlock(_mpb);
    }

    static void SetParticlesColor(ParticleSystem ps, Color color)
    {
        if (ps == null) return;
        var main = ps.main;
        main.startColor = color;
    }
}
