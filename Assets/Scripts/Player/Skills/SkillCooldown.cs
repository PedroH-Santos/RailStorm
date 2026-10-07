using UnityEngine;

public class SkillCooldown
{
    public float Remaining { get; private set; }
    public float Duration { get; private set; }

    public bool IsCoolingDown => Remaining > 0f;
    public float RemainingFraction => Duration > 0f ? Mathf.Clamp01(Remaining / Duration) : 0f;

    public void Start(float seconds)
    {
        Duration = seconds;
        Remaining = seconds;
    }

    public void Tick(float deltaTime)
    {
        if (IsCoolingDown) Remaining = Mathf.Max(0f, Remaining - deltaTime);
    }

    public void Reset()
    {
        Remaining = 0f;
        Duration = 0f;
    }
}
