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
    bool _showingBurning;

    public void Track(BurnReceiver receiver)
    {
        _receiver = receiver;
        _receiver.OnStacksChanged += ShowState;

        if (Camera.main != null) _camera = Camera.main.transform;
        if (fill != null) fill.fillAmount = 0f;
        if (group != null) group.alpha = 0f;

        StayNextToEnemy();
    }

    void OnDestroy()
    {
        if (_receiver != null) _receiver.OnStacksChanged -= ShowState;
    }

    void LateUpdate()
    {
        bool enemyIsGone = _receiver == null;
        if (enemyIsGone)
        {
            Destroy(gameObject);
            return;
        }

        StayNextToEnemy();
    }

    void StayNextToEnemy()
    {
        var burn = _receiver.Burn;
        Vector3 screenRight = _camera != null ? _camera.right : Vector3.right;

        transform.position = _receiver.transform.position + Vector3.up * burn.indicatorHeight + screenRight * burn.indicatorSide;
        if (_camera != null) transform.rotation = _camera.rotation;
    }

    void ShowState(BurnReceiver receiver)
    {
        float progress = receiver.IgnitionProgress;
        bool hasAnything = progress > 0f;

        FadeTo(hasAnything ? 1f : 0f);
        FillTo(progress, receiver.IsBurning);
        AnimateIcon(receiver.IsBurning, hasAnything);
    }

    void FadeTo(float alpha)
    {
        if (group == null) return;

        group.DOKill();
        group.DOFade(alpha, fadeDuration).SetLink(gameObject);
    }

    void FillTo(float progress, bool burning)
    {
        if (fill == null) return;

        fill.DOKill();
        fill.color = burning ? burningColor : stackingColor;
        fill.DOFillAmount(progress, fillDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    void AnimateIcon(bool burning, bool hasStacks)
    {
        if (icon == null) return;

        bool alreadyPulsingForThisBurn = burning && _showingBurning;
        _showingBurning = burning;
        if (alreadyPulsingForThisBurn) return;

        icon.DOKill();
        icon.localScale = Vector3.one;

        if (burning) PulseWhileBurning();
        else if (hasStacks) PunchForNewStack();
    }

    void PulseWhileBurning()
    {
        icon.DOScale(burningPulseScale, burningPulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
    }

    void PunchForNewStack()
    {
        icon.DOPunchScale(Vector3.one * stackPunch, 0.25f, 6, 0.5f).SetLink(gameObject);
    }
}
