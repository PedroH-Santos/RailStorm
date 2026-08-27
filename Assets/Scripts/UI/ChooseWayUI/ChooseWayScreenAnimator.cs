using UnityEngine;

public class ChooseWayScreenAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform bar;
    [SerializeField] private float enterDuration = 0.28f;
    [SerializeField] private float popDuration = 0.16f;
    [SerializeField] private float popScale = 1.05f;

    Vector2 _restPosition;
    float _riseDistance;
    Coroutine _routine;

    void Awake()
    {
        if (bar != null) _restPosition = bar.anchoredPosition;
    }

    public void PlayEnter()
    {
        if (bar == null) return;

        _riseDistance = bar.rect.height + 80f;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(EnterRoutine());
    }

    public void PlayPop()
    {
        if (bar == null) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PopRoutine());
    }

    System.Collections.IEnumerator EnterRoutine()
    {
        bar.anchoredPosition = _restPosition - new Vector2(0f, _riseDistance);
        bar.localScale = Vector3.one;

        float t = 0f;
        while (t < enterDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / enterDuration);
            float eased = BackOut(p);
            bar.anchoredPosition = Vector2.Lerp(_restPosition - new Vector2(0f, _riseDistance), _restPosition, eased);
            yield return null;
        }

        bar.anchoredPosition = _restPosition;
    }

    System.Collections.IEnumerator PopRoutine()
    {
        bar.anchoredPosition = _restPosition;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popDuration);
            float scale = p < 0.5f
                ? Mathf.Lerp(1f, popScale, p / 0.5f)
                : Mathf.Lerp(popScale, 1f, (p - 0.5f) / 0.5f);
            bar.localScale = Vector3.one * scale;
            yield return null;
        }

        bar.localScale = Vector3.one;
    }

    static float BackOut(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}
