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

    int ActiveSlotCount => Mathf.Min(_handler.UnlockedSlots, bindings.Length);
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

        for (int slot = 0; slot < ActiveSlotCount; slot++)
            if (CanStartCast(slot)) StartCoroutine(CastRoutine(slot));
    }

    bool CanStartCast(int slot)
    {
        if (_busySlots[slot] || !bindings[slot].WasPressedThisFrame() || !_handler.IsReady(slot)) return false;

        bool needsAim = _handler.GetSlot(slot) is IHoldToAimSkill;
        return !needsAim || !_aim.IsAiming;
    }

    IEnumerator CastRoutine(int slot)
    {
        _busySlots[slot] = true;
        var skill = _handler.GetSlot(slot);

        if (skill is IHoldToAimSkill aimSkill)
            yield return AimThenCast(slot, skill, aimSkill);
        else
            yield return CastInstantly(slot, skill);

        _busySlots[slot] = false;
    }

    IEnumerator CastInstantly(int slot, SkillDefinition skill)
    {
        if (_weapon.Animation != null) _weapon.Animation.PlayAttackAnimation();

        yield return WaitForCastDelay(skill);
        _handler.TryCast(slot, BuildContext());
    }

    IEnumerator AimThenCast(int slot, SkillDefinition skill, IHoldToAimSkill aimSkill)
    {
        _aim.Begin(aimSkill, _handler.GetLevel(skill));

        while (bindings[slot].IsHeld())
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
        _handler.TryCast(slot, BuildContext(aimPoint));
    }

    bool CanKeepAiming(int slot, SkillDefinition skill) =>
        Time.timeScale > 0f && _handler.GetSlot(slot) == skill;

    static IEnumerator WaitForCastDelay(SkillDefinition skill)
    {
        if (skill != null && skill.castDelay > 0f) yield return new WaitForSeconds(skill.castDelay);
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
