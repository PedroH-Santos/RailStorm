using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    const int BaseLayer = 0;
    const int ActionLayer = 1;

    static readonly int AttackHash = Animator.StringToHash("attack");
    static readonly int InCombatHash = Animator.StringToHash("inCombat");
    static readonly int CelebrateHash = Animator.StringToHash("celebrate");
    static readonly int QuickCelebrateHash = Animator.StringToHash("quickCelebrate");
    static readonly int JoltHash = Animator.StringToHash("jolt");
    static readonly int AimingHash = Animator.StringToHash("aiming");
    static readonly int AimStartHash = Animator.StringToHash("aimStart");
    static readonly int AimReleaseHash = Animator.StringToHash("aimRelease");
    static readonly int GestureHash = Animator.StringToHash("gesture");
    static readonly int GestureIndexHash = Animator.StringToHash("gestureIndex");

    static readonly int RelaxedIdleState = Animator.StringToHash("Idle_Relaxed");
    static readonly int CelebrateState = Animator.StringToHash("Celebrate");
    static readonly int QuickCelebrateState = Animator.StringToHash("Celebrate_Quick");

    Animator _animator;

    public bool InCombat { get; private set; }

    public bool IsInIdlePose
    {
        get
        {
            if (_animator == null || _animator.IsInTransition(BaseLayer) || _animator.IsInTransition(ActionLayer)) return false;
            return CurrentState(BaseLayer).IsTag("Idle") && CurrentState(ActionLayer).IsTag("Empty");
        }
    }

    public bool IsInRelaxedIdle => !InCombat && IsInIdlePose && CurrentState(BaseLayer).shortNameHash == RelaxedIdleState;

    public bool CanStartQuickCelebrate
        => CurrentState(BaseLayer).shortNameHash != CelebrateState && !_animator.IsInTransition(BaseLayer);

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayAttackAnimation() => _animator.SetTrigger(AttackHash);

    public void BeginAimHold()
    {
        _animator.ResetTrigger(AimReleaseHash);
        _animator.SetBool(AimingHash, true);
        _animator.SetTrigger(AimStartHash);
    }

    public void ReleaseAimHold()
    {
        _animator.ResetTrigger(AimStartHash);
        _animator.SetBool(AimingHash, false);
        _animator.SetTrigger(AimReleaseHash);
    }

    public void CancelAimHold()
    {
        _animator.ResetTrigger(AimStartHash);
        _animator.ResetTrigger(AimReleaseHash);
        _animator.SetBool(AimingHash, false);
    }

    public void EnterCombatStance()
    {
        InCombat = true;
        _animator.ResetTrigger(CelebrateHash);
        _animator.SetBool(InCombatHash, true);
    }

    public void LeaveCombatAndCelebrate()
    {
        InCombat = false;
        _animator.SetBool(InCombatHash, false);
        _animator.SetTrigger(CelebrateHash);
    }

    public void PlayQuickCelebrate() => _animator.SetTrigger(QuickCelebrateHash);

    public void PlayJolt() => _animator.SetTrigger(JoltHash);

    public void PlayGesture(int gestureIndex)
    {
        _animator.SetInteger(GestureIndexHash, gestureIndex);
        _animator.SetTrigger(GestureHash);
    }

    public bool TryGetCelebrateProgress(out float normalizedTime)
        => TryGetStateProgress(BaseLayer, CelebrateState, out normalizedTime);

    public bool TryGetQuickCelebrateProgress(out float normalizedTime)
        => TryGetStateProgress(ActionLayer, QuickCelebrateState, out normalizedTime);

    bool TryGetStateProgress(int layer, int stateHash, out float normalizedTime)
    {
        var state = CurrentState(layer);
        normalizedTime = state.normalizedTime;
        return state.shortNameHash == stateHash;
    }

    AnimatorStateInfo CurrentState(int layer) => _animator.GetCurrentAnimatorStateInfo(layer);
}
