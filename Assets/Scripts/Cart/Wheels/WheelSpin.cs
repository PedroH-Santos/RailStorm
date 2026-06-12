using StarterAssets;
using UnityEngine;

public class WheelSpin : MonoBehaviour
{
    public float radius = 0.3f;
    [SerializeField]
    private PlayerController _player; 

    void Update()
    {
        float speed = _player.CurrentSpeed; 
        float angularSpeed = (speed / radius) * Mathf.Rad2Deg;
        transform.Rotate(Vector3.right * angularSpeed * Time.deltaTime, Space.Self);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, transform.right, radius);
    }

    void DrawCircle(Vector3 center, Vector3 normal, float r)
    {
        Vector3 forward = Vector3.Slerp(normal, -normal, 0.5f);
        forward = Vector3.Cross(normal, Vector3.up);
        if (forward == Vector3.zero) forward = Vector3.Cross(normal, Vector3.right);
        forward.Normalize();
        Vector3 right = Vector3.Cross(forward, normal).normalized;

        int segments = 32;
        Vector3 prevPoint = center + forward * r;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 nextPoint = center + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * r;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}