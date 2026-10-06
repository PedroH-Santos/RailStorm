using UnityEngine;

public class SkillChargeState
{
    public int Remaining { get; private set; }
    public int Total { get; private set; }
    public float WindowRemaining { get; private set; }

    public bool HasPendingCharges => Remaining > 0;

    public void Begin(int total, float windowSeconds)
    {
        Total = Mathf.Max(1, total);
        Remaining = Total;
        WindowRemaining = windowSeconds;
    }

    public void ConsumeOne()
    {
        Remaining = Mathf.Max(0, Remaining - 1);
        if (Remaining == 0) Clear();
    }

    public void RestartWindow(float windowSeconds) => WindowRemaining = windowSeconds;

    public bool TickWindow(float deltaTime)
    {
        if (!HasPendingCharges || WindowRemaining <= 0f) return false;

        WindowRemaining -= deltaTime;
        if (WindowRemaining > 0f) return false;

        Clear();
        return true;
    }

    public void Clear()
    {
        Remaining = 0;
        Total = 0;
        WindowRemaining = 0f;
    }
}
