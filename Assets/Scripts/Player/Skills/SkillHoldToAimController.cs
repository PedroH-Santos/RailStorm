using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHoldToAimController : MonoBehaviour
{
    [SerializeField] private float groundProbeHeight = 20f;
    [SerializeField] private float groundProbeDistance = 50f;

    readonly Dictionary<SkillAimMarker, SkillAimMarker> _markerByPrefab = new();

    public IEnumerator HoldToAim(
        IHoldToAimSkill skill,
        int level,
        PlayerWeaponController weapon,
        Func<bool> isKeyHeld,
        Func<bool> isAimStillValid,
        Action<Vector3> onReleased)
    {
        float maxDistance = skill.GetAimMaxDistance(level);
        float travelSeconds = Mathf.Max(0.01f, skill.AimTravelSeconds);
        var marker = GetMarker(skill.AimMarkerPrefab);
        if (marker != null) marker.Show(skill.GetAimAreaRadius(level));

        float elapsed = 0f;
        Vector3 aimPoint = ProjectToGround(weapon, 0f);

        while (true)
        {
            if (!isAimStillValid())
            {
                if (marker != null) marker.Hide();
                yield break;
            }

            elapsed += Time.deltaTime;
            float distance = Mathf.Min(maxDistance, maxDistance * elapsed / travelSeconds);
            aimPoint = ProjectToGround(weapon, distance);

            if (marker != null)
            {
                marker.MoveTo(aimPoint);
                marker.SetReachedMax(distance >= maxDistance);
            }

            if (!isKeyHeld()) break;
            yield return null;
        }

        if (marker != null) marker.Hide();
        onReleased?.Invoke(aimPoint);
    }

    public void HideAllMarkers()
    {
        foreach (var marker in _markerByPrefab.Values)
            if (marker != null) marker.Hide();
    }

    Vector3 ProjectToGround(PlayerWeaponController weapon, float distance)
    {
        var owner = weapon.Owner;
        Vector3 direction = weapon.AimDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = owner.forward;

        Vector3 flatPoint = owner.position + direction.normalized * distance;
        Vector3 probeOrigin = flatPoint + Vector3.up * groundProbeHeight;

        if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, groundProbeDistance, weapon.GroundMask, QueryTriggerInteraction.Ignore))
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
