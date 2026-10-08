using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerAim))]
public class SkillHoldToAimController : MonoBehaviour
{
    const float MinTravelSeconds = 0.01f;
    const float MinFacingSqr = 0.0001f;

    [SerializeField] private float groundProbeHeight = 20f;
    [SerializeField] private float groundProbeDistance = 50f;

    readonly Dictionary<SkillAimMarker, SkillAimMarker> _markerByPrefab = new();

    PlayerAim _playerAim;
    PlayerAnimationController _animation;
    SkillAimMarker _marker;
    float _maxDistance;
    float _travelSeconds;
    float _elapsed;

    public bool IsAiming { get; private set; }
    public Vector3 AimPoint { get; private set; }

    void Awake()
    {
        _playerAim = GetComponent<PlayerAim>();
        _animation = GetComponentInChildren<PlayerAnimationController>();
    }

    void Update()
    {
        if (IsAiming) MoveMarkerForward(Time.deltaTime);
    }

    public void Begin(IHoldToAimSkill skill, SkillLevel level)
    {
        _maxDistance = skill.GetAimMaxDistance(level);
        _travelSeconds = Mathf.Max(MinTravelSeconds, skill.AimTravelSeconds);
        _elapsed = 0f;

        _marker = GetMarker(skill.AimMarkerPrefab);
        if (_marker != null) _marker.Show(skill.GetAimAreaRadius(level));

        IsAiming = true;
        MoveMarkerForward(0f);

        if (_animation != null) _animation.BeginAimHold();
    }

    public Vector3 Release()
    {
        StopAiming();
        if (_animation != null) _animation.ReleaseAimHold();
        return AimPoint;
    }

    public void Cancel()
    {
        if (!IsAiming) return;

        StopAiming();
        if (_animation != null) _animation.CancelAimHold();
    }

    void MoveMarkerForward(float deltaTime)
    {
        _elapsed += deltaTime;
        float distance = Mathf.Min(_maxDistance, _maxDistance * _elapsed / _travelSeconds);
        AimPoint = GroundPointInFront(distance);

        if (_marker == null) return;
        _marker.MoveTo(AimPoint);
        _marker.SetRangeProgress(distance / _maxDistance);
        _marker.SetReachedMax(distance >= _maxDistance);
    }

    void StopAiming()
    {
        IsAiming = false;
        if (_marker != null) _marker.Hide();
        _marker = null;
    }

    Vector3 GroundPointInFront(float distance)
    {
        var model = _playerAim.Model;
        Vector3 facing = _playerAim.FacingDirection;
        facing.y = 0f;
        if (facing.sqrMagnitude < MinFacingSqr) facing = model.forward;

        Vector3 pointAtModelHeight = model.position + facing.normalized * distance;
        return ProjectOntoGround(pointAtModelHeight);
    }

    Vector3 ProjectOntoGround(Vector3 point)
    {
        Vector3 probeOrigin = point + Vector3.up * groundProbeHeight;
        bool hitGround = Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, groundProbeDistance,
            _playerAim.GroundMask, QueryTriggerInteraction.Ignore);

        return hitGround ? hit.point : point;
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
