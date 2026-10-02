using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BurnIndicator : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform icon;

    [Header("Cores")]
    [SerializeField] private Color stackingColor = new Color(1f, 0.69f, 0.125f, 1f);
    [SerializeField] private Color burningColor = new Color(0.88f, 0.32f, 0.1f, 1f);

    [Header("Animação")]
    [SerializeField] private float fillDuration = 0.18f;
    [SerializeField] private float stackPunch = 0.35f;
    [SerializeField] private float burningPulseScale = 1.18f;
    [SerializeField] private float burningPulseDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.15f;

    BurnReceiver _receiver;
    Transform _camera;
    bool _burning;

    public void Bind(BurnReceiver receiver)
    {
        _receiver = receiver;
        _receiver.OnStacksChanged += Refresh;

        if (Camera.main != null) _camera = Camera.main.transform;
        if (fill != null) fill.fillAmount = 0f;
        if (group != null) group.alpha = 0f;

        Follow();
    }

    void OnDestroy()
    {
        if (_receiver != null) _receiver.OnStacksChanged -= Refresh;
    }

    void LateUpdate()
    {
        if (_receiver == null)
        {
            Destroy(gameObject);
            return;
        }

        Follow();
    }

    void Follow()
    {
        var burn = _receiver.Burn;
        Vector3 side = _camera != null ? _camera.right : Vector3.right;
        transform.position = _receiver.transform.position + Vector3.up * burn.indicatorHeight + side * burn.indicatorSide;
        if (_camera != null) transform.rotation = _camera.rotation;
    }

    void Refresh(BurnReceiver receiver)
    {
        float target = receiver.FillNormalized;
        bool visible = target > 0f;
        bool burning = receiver.IsBurning;

        if (group != null)
        {
            group.DOKill();
            group.DOFade(visible ? 1f : 0f, fadeDuration).SetLink(gameObject);
        }

        if (fill != null)
        {
            fill.DOKill();
            fill.color = burning ? burningColor : stackingColor;
            fill.DOFillAmount(target, fillDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        if (icon == null) return;

        bool wasBurning = _burning;
        _burning = burning;
        if (burning && wasBurning) return;

        icon.DOKill();
        icon.localScale = Vector3.one;

        if (burning)
            icon.DOScale(burningPulseScale, burningPulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        else if (visible)
            icon.DOPunchScale(Vector3.one * stackPunch, 0.25f, 6, 0.5f).SetLink(gameObject);
    }
}
