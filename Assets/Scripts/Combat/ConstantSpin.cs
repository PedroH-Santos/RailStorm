using UnityEngine;

public class ConstantSpin : MonoBehaviour
{
    [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 0f, 360f);

    void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
}
