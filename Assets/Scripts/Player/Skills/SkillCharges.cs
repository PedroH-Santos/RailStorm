using UnityEngine;

public class SkillCharges
{
    public int Total { get; private set; }
    public int Remaining { get; private set; }

    float _secondsLeftToUseTheRest;
    bool _hasTimeLimit;
    bool _windowHeldOpen;

    public bool HasChargesLeft => Remaining > 0;

    public void StartBurst(int total)
    {
        Total = Mathf.Max(1, total);
        Remaining = Total;
    }

    public void HoldWindowOpen() => _windowHeldOpen = true;

    public void LetWindowRun() => _windowHeldOpen = false;

    public void SpendOne(float secondsToUseTheRest)
    {
        Remaining = Mathf.Max(0, Remaining - 1);
        _secondsLeftToUseTheRest = secondsToUseTheRest;
        _hasTimeLimit = secondsToUseTheRest > 0f;

        if (!HasChargesLeft) Clear();
    }

    public bool RanOutOfTime(float deltaTime)
    {
        if (!HasChargesLeft || !_hasTimeLimit || _windowHeldOpen) return false;

        _secondsLeftToUseTheRest -= deltaTime;
        if (_secondsLeftToUseTheRest > 0f) return false;

        Clear();
        return true;
    }

    public void Clear()
    {
        Total = 0;
        Remaining = 0;
        _secondsLeftToUseTheRest = 0f;
        _hasTimeLimit = false;
    }
}
