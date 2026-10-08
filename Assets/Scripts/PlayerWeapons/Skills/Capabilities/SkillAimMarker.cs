using DG.Tweening;
using UnityEngine;

public class SkillAimMarker : MonoBehaviour
{
    const float FullTurnRadians = Mathf.PI * 2f;
    const float MinFacingSqr = 0.0001f;

    [Tooltip("Objeto escalado pelo raio da área. A escala 1 deve cobrir raio 1.")]
    [SerializeField] private Transform areaVisual;
    [SerializeField] private float groundOffset = 0.05f;

    [Header("Animação")]
    [SerializeField] private float showDuration = 0.15f;
    [SerializeField] private float reachedMaxPunch = 0.15f;

    [Header("Barra de alcance")]
    [Tooltip("Arco em volta do anel que enche conforme o marcador se afasta do vagão. Usa posições em World.")]
    [SerializeField] private LineRenderer rangeArc;
    [Tooltip("Raio do arco, em múltiplos do raio da área.")]
    [SerializeField] private float rangeArcRadiusScale = 1.15f;
    [SerializeField] private int rangeArcSegments = 48;
    [Tooltip("Piscadas por segundo do arco quando o alcance máximo é atingido.")]
    [SerializeField] private float reachedMaxBlinkSpeed = 6f;
    [Range(0f, 1f)] [SerializeField] private float reachedMaxBlinkMinAlpha = 0.35f;

    float _radius = 1f;
    float _rangeProgress;
    bool _reachedMax;
    Color _arcStartColor;
    Color _arcEndColor;
    Vector3[] _arcPoints;

    Transform Visual => areaVisual != null ? areaVisual : transform;

    void Awake()
    {
        if (rangeArc == null) return;

        _arcStartColor = rangeArc.startColor;
        _arcEndColor = rangeArc.endColor;
        _arcPoints = new Vector3[rangeArcSegments + 1];
        rangeArc.useWorldSpace = true;
    }

    void Update()
    {
        BlinkArcWhileAtMax();
    }

    void LateUpdate()
    {
        DrawRangeArc();
    }

    public void Show(float radius)
    {
        _radius = Mathf.Max(0.01f, radius);
        _reachedMax = false;
        _rangeProgress = 0f;
        gameObject.SetActive(true);
        SetArcAlpha(1f);

        Visual.DOKill();
        Visual.localScale = Vector3.zero;
        Visual.DOScale(Vector3.one * _radius, showDuration).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    public void MoveTo(Vector3 groundPoint)
    {
        transform.position = groundPoint + Vector3.up * groundOffset;
    }

    public void SetRangeProgress(float progress)
    {
        _rangeProgress = Mathf.Clamp01(progress);
    }

    public void SetReachedMax(bool reachedMax)
    {
        if (reachedMax == _reachedMax) return;
        _reachedMax = reachedMax;
        if (!reachedMax)
        {
            SetArcAlpha(1f);
            return;
        }

        Visual.DOKill(true);
        Visual.localScale = Vector3.one * _radius;
        Visual.DOPunchScale(Vector3.one * (_radius * reachedMaxPunch), 0.25f, 6, 0.5f).SetLink(gameObject);
    }

    public void Hide()
    {
        Visual.DOKill();
        gameObject.SetActive(false);
    }

    void DrawRangeArc()
    {
        if (rangeArc == null) return;

        int pointCount = Mathf.CeilToInt(rangeArcSegments * _rangeProgress) + 1;
        if (pointCount < 2)
        {
            rangeArc.positionCount = 0;
            return;
        }

        float arcRadius = rangeArcRadiusScale * Visual.lossyScale.x;
        float sweepRadians = FullTurnRadians * _rangeProgress;
        ScreenUpAndRightOnGround(out Vector3 up, out Vector3 right);
        Vector3 center = Visual.position;

        for (int i = 0; i < pointCount; i++)
        {
            float angle = sweepRadians * i / (pointCount - 1);
            Vector3 direction = up * Mathf.Cos(angle) + right * Mathf.Sin(angle);
            _arcPoints[i] = center + direction * arcRadius;
        }

        rangeArc.positionCount = pointCount;
        rangeArc.SetPositions(_arcPoints);
    }

    void BlinkArcWhileAtMax()
    {
        if (!_reachedMax || rangeArc == null) return;

        float wave = (Mathf.Sin(Time.time * reachedMaxBlinkSpeed * FullTurnRadians) + 1f) * 0.5f;
        SetArcAlpha(Mathf.Lerp(reachedMaxBlinkMinAlpha, 1f, wave));
    }

    void SetArcAlpha(float alpha)
    {
        if (rangeArc == null) return;

        rangeArc.startColor = new Color(_arcStartColor.r, _arcStartColor.g, _arcStartColor.b, _arcStartColor.a * alpha);
        rangeArc.endColor = new Color(_arcEndColor.r, _arcEndColor.g, _arcEndColor.b, _arcEndColor.a * alpha);
    }

    static void ScreenUpAndRightOnGround(out Vector3 up, out Vector3 right)
    {
        var camera = Camera.main;
        up = camera != null ? camera.transform.forward : Vector3.forward;
        up.y = 0f;
        if (up.sqrMagnitude < MinFacingSqr) up = Vector3.forward;
        up.Normalize();
        right = Vector3.Cross(Vector3.up, up);
    }
}
