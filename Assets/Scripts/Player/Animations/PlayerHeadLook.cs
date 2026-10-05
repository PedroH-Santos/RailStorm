using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerHeadLook : MonoBehaviour
{
    [SerializeField] private Transform neck;
    [SerializeField] private Transform head;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Alvos")]
    [Tooltip("Distância máxima até um ponto de interesse (marcador do minimapa).")]
    [SerializeField] private float lookRadius = 7f;

    [Header("Limites")]
    [SerializeField] private float maxYaw = 60f;
    [SerializeField] private float maxPitch = 20f;
    [Tooltip("Alvo mais para trás do que maxYaw × este fator é ignorado.")]
    [SerializeField] private float ignoreBehindFactor = 1.4f;
    [Range(0f, 1f)]
    [SerializeField] private float neckShare = 0.3f;

    [Header("Suavização")]
    [Tooltip("Segundos para entrar e sair do olhar.")]
    [SerializeField] private float blendTime = 0.25f;
    [SerializeField] private float aimSmoothing = 0.15f;

    float _weight;
    float _yaw;
    float _pitch;

    void Awake()
    {
        if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
        if (neck == null) neck = FindBone("Neck");
        if (head == null) head = FindBone("Head");
    }

    Transform FindBone(string boneName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == boneName) return t;
        return null;
    }

    void LateUpdate()
    {
        if (neck == null || head == null) return;

        float dt = Time.deltaTime;
        float yaw = 0f;
        float pitch = 0f;
        bool canLook = animationController == null || animationController.IsInIdlePose;
        bool hasTarget = canLook && TryGetTargetAngles(out yaw, out pitch);

        if (hasTarget)
        {
            float k = aimSmoothing > 0f ? 1f - Mathf.Exp(-dt / aimSmoothing) : 1f;
            _yaw = Mathf.Lerp(_yaw, yaw, k);
            _pitch = Mathf.Lerp(_pitch, pitch, k);
        }

        float step = blendTime > 0f ? dt / blendTime : 1f;
        _weight = Mathf.MoveTowards(_weight, hasTarget ? 1f : 0f, step);
        if (_weight <= 0f) return;

        Apply(neck, neckShare);
        Apply(head, 1f - neckShare);
    }

    void Apply(Transform bone, float share)
    {
        float amount = _weight * share;
        bone.rotation = Quaternion.AngleAxis(_yaw * amount, transform.up)
                        * Quaternion.AngleAxis(-_pitch * amount, transform.right)
                        * bone.rotation;
    }

    bool TryGetTargetAngles(out float yaw, out float pitch)
    {
        yaw = 0f;
        pitch = 0f;

        MinimapMarker best = null;
        float bestDistance = lookRadius;
        Vector3 origin = head.position;

        foreach (var marker in MinimapMarkerRegistry.All)
        {
            if (marker == null || marker.transform.IsChildOf(transform.root)) continue;

            float distance = Vector3.Distance(origin, marker.WorldPosition);
            if (distance > bestDistance) continue;

            bestDistance = distance;
            best = marker;
        }

        if (best == null) return false;

        Vector3 local = transform.InverseTransformDirection(best.WorldPosition - origin);
        float horizontal = new Vector2(local.x, local.z).magnitude;
        float rawYaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        if (Mathf.Abs(rawYaw) > maxYaw * ignoreBehindFactor) return false;

        yaw = Mathf.Clamp(rawYaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(Mathf.Atan2(local.y, horizontal) * Mathf.Rad2Deg, -maxPitch, maxPitch);
        return true;
    }
}
