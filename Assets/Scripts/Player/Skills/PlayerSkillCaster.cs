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
    }

    void Update()
    {
        if (Time.timeScale <= 0f || weaponController == null) return;

        for (int slot = 0; slot < _handler.UnlockedSlots && slot < bindings.Length; slot++)
        {
            if (_castingSlot[slot] || !WasPressed(bindings[slot]) || !_handler.IsReady(slot)) continue;
            StartCoroutine(CastAfterWindup(slot));
        }
    }

    IEnumerator CastAfterWindup(int slot)
    {
        _castingSlot[slot] = true;

        if (weaponController.Animation != null) weaponController.Animation.PlayAttackAnimation();

        var skill = _handler.GetSlot(slot);
        float delay = skill != null ? skill.castDelay : 0f;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _castingSlot[slot] = false;

        var burn = _handler.Weapon != null ? _handler.Weapon.burn : null;
        var context = new SkillCastContext(weaponController.FirePoint, weaponController.AimDirection, weaponController.Owner, burn);
        _handler.TryCast(slot, context);
    }

    static bool WasPressed(SlotBinding binding)
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && binding.key != Key.None && keyboard[binding.key].wasPressedThisFrame) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[binding.gamepadButton].wasPressedThisFrame;
    }

    public string GetKeyLabel(int slot)
    {
        if (slot < 0 || slot >= bindings.Length) return string.Empty;

        bool gamepad = InventoryScreenInput.Instance != null && InventoryScreenInput.Instance.UsingGamepad;
        return gamepad ? bindings[slot].gamepadLabel : bindings[slot].keyLabel;
    }
}
