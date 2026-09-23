using System;
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

    [Header("Animação")]
    [SerializeField] private float animationInterval = 0.5f;

    PlayerSkillHandler _handler;
    float _animationTimer;

    public static PlayerSkillCaster Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _handler = GetComponent<PlayerSkillHandler>();
        if (weaponController == null) weaponController = GetComponent<PlayerWeaponController>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        _animationTimer += Time.deltaTime;

        if (Time.timeScale <= 0f || weaponController == null) return;

        for (int slot = 0; slot < _handler.UnlockedSlots && slot < bindings.Length; slot++)
        {
            if (!IsHeld(bindings[slot]) || !_handler.IsReady(slot)) continue;

            var context = new SkillCastContext(weaponController.FirePoint, weaponController.AimDirection, weaponController.Owner);
            if (_handler.TryCast(slot, context)) PlayAttackAnimation();
        }
    }

    static bool IsHeld(SlotBinding binding)
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && binding.key != Key.None && keyboard[binding.key].isPressed) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[binding.gamepadButton].isPressed;
    }

    void PlayAttackAnimation()
    {
        if (_animationTimer < animationInterval) return;

        _animationTimer = 0f;
        if (weaponController.Animation != null) weaponController.Animation.PlayAttackAnimation();
    }

    public string GetKeyLabel(int slot)
    {
        if (slot < 0 || slot >= bindings.Length) return string.Empty;

        bool gamepad = InventoryScreenInput.Instance != null && InventoryScreenInput.Instance.UsingGamepad;
        return gamepad ? bindings[slot].gamepadLabel : bindings[slot].keyLabel;
    }
}
