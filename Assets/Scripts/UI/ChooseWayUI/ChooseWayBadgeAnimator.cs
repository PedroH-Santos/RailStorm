using UnityEngine;

public class ChooseWayBadgeAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform plate;
    [SerializeField] private float swayAmplitude = 2.5f;
    [SerializeField] private float swayPeriod = 2.6f;
    [SerializeField] private float restingScale = 1f;
    [SerializeField] private float selectedScale = 1.14f;
    [SerializeField] private float scaleSpeed = 10f;

    float _elapsed;
    float _targetScale = 1f;
    float _currentScale = 1f;

    void OnEnable()
    {
        _elapsed = 0f;
        _currentScale = restingScale;
        _targetScale = restingScale;
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

        float sway = Mathf.Sin(_elapsed / swayPeriod * Mathf.PI * 2f) * swayAmplitude;
        plate.localRotation = Quaternion.Euler(0f, 0f, sway);
        plate.localScale = Vector3.one * _currentScale;
    }
}
