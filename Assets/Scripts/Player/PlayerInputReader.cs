using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }

    void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }
}
