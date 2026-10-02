using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(LifeSystem))]
public class HitFlash : MonoBehaviour
{
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color flashColor = new Color(0.5f, 0.42f, 0.3f, 1f);
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float punchScale = 0.08f;

    LifeSystem _life;
    MaterialPropertyBlock _block;
    float _blend;
    Tween _tween;
    Vector3 _restScale;

    void Awake()
    {
        _life = GetComponent<LifeSystem>();
        _block = new MaterialPropertyBlock();
        _restScale = transform.localScale;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable() => _life.OnDamaged += Play;

    void OnDisable()
    {
        _life.OnDamaged -= Play;
        _tween?.Kill();
        transform.DOKill();
    }

    void Play(int damage)
    {
        _tween?.Kill();
        _blend = 1f;
        Apply();
        _tween = DOTween.To(() => _blend, value => { _blend = value; Apply(); }, 0f, flashDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);

        transform.DOKill(true);
        transform.localScale = _restScale;
        transform.DOPunchScale(_restScale * punchScale, flashDuration * 2f, 6, 0.5f).SetLink(gameObject);
    }

    void Apply()
    {
        foreach (var target in renderers)
        {
            if (target == null) continue;

            if (_blend <= 0f)
            {
                target.SetPropertyBlock(null);
                continue;
            }

            target.GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, flashColor * _blend);
            target.SetPropertyBlock(_block);
        }
    }
}
