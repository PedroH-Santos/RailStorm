using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(PlayerSkillHandler))]
[RequireComponent(typeof(PlayerWeaponController))]
[RequireComponent(typeof(SkillHoldToAimController))]
public class PlayerSkillCaster : MonoBehaviour
{
    [SerializeField] private SkillSlotBinding[] bindings =
    {
        new SkillSlotBinding(Key.J, GamepadButton.West, "J", "X"),
        new SkillSlotBinding(Key.K, GamepadButton.RightShoulder, "K", "RB"),
        new SkillSlotBinding(Key.L, GamepadButton.RightTrigger, "L", "RT"),
    };

    PlayerSkillHandler _handler;
    PlayerWeaponController _weapon;
    SkillHoldToAimController _aim;
    bool[] _busySlots;

    public static PlayerSkillCaster Instance { get; private set; }

    int ActiveSlotCount => Mathf.Min(_handler.UnlockedSlotCount, bindings.Length);
    BurnDefinition WeaponBurn => _handler.Weapon != null ? _handler.Weapon.burn : null;

    void Awake()
    {
        Instance = this;
        _handler = GetComponent<PlayerSkillHandler>();
        _weapon = GetComponent<PlayerWeaponController>();
        _aim = GetComponent<SkillHoldToAimController>();
        _busySlots = new bool[bindings.Length];
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        Array.Clear(_busySlots, 0, _busySlots.Length);
        _aim.Cancel();
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        for (int index = 0; index < ActiveSlotCount; index++)
            if (CanStartCast(index)) StartCoroutine(CastRoutine(_handler.GetSlot(index)));
    }

    bool CanStartCast(int index)
    {
        var slot = _handler.GetSlot(index);
        if (_busySlots[index] || !bindings[index].WasPressedThisFrame() || !slot.IsReady) return false;

        bool needsAim = slot.Skill.Definition is IHoldToAimSkill;
        return !needsAim || !_aim.IsAiming;
    }

    IEnumerator CastRoutine(SkillSlot slot)
    {
        _busySlots[slot.Index] = true;
        var skill = slot.Skill;

        if (skill.Definition is IHoldToAimSkill aimSkill)
            yield return AimThenCast(slot, skill, aimSkill);
        else
            yield return CastInstantly(slot, skill);

        _busySlots[slot.Index] = false;
    }

    IEnumerator CastInstantly(SkillSlot slot, OwnedSkill skill)
    {
        if (_weapon.Animation != null) _weapon.Animation.PlayAttackAnimation();

        yield return WaitForCastDelay(skill);
        _handler.TryCast(slot.Index, BuildContext());
    }

    IEnumerator AimThenCast(SkillSlot slot, OwnedSkill skill, IHoldToAimSkill aimSkill)
    {
        _aim.Begin(aimSkill, skill.Level);

        while (bindings[slot.Index].IsHeld())
        {
            if (!CanKeepAiming(slot, skill))
            {
                _aim.Cancel();
                yield break;
            }
            yield return null;
        }

        Vector3 aimPoint = _aim.Release();
        yield return WaitForCastDelay(skill);
        _handler.TryCast(slot.Index, BuildContext(aimPoint));
    }

    static bool CanKeepAiming(SkillSlot slot, OwnedSkill skill) =>
        Time.timeScale > 0f && slot.Skill == skill;

    static IEnumerator WaitForCastDelay(OwnedSkill skill)
    {
        float delay = skill.Definition.castDelay;
        if (delay > 0f) yield return new WaitForSeconds(delay);
    }

    SkillCastContext BuildContext() =>
        new SkillCastContext(_weapon.FirePoint, _weapon.AimDirection, _weapon.Owner, WeaponBurn);

    SkillCastContext BuildContext(Vector3 aimPoint) =>
        new SkillCastContext(_weapon.FirePoint, _weapon.AimDirection, _weapon.Owner, WeaponBurn, aimPoint);

    public string GetKeyLabel(int slot)
    {
        if (slot < 0 || slot >= bindings.Length) return string.Empty;

        bool usingGamepad = InventoryScreenInput.Instance != null && InventoryScreenInput.Instance.UsingGamepad;
        return bindings[slot].Label(usingGamepad);
    }
}
