using UnityEngine;

public readonly struct EmberRainStorm
{
    const float MinPulseInterval = 0.05f;
    const float RoundingTolerance = 0.0001f;

    public readonly float Radius;
    public readonly float Duration;
    public readonly float PulseInterval;

    public EmberRainStorm(float radius, float duration, float pulseInterval)
    {
        Radius = radius;
        Duration = duration;
        PulseInterval = Mathf.Max(MinPulseInterval, pulseInterval);
    }

    public int TotalPulses => Mathf.FloorToInt(Duration / PulseInterval + RoundingTolerance);

    public float TimeOfPulse(int pulseNumber) => pulseNumber * PulseInterval;
}
