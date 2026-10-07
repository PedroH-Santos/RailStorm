using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerProgressCelebration : MonoBehaviour
{
    PlayerAnimationController _animation;
    PlayerItemHandler _items;
    PlayerPerkHandler _perks;
    PlayerSkillHandler _skills;
    PlayerCarWeaponHandler _carWeapons;

    int _progressScore;
    bool _celebrationPending;

    void Awake()
    {
        _animation = GetComponent<PlayerAnimationController>();
        _items = GetComponentInParent<PlayerItemHandler>();
        _perks = GetComponentInParent<PlayerPerkHandler>();
        _skills = GetComponentInParent<PlayerSkillHandler>();
    }

    void OnEnable()
    {
        if (_items != null) _items.OnItemsChanged += CelebrateIfProgressGrew;
        if (_perks != null) _perks.OnPerksChanged += CelebrateIfProgressGrew;
        if (_skills != null) _skills.OnSkillsChanged += CelebrateIfProgressGrew;
    }

    void Start()
    {
        _carWeapons = PlayerCarWeaponHandler.Instance;
        if (_carWeapons != null) _carWeapons.OnWeaponsChanged += CelebrateIfProgressGrew;

        _progressScore = ComputeProgressScore();
    }

    void OnDisable()
    {
        if (_items != null) _items.OnItemsChanged -= CelebrateIfProgressGrew;
        if (_perks != null) _perks.OnPerksChanged -= CelebrateIfProgressGrew;
        if (_skills != null) _skills.OnSkillsChanged -= CelebrateIfProgressGrew;
        if (_carWeapons != null) _carWeapons.OnWeaponsChanged -= CelebrateIfProgressGrew;
    }

    void Update()
    {
        bool gameIsRunning = Time.timeScale > 0f;
        if (!_celebrationPending || !gameIsRunning || !_animation.CanStartQuickCelebrate) return;

        _celebrationPending = false;
        _animation.PlayQuickCelebrate();
    }

    void CelebrateIfProgressGrew()
    {
        int score = ComputeProgressScore();
        if (score > _progressScore) _celebrationPending = true;
        _progressScore = score;
    }

    int ComputeProgressScore()
        => ItemsOwned() + PerkLevels() + SkillLevels() + CarWeaponLevels();

    int ItemsOwned() => _items != null ? _items.AcquiredItems.Count : 0;

    int PerkLevels()
    {
        if (_perks == null) return 0;

        int total = 0;
        foreach (var perk in _perks.AcquiredPerks) total += _perks.GetRarity(perk) + 1;
        return total;
    }

    int SkillLevels()
    {
        if (_skills == null) return 0;

        int total = 0;
        foreach (var skill in _skills.Owned) total += skill.Level.Number;
        return total;
    }

    int CarWeaponLevels()
    {
        if (_carWeapons == null) return 0;

        int total = 0;
        foreach (var weapon in _carWeapons.AcquiredWeapons)
        {
            total += _carWeapons.GetRarity(weapon) + 1;
            foreach (var weaponPerk in _carWeapons.GetAppliedPerks(weapon))
                total += _carWeapons.GetRarity(weaponPerk) + 1;
        }
        return total;
    }
}
