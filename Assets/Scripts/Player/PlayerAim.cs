using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAim : MonoBehaviour
{
    const float StickDeadZoneSqr = 0.1f;
    const float MinTurnDistanceSqr = 0.001f;
    const float MouseRayDistance = 100f;

    [Header("Mira")]
    [Tooltip("Modelo do personagem, que gira para olhar na direção da mira.")]
    [FormerlySerializedAs("player")]
    [SerializeField] private Transform model;
    [Tooltip("Camadas do chão, usadas para saber onde o mouse aponta.")]
    [SerializeField] private LayerMask groundMask;

    [Header("Disparo")]
    [SerializeField] private Transform firePoint;

    public Transform Model => model != null ? model : transform;
    public Transform FirePoint => firePoint;
    public Vector3 FacingDirection => Model.forward;
    public LayerMask GroundMask => groundMask;

    void Update()
    {
        if (model == null) return;

        if (TryReadStickDirection(out var direction) || TryReadMouseDirection(out direction))
            TurnModelTowards(direction);
    }

    static bool TryReadStickDirection(out Vector3 direction)
    {
        direction = Vector3.zero;
        if (Gamepad.current == null) return false;

        Vector2 stick = Gamepad.current.leftStick.ReadValue();
        if (stick.sqrMagnitude <= StickDeadZoneSqr) return false;

        direction = new Vector3(stick.x, 0f, stick.y);
        return true;
    }

    bool TryReadMouseDirection(out Vector3 direction)
    {
        direction = Vector3.zero;
        var camera = Camera.main;
        if (Mouse.current == null || camera == null) return false;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, MouseRayDistance, groundMask)) return false;

        Vector3 pointAtModelHeight = hit.point;
        pointAtModelHeight.y = model.position.y;
        direction = pointAtModelHeight - model.position;
        return true;
    }

    void TurnModelTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < MinTurnDistanceSqr) return;
        model.rotation = Quaternion.LookRotation(direction);
    }
}
