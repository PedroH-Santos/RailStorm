public interface IHoldToAimSkill
{
    float GetAimMaxDistance(int level);
    float GetAimAreaRadius(int level);
    float AimTravelSeconds { get; }
    SkillAimMarker AimMarkerPrefab { get; }
}
