using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(PlayerSkillHandler))]
[RequireComponent(typeof(PlayerAim))]
[RequireComponent(typeof(SkillHoldToAimController))]
public class PlayerSkillCaster : MonoBehaviour
{
    [SerializeField] private SkillSlotBinding[] bindings =
    {
        new SkillSlotBinding(Key.J, GamepadButton.West, "J", "X"),
        new SkillSlotBinding(Key.K, GamepadButton.RightShoulder, "K", "RB"),
        new SkillSlotBinding(Key.L, GamepadButton.RightTrigger, "L", "RT"),
    };

    PlayerSkillHandler _skills;
    PlayerAim _playerAim;
    SkillHoldToAimController _holdToAim;
    PlayerAnimationController _animation;
    bool[] _slotIsCasting;
    SkillCharges _chargesWaitingForAim;

    public static PlayerSkillCaster Instance { get; private set; }

    int UsableSlotCount => Mathf.Min(_skills.UnlockedSlotCount, bindings.Length);
    BurnDefinition WeaponBurn => _skills.Weapon != null ? _skills.Weapon.burn : null;

    void Awake()
    {
        Instance = this;
        _skills = GetComponent<PlayerSkillHandler>();
        _playerAim = GetComponent<PlayerAim>();
        _holdToAim = GetComponent<SkillHoldToAimController>();
        _animation = GetComponentInChildren<PlayerAnimationController>();
        _slotIsCasting = new bool[bindings.Length];
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        Array.Clear(_slotIsCasting, 0, _slotIsCasting.Length);
        _holdToAim.Cancel();
        LetChargeWindowRun();
    }

    void Update()
    {
        bool gameIsPaused = Time.timeScale <= 0f;
        if (gameIsPaused) return;

        for (int index = 0; index < UsableSlotCount; index++)
            if (PlayerStartedCastingFrom(index)) StartCoroutine(CastRoutine(_skills.GetSlot(index)));
    }

    bool PlayerStartedCastingFrom(int index)
    {
        var slot = _skills.GetSlot(index);
        if (_slotIsCasting[index] || !bindings[index].WasPressedThisFrame() || !slot.IsReady) return false;

        bool needsGroundAim = slot.Skill.Definition is IHoldToAimSkill;
        return !needsGroundAim || !_holdToAim.IsAiming;
    }

    IEnumerator CastRoutine(SkillSlot slot)
    {
        _slotIsCasting[slot.Index] = true;
        var skill = slot.Skill;

        if (skill.Definition is IHoldToAimSkill holdToAimSkill)
            yield return AimThenCast(slot, skill, holdToAimSkill);
        else
            yield return CastInstantly(slot, skill);

        _slotIsCasting[slot.Index] = false;
    }

    IEnumerator CastInstantly(SkillSlot slot, OwnedSkill skill)
    {
        if (_animation != null) _animation.PlayAttackAnimation();

        yield return WaitForAttackSwing(skill);
        _skills.TryCast(slot.Index, SkillCastContext.TowardsAim(_playerAim.FirePoint, _playerAim.FacingDirection, WeaponBurn));
    }

    IEnumerator AimThenCast(SkillSlot slot, OwnedSkill skill, IHoldToAimSkill holdToAimSkill)
    {
        _holdToAim.Begin(holdToAimSkill, skill.Level);
        KeepChargeWindowOpenWhileAiming(slot.Charges);

        while (bindings[slot.Index].IsHeld())
        {
            if (!CanKeepAiming(slot, skill))
            {
                _holdToAim.Cancel();
                LetChargeWindowRun();
                yield break;
            }
            yield return null;
        }

        Vector3 groundPoint = _holdToAim.Release();
        yield return WaitForAttackSwing(skill);
        _skills.TryCast(slot.Index, SkillCastContext.AtGroundPoint(_playerAim.FirePoint, _playerAim.FacingDirection, WeaponBurn, groundPoint));
        LetChargeWindowRun();
    }

    void KeepChargeWindowOpenWhileAiming(SkillCharges charges)
    {
        _chargesWaitingForAim = charges;
        _chargesWaitingForAim.HoldWindowOpen();
    }

    void LetChargeWindowRun()
    {
        if (_chargesWaitingForAim == null) return;

        _chargesWaitingForAim.LetWindowRun();
        _chargesWaitingForAim = null;
    }

    static bool CanKeepAiming(SkillSlot slot, OwnedSkill skill)
    {
        bool gameIsRunning = Time.timeScale > 0f;
        bool skillIsStillInThisSlot = slot.Skill == skill;
        return gameIsRunning && skillIsStillInThisSlot;
    }

    static IEnumerator WaitForAttackSwing(OwnedSkill skill)
    {
        float swingSeconds = skill.Definition.castDelay;
        if (swingSeconds > 0f) yield return new WaitForSeconds(swingSeconds);
    }

    public string GetKeyLabel(int slot)
    {
        if (slot < 0 || slot >= bindings.Length) return string.Empty;

        bool usingGamepad = InventoryScreenInput.Instance != null && InventoryScreenInput.Instance.UsingGamepad;
        return bindings[slot].Label(usingGamepad);
    }
}
