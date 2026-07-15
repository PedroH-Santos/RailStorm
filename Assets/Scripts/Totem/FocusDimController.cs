using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocusDimController : MonoBehaviour
{
    [SerializeField] private Volume focusVolume;
    [SerializeField] private float fadeDuration = 0.25f;

    ColorAdjustments _colorAdjustments;
    Vignette _vignette;
    Coroutine _routine;

    void Awake()
    {
        if (focusVolume == null || focusVolume.profile == null) return;

        focusVolume.profile.TryGet(out _colorAdjustments);
        focusVolume.profile.TryGet(out _vignette);
    }

    public void SetFocused(bool focused, Color themeColor)
    {
        if (focusVolume == null) return;

        if (_vignette != null)
            _vignette.color.value = themeColor;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Fade(focused ? 1f : 0f));
    }

    IEnumerator Fade(float target)
    {
        float start = focusVolume.weight;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            focusVolume.weight = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        focusVolume.weight = target;
    }
}