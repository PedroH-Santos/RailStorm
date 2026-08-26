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

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock _mpb;

    Color _baseEmissionColor = Color.white;
    Vector3 _baseScale;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _baseScale = transform.localScale;

        ForceUnscaledTime(idleParticles);
        ForceUnscaledTime(selectParticles);
        ForceUnscaledTime(unlockBurstParticles);

        badge?.Hide();
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
        SetParticlesColor(idleParticles, color);
        SetParticlesColor(selectParticles, color);
        SetParticlesColor(unlockBurstParticles, color);
        badge?.SetAccent(color);
    }

    public void Bind(SplineEntry entry, bool affordable)
    {
        badge?.Bind(entry, affordable);
    }

    public void Show()
    {
        badge?.Show();
    }

    public void Hide()
    {
        badge?.Hide();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        badge?.SetSelected(selected);

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
    }

    public void PlayUnlockEffect(System.Action onComplete = null)
    {
        StartCoroutine(UnlockRoutine(onComplete));
    }

    IEnumerator UnlockRoutine(System.Action onComplete)
    {
        if (unlockBurstParticles != null) unlockBurstParticles.Play();

        float t = 0f;
        float duration = 0.5f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(t / duration * Mathf.PI) * 0.15f;
            transform.localScale = _baseScale * pulse;
            ApplyEmission(_baseEmissionColor, Mathf.Lerp(selectedEmissionIntensity, unlockEmissionIntensity, t / duration));
            yield return null;
        }

        transform.localScale = _baseScale;
        onComplete?.Invoke();
    }

    public void PlayDeniedEffect()
    {
        StartCoroutine(ShakeRoutine());
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
