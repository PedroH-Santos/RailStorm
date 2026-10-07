using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[Serializable]
public class SkillSlotBinding
{
    [SerializeField] private Key key = Key.J;
    [SerializeField] private GamepadButton gamepadButton = GamepadButton.West;
    [SerializeField] private string keyLabel = "J";
    [SerializeField] private string gamepadLabel = "X";

    public SkillSlotBinding()
    {
    }

    public SkillSlotBinding(Key key, GamepadButton gamepadButton, string keyLabel, string gamepadLabel)
    {
        this.key = key;
        this.gamepadButton = gamepadButton;
        this.keyLabel = keyLabel;
        this.gamepadLabel = gamepadLabel;
    }

    public bool WasPressedThisFrame()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && key != Key.None && keyboard[key].wasPressedThisFrame) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[gamepadButton].wasPressedThisFrame;
    }

    public bool IsHeld()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && key != Key.None && keyboard[key].isPressed) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && gamepad[gamepadButton].isPressed;
    }

    public string Label(bool usingGamepad) => usingGamepad ? gamepadLabel : keyLabel;
}
