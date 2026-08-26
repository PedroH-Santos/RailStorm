using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocusDimController : MonoBehaviour
{
    static FocusDimController _instance;
    public static FocusDimController Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<FocusDimController>(FindObjectsInactive.Include);
                if (found != null)
                {
                    found.gameObject.SetActive(true);
                    _instance = found;
                }
            }

            return _instance;
        }
    }

    [SerializeField] private Volume focusVolume;
    [SerializeField] private float fadeDuration = 0.25f;

    ColorAdjustments _colorAdjustments;
    Vignette _vignette;
    Coroutine _routine;

    void Awake()
    {
        _instance = this;

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
