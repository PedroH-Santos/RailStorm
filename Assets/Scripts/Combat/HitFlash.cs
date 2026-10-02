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
    float _flashBlend;
    Tween _tween;
    Vector3 _restScale;
    bool _sustained;
    Color _sustainedColor;
    Color _sustainedBodyTint;
    Color[] _baseColors;
    float _sustainedSpeed;
    float _sustainedTime;

    void Awake()
    {
        _life = GetComponent<LifeSystem>();
        _block = new MaterialPropertyBlock();
        _restScale = transform.localScale;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        _baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var material = renderers[i] != null ? renderers[i].sharedMaterial : null;
            _baseColors[i] = material != null && material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.white;
        }
    }

    void OnEnable() => _life.OnDamaged += Play;

    void OnDisable()
    {
        _life.OnDamaged -= Play;
        _tween?.Kill();
        transform.DOKill();
    }

    void Update()
    {
        if (!_sustained) return;

        _sustainedTime += Time.deltaTime;
        Apply();
    }

    public void SetSustainedGlow(Color color, Color bodyTint, float pulseSpeed)
    {
        _sustained = true;
        _sustainedColor = color;
        _sustainedBodyTint = bodyTint;
        _sustainedSpeed = pulseSpeed;
        _sustainedTime = 0f;
        Apply();
    }

    public void ClearSustainedGlow()
    {
        _sustained = false;
        Apply();
    }

    void Play(int damage)
    {
        _tween?.Kill();
        _flashBlend = 1f;
        Apply();
        _tween = DOTween.To(() => _flashBlend, value => { _flashBlend = value; Apply(); }, 0f, flashDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);

        transform.DOKill(true);
        transform.localScale = _restScale;
        transform.DOPunchScale(_restScale * punchScale, flashDuration * 2f, 6, 0.5f).SetLink(gameObject);
    }

    float SustainedStrength()
    {
        if (!_sustained) return 0f;

        float wave = 0.5f + 0.5f * Mathf.Sin(_sustainedTime * _sustainedSpeed);
        return Mathf.Lerp(sustainedPulseFloor, 1f, wave);
    }

    void Apply()
    {
        bool idle = !_sustained && _flashBlend <= 0f;
        float strength = SustainedStrength();
        Color emission = flashColor * _flashBlend + _sustainedColor * strength;

        for (int i = 0; i < renderers.Length; i++)
        {
            var target = renderers[i];
            if (target == null) continue;

            if (idle)
            {
                target.SetPropertyBlock(null);
                continue;
            }

            target.GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, emission);
            _block.SetColor(BaseColorId, Color.Lerp(_baseColors[i], _baseColors[i] * _sustainedBodyTint, strength));
            target.SetPropertyBlock(_block);
        }
    }
}
