public interface IHoldToAimSkill
{
    float GetAimMaxDistance(SkillLevel level);
    float GetAimAreaRadius(SkillLevel level);
    float AimTravelSeconds { get; }
    SkillAimMarker AimMarkerPrefab { get; }
}
