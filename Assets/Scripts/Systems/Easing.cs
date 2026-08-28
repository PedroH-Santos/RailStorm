using UnityEngine;

public static class Easing
{
    public static float BackOut(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = Mathf.Clamp01(t) - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }

    public static float CubicOut(float t)
    {
        float p = 1f - Mathf.Clamp01(t);
        return 1f - p * p * p;
    }

    public static float CubicIn(float t)
    {
        float p = Mathf.Clamp01(t);
        return p * p * p;
    }
}
