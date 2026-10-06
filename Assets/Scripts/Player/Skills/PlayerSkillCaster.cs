using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(PlayerSkillHandler))]
public class PlayerSkillCaster : MonoBehaviour
{
    [Serializable]
    public class SlotBinding
    {
        public Key key = Key.J;
        public GamepadButton gamepadButton = GamepadButton.West;
        public string keyLabel = "J";
        public string gamepadLabel = "X";
    }

    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private SkillHoldToAimController holdToAim;

    [SerializeField] private SlotBinding[] bindings =
    {
        new SlotBinding { key = Key.J, gamepadButton = GamepadButton.West, keyLabel = "J", gamepadLabel = "X" },
        new SlotBinding { key = Key.K, gamepadButton = GamepadButton.RightShoulder, keyLabel = "K", gamepadLabel = "RB" },
        new SlotBinding { key = Key.L, gamepadButton = GamepadButton.RightTrigger, keyLabel = "L", gamepadLabel = "RT" },
    };

    PlayerSkillHandler _handler;
    bool[] _castingSlot;

    public static PlayerSkillCaster Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _handler = GetComponent<PlayerSkillHandler>();
        if (weaponController == null) weaponController = GetComponent<PlayerWeaponController>();
        if (holdToAim == null) holdToAim = GetComponent<SkillHoldToAimController>();
        _castingSlot = new bool[bindings.Length];
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        Array.Clear(_castingSlot, 0, _castingSlot.Length);
        if (holdToAim != null) holdToAim.HideAllMarkers();
    }

    void Update()
    {
        if (Time.timeScale <= 0f || weaponController == null) return;

        for (int slot = 0; slot < _handler.UnlockedSlots && slot < bindings.Length; slot++)
        {
            if (_castingSlot[slot] || !WasPressed(bindings[slot]) || !_handler.IsReady(slot)) continue;

            if (_handler.GetSlot(slot) is IHoldToAimSkill holdToAimSkill && holdToAim != null)
                StartCoroutine(AimThenCast(slot, holdToAimSkill));
            else
                StartCoroutine(CastAfterWindup(slot));
        }
    }

    IEnumerator AimThenCast(int slot, IHoldToAimSkill holdToAimSkill)
    {
        _castingSlot[slot] = true;

        var skill = _handler.GetSlot(slot);
        bool released = false;
        Vector3 aimPoint = Vector3.zero;

        yield return holdToAim.HoldToAim(
            holdToAimSkill,
            _handler.GetLevel(skill),
            weaponController,
            () => IsHeld(bindings[slot]),
            () => Time.timeScale > 0f && _handler.GetSlot(slot) == skill,
            point => { released = true; aimPoint = point; });

        if (!released)
        {
            _castingSlot[slot] = false;
            yield break;
        }

        yield return CastAfterWindup(slot, aimPoint);
    }

    IEnumerator CastAfterWindup(int slot) => CastAfterWindup(slot, null);

    IEnumerator CastAfterWindup(int slot, Vector3? aimPoint)
    {
        _castingSlot[slot] = true;

        if (weaponController.Animation != null) weaponController.Animation.PlayAttackAnimation();

        var skill = _handler.GetSlot(slot);
        float delay = skill != null ? skill.castDelay : 0f;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _castingSlot[slot] = false;

        var burn = _handler.Weapon != null ? _handler.Weapon.burn : null;
        var context = aimPoint.HasValue
            ? new SkillCastContext(weaponController.FirePoint, weaponController.AimDirection, weaponController.Owner, burn, aimPoint.Value)
            : new SkillCastContext(weaponController.FirePoint, weaponController.AimDirection, weaponController.Owner, burn);
        _handler.TryCast(slot, context);
    }

    static bool WasPressed(SlotBinding binding)
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && binding.key != Key.None && keyboard[binding.key].wasPressedThisFrame) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[binding.gamepadButton].wasPressedThisFrame;
    }

    static bool IsHeld(SlotBinding binding)
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && binding.key != Key.None && keyboard[binding.key].isPressed) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[binding.gamepadButton].isPressed;
    }

    public string GetKeyLabel(int slot)
    {
        if (slot < 0 || slot >= bindings.Length) return string.Empty;

        bool gamepad = InventoryScreenInput.Instance != null && InventoryScreenInput.Instance.UsingGamepad;
        return gamepad ? bindings[slot].gamepadLabel : bindings[slot].keyLabel;
    }
}
