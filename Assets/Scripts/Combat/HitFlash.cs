using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(LifeSystem))]
public class HitFlash : MonoBehaviour
{
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color flashColor = new Color(0.5f, 0.42f, 0.3f, 1f);
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float punchScale = 0.08f;
    [Tooltip("Menor intensidade do brilho sustentado durante o pulso (1 = sem pulso).")]
    [Range(0f, 1f)]
    [SerializeField] private float sustainedPulseFloor = 0.45f;

    LifeSystem _life;
    MaterialPropertyBlock _block;
    Color[] _originalBodyColors;
    Vector3 _restScale;

    float _hitFlashStrength;
    Tween _hitFlashFade;

    bool _glowing;
    Color _glowColor;
    Color _glowBodyTint;
    float _glowPulseSpeed;
    float _glowElapsed;

    void Awake()
    {
        _life = GetComponent<LifeSystem>();
        _block = new MaterialPropertyBlock();
        _restScale = transform.localScale;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        _originalBodyColors = ReadOriginalBodyColors();
    }

    void OnEnable() => _life.OnDamaged += FlashOnHit;

    void OnDisable()
    {
        _life.OnDamaged -= FlashOnHit;
        _hitFlashFade?.Kill();
        transform.DOKill();
    }

    void Update()
    {
        if (!_glowing) return;

        _glowElapsed += Time.deltaTime;
        PaintRenderers();
    }

    public void StartGlowing(Color glowColor, Color bodyTint, float pulseSpeed)
    {
        _glowing = true;
        _glowColor = glowColor;
        _glowBodyTint = bodyTint;
        _glowPulseSpeed = pulseSpeed;
        _glowElapsed = 0f;
        PaintRenderers();
    }

    public void StopGlowing()
    {
        _glowing = false;
        PaintRenderers();
    }

    void FlashOnHit(int damage)
    {
        FadeHitFlashFromFull();
        PunchScale();
    }

    void FadeHitFlashFromFull()
    {
        _hitFlashFade?.Kill();
        _hitFlashStrength = 1f;
        PaintRenderers();

        _hitFlashFade = DOTween.To(() => _hitFlashStrength, value => { _hitFlashStrength = value; PaintRenderers(); }, 0f, flashDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    void PunchScale()
    {
        transform.DOKill(true);
        transform.localScale = _restScale;
        transform.DOPunchScale(_restScale * punchScale, flashDuration * 2f, 6, 0.5f).SetLink(gameObject);
    }

    float CurrentGlowStrength()
    {
        if (!_glowing) return 0f;

        float wave = 0.5f + 0.5f * Mathf.Sin(_glowElapsed * _glowPulseSpeed);
        return Mathf.Lerp(sustainedPulseFloor, 1f, wave);
    }

    void PaintRenderers()
    {
        bool nothingToShow = !_glowing && _hitFlashStrength <= 0f;
        if (nothingToShow)
        {
            RestoreOriginalLook();
            return;
        }

        float glowStrength = CurrentGlowStrength();
        Color emission = flashColor * _hitFlashStrength + _glowColor * glowStrength;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Color tintedBody = Color.Lerp(_originalBodyColors[i], _originalBodyColors[i] * _glowBodyTint, glowStrength);
            renderers[i].GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, emission);
            _block.SetColor(BaseColorId, tintedBody);
            renderers[i].SetPropertyBlock(_block);
        }
    }

    void RestoreOriginalLook()
    {
        foreach (var target in renderers)
            if (target != null) target.SetPropertyBlock(null);
    }

    Color[] ReadOriginalBodyColors()
    {
        var colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var material = renderers[i] != null ? renderers[i].sharedMaterial : null;
            colors[i] = material != null && material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.white;
        }
        return colors;
    }
}
