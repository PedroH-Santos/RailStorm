using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerWeaponController))]
public class SkillHoldToAimController : MonoBehaviour
{
    [SerializeField] private float groundProbeHeight = 20f;
    [SerializeField] private float groundProbeDistance = 50f;

    readonly Dictionary<SkillAimMarker, SkillAimMarker> _markerByPrefab = new();

    PlayerWeaponController _weapon;
    SkillAimMarker _marker;
    float _maxDistance;
    float _travelSeconds;
    float _elapsed;

    public bool IsAiming { get; private set; }
    public Vector3 AimPoint { get; private set; }

    PlayerAnimationController PlayerAnimation => _weapon.Animation;

    void Awake()
    {
        _weapon = GetComponent<PlayerWeaponController>();
    }

    void Update()
    {
        if (IsAiming) AdvanceAim(Time.deltaTime);
    }

    public void Begin(IHoldToAimSkill skill, SkillLevel level)
    {
        _maxDistance = skill.GetAimMaxDistance(level);
        _travelSeconds = Mathf.Max(0.01f, skill.AimTravelSeconds);
        _elapsed = 0f;

        _marker = GetMarker(skill.AimMarkerPrefab);
        if (_marker != null) _marker.Show(skill.GetAimAreaRadius(level));

        IsAiming = true;
        AdvanceAim(0f);

        if (PlayerAnimation != null) PlayerAnimation.BeginAimHold();
    }

    public Vector3 Release()
    {
        StopAiming();
        if (PlayerAnimation != null) PlayerAnimation.ReleaseAimHold();
        return AimPoint;
    }

    public void Cancel()
    {
        if (!IsAiming) return;

        StopAiming();
        if (PlayerAnimation != null) PlayerAnimation.CancelAimHold();
    }

    void AdvanceAim(float deltaTime)
    {
        _elapsed += deltaTime;
        float distance = Mathf.Min(_maxDistance, _maxDistance * _elapsed / _travelSeconds);
        AimPoint = ProjectToGround(distance);

        if (_marker == null) return;
        _marker.MoveTo(AimPoint);
        _marker.SetReachedMax(distance >= _maxDistance);
    }

    void StopAiming()
    {
        IsAiming = false;
        if (_marker != null) _marker.Hide();
        _marker = null;
    }

    Vector3 ProjectToGround(float distance)
    {
        var owner = _weapon.Owner;
        Vector3 direction = _weapon.AimDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = owner.forward;

        Vector3 flatPoint = owner.position + direction.normalized * distance;
        Vector3 probeOrigin = flatPoint + Vector3.up * groundProbeHeight;

        if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, groundProbeDistance, _weapon.GroundMask, QueryTriggerInteraction.Ignore))
            return hit.point;

        return flatPoint;
    }

    SkillAimMarker GetMarker(SkillAimMarker prefab)
    {
        if (prefab == null) return null;
        if (_markerByPrefab.TryGetValue(prefab, out var marker) && marker != null) return marker;

        marker = Instantiate(prefab);
        marker.Hide();
        _markerByPrefab[prefab] = marker;
        return marker;
    }
}
