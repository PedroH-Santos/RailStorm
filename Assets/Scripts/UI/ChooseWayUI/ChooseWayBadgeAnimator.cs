using UnityEngine;

public class ChooseWayBadgeAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform plate;
    [SerializeField] private float swayAmplitude = 2.5f;
    [SerializeField] private float swayPeriod = 2.6f;
    [SerializeField] private float restingScale = 1f;
    [SerializeField] private float selectedScale = 1.14f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("Punch de desbloqueio")]
    [SerializeField] private float punchScale = 0.22f;
    [SerializeField] private float punchDuration = 0.28f;

    float _elapsed;
    float _targetScale = 1f;
    float _currentScale = 1f;
    float _punchRemaining;

    void OnEnable()
    {
        _elapsed = 0f;
        _currentScale = restingScale;
        _targetScale = restingScale;
        _punchRemaining = 0f;
    }

    public void PlayPunch()
    {
        _punchRemaining = punchDuration;
    }

    public void SetSelected(bool selected)
    {
        _targetScale = selected ? selectedScale : restingScale;
    }

    void LateUpdate()
    {
        if (plate == null) return;

        _elapsed += Time.unscaledDeltaTime;
        _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.unscaledDeltaTime * scaleSpeed);

        float punch = 0f;
        if (_punchRemaining > 0f)
        {
            _punchRemaining -= Time.unscaledDeltaTime;
            float p = 1f - Mathf.Clamp01(_punchRemaining / punchDuration);
            punch = Mathf.Sin(p * Mathf.PI) * punchScale;
        }

        float sway = Mathf.Sin(_elapsed / swayPeriod * Mathf.PI * 2f) * swayAmplitude;
        plate.localRotation = Quaternion.Euler(0f, 0f, sway);
        plate.localScale = Vector3.one * (_currentScale + punch);
    }
}
