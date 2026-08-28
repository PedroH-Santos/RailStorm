using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class SplineUnlockSequence : MonoBehaviour
{
    [Header("Ritmo")]
    [SerializeField] private float chargeDuration = 0.3f;
    [SerializeField] private float payDelay = 0.08f;
    [SerializeField] private float trackRevealDuration = 0.7f;
    [SerializeField] private float vanishHold = 0.3f;
    [SerializeField] private float tailDelay = 0.1f;

    Coroutine _routine;

    public bool IsPlaying => _routine != null;

    public void Play(SplineEntry entry, TotemView view, bool reversed, Action onCoinsSpent, Action onComplete)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(SequenceRoutine(entry, view, reversed, onCoinsSpent, onComplete));
    }

    IEnumerator SequenceRoutine(SplineEntry entry, TotemView view, bool reversed, Action onCoinsSpent, Action onComplete)
    {
        view?.PlayCharge();

        yield return WaitUnscaled(chargeDuration);

        onCoinsSpent?.Invoke();

        yield return WaitUnscaled(payDelay);

        if (SplinePathVisual.TryGet(entry.index, out var pathVisual))
            pathVisual.PlayUnlockReveal(reversed, trackRevealDuration);

        SplinePathParticles.Instance?.PlayReveal(trackRevealDuration);

        yield return WaitUnscaled(trackRevealDuration);

        view?.PlayVanish();

        yield return WaitUnscaled(vanishHold + tailDelay);

        _routine = null;
        onComplete?.Invoke();
    }

    static IEnumerator WaitUnscaled(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
