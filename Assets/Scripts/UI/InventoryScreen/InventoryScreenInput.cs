using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryScreenInput : MonoBehaviour
{
    public static InventoryScreenInput Instance { get; private set; }

    [SerializeField] private InventoryScreenUI screen;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerStatsAggregator stats;

    public bool UsingGamepad { get; private set; }

    void Awake()
    {
        Instance = this;
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (stats == null && player != null) stats = player.GetComponent<PlayerStatsAggregator>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        bool keyPressed = keyboard != null && keyboard.iKey.wasPressedThisFrame;
        bool padPressed = gamepad != null && gamepad.buttonNorth.wasPressedThisFrame;

        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) UsingGamepad = false;
        if (gamepad != null && gamepad.wasUpdatedThisFrame && GamepadHasInput(gamepad)) UsingGamepad = true;

        if (keyPressed || padPressed) TryOpen();
    }

    static bool GamepadHasInput(Gamepad gamepad)
    {
        foreach (var control in gamepad.allControls)
            if (control is UnityEngine.InputSystem.Controls.ButtonControl button && button.wasPressedThisFrame)
                return true;

        return gamepad.leftStick.ReadValue().sqrMagnitude > 0.25f;
    }

    public bool CanOpen => screen != null && !screen.IsOpen && Time.timeScale > 0f;

    public void TryOpen()
    {
        if (!CanOpen) return;

        if (player != null) player.SetMovementLocked(true);
        screen.Open(stats, Close);
    }

    void Close()
    {
        if (screen == null || !screen.IsOpen) return;

        if (player != null) player.SetMovementLocked(false);
        screen.Close();
    }
}
