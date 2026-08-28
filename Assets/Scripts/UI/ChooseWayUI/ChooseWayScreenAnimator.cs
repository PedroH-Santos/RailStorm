using UnityEngine;

public class ChooseWayScreenAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform bar;
    [SerializeField] private float enterDuration = 0.28f;
    [SerializeField] private float popDuration = 0.16f;
    [SerializeField] private float popScale = 1.05f;
    [SerializeField] private float unlockPunchScale = 1.12f;
    [SerializeField] private float unlockPunchDuration = 0.26f;
    [SerializeField] private float exitDuration = 0.24f;

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

    public void PlayUnlockPunch()
    {
        if (bar == null) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PunchRoutine());
    }

    public void PlayExit(System.Action onDone)
    {
        if (bar == null)
        {
            onDone?.Invoke();
            return;
        }

        _riseDistance = bar.rect.height + 80f;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ExitRoutine(onDone));
    }

    System.Collections.IEnumerator PunchRoutine()
    {
        bar.anchoredPosition = _restPosition;

        float t = 0f;
        while (t < unlockPunchDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / unlockPunchDuration);
            float scale = 1f + Mathf.Sin(p * Mathf.PI) * (unlockPunchScale - 1f);
            bar.localScale = Vector3.one * scale;
            yield return null;
        }

        bar.localScale = Vector3.one;
        _routine = null;
    }

    System.Collections.IEnumerator ExitRoutine(System.Action onDone)
    {
        bar.localScale = Vector3.one;
        Vector2 from = bar.anchoredPosition;
        Vector2 to = _restPosition - new Vector2(0f, _riseDistance);

        float t = 0f;
        while (t < exitDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / exitDuration);
            bar.anchoredPosition = Vector2.Lerp(from, to, Easing.CubicIn(p));
            yield return null;
        }

        bar.anchoredPosition = _restPosition;
        _routine = null;
        onDone?.Invoke();
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
            float eased = Easing.BackOut(p);
            bar.anchoredPosition = Vector2.Lerp(_restPosition - new Vector2(0f, _riseDistance), _restPosition, eased);
            yield return null;
        }

        bar.anchoredPosition = _restPosition;
        _routine = null;
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
        _routine = null;
    }
}
