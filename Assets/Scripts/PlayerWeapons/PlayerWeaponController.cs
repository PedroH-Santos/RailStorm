using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Aim")]
    public Transform player;
    public LayerMask groundMask;

    [Header("Disparo")]
    public Transform firePoint;

    PlayerAnimationController _animation;

    public Transform FirePoint => firePoint;
    public Transform Owner => player != null ? player : transform;
    public Vector3 AimDirection => Owner.forward;
    public PlayerAnimationController Animation => _animation;

    void Start()
    {
        if (player != null) _animation = player.GetComponent<PlayerAnimationController>();
    }

    void Update() => AimAtMouse();

    void AimAtMouse()
    {
        if (player == null) return;

        Vector3 aimDirection = Vector3.zero;

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.1f)
                aimDirection = new Vector3(stick.x, 0f, stick.y);
        }

        if (aimDirection == Vector3.zero && Mouse.current != null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                Vector3 lookPoint = hit.point;
                lookPoint.y = player.position.y;
                aimDirection = lookPoint - player.position;
            }
        }

        if (aimDirection.sqrMagnitude < 0.001f) return;

        player.rotation = Quaternion.LookRotation(aimDirection);
    }
}
