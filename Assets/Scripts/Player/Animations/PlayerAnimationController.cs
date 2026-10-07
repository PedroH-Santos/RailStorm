using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    public const int BaseLayer = 0;
    public const int ActionLayer = 1;
    public const int ReactionLayer = 2;

    static readonly int AttackHash = Animator.StringToHash("attack");
    static readonly int InCombatHash = Animator.StringToHash("inCombat");
    static readonly int CelebrateHash = Animator.StringToHash("celebrate");
    static readonly int QuickCelebrateHash = Animator.StringToHash("quickCelebrate");
    static readonly int JoltHash = Animator.StringToHash("jolt");
    static readonly int AimingHash = Animator.StringToHash("aiming");
    static readonly int AimStartHash = Animator.StringToHash("aimStart");
    static readonly int AimReleaseHash = Animator.StringToHash("aimRelease");
    static readonly int GestureHash = Animator.StringToHash("gesture");
    static readonly int GestureIndexHash = Animator.StringToHash("gestureIndex");
    static readonly int RelaxedStateHash = Animator.StringToHash("Idle_Relaxed");
    static readonly int CelebrateStateHash = Animator.StringToHash("Celebrate");
    static readonly int QuickCelebrateStateHash = Animator.StringToHash("Celebrate_Quick");

    [Header("Gestos fora de combate")]
    [SerializeField] private Vector2 gestureInterval = new(8f, 15f);
    [SerializeField] private int gestureCount = 2;

    [Header("Fogo na mão nas comemorações")]
    [SerializeField] private ParticleSystem handFlamePrefab;
    [SerializeField] private Transform handFlameBone;
    [SerializeField] private Vector3 handFlameOffset = new(0f, 0.25f, 0f);
    [SerializeField] private float handFlameScale = 0.7f;
    [Tooltip("Trecho (tempo normalizado) da Celebrate com fogo na mão: frames 13–34 de 58.")]
    [SerializeField] private Vector2 celebrateFlameWindow = new(13f / 58f, 34f / 58f);
    [Tooltip("Trecho (tempo normalizado) da Celebrate_Quick com fogo na mão: frames 8–20 de 30.")]
    [SerializeField] private Vector2 quickCelebrateFlameWindow = new(8f / 30f, 20f / 30f);

    Animator _animator;
    PlayerItemHandler _items;
    PlayerPerkHandler _perks;
    PlayerSkillHandler _skills;
    PlayerCarWeaponHandler _carWeapons;
    PlayerController _cart;
    ParticleSystem _handFlame;
    bool _handFlameOn;
    bool _inCombat;
    bool _pendingQuickCelebrate;
    float _gestureTimer;
    int _progressScore;

    public bool InCombat => _inCombat;

    public bool IsInIdlePose
    {
        get
        {
            if (_animator == null || _animator.IsInTransition(BaseLayer) || _animator.IsInTransition(ActionLayer)) return false;
            return _animator.GetCurrentAnimatorStateInfo(BaseLayer).IsTag("Idle")
                   && _animator.GetCurrentAnimatorStateInfo(ActionLayer).IsTag("Empty");
        }
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _items = GetComponentInParent<PlayerItemHandler>();
        _perks = GetComponentInParent<PlayerPerkHandler>();
        _skills = GetComponentInParent<PlayerSkillHandler>();
        _cart = GetComponentInParent<PlayerController>();
        ResetGestureTimer();
    }

    void OnEnable()
    {
        EnemySpawner.OnWaveStarted += EnterCombat;
        EnemySpawner.OnWaveCleared += LeaveCombat;
        if (_items != null) _items.OnItemsChanged += HandleProgressChanged;
        if (_perks != null) _perks.OnPerksChanged += HandleProgressChanged;
        if (_skills != null) _skills.OnSkillsChanged += HandleProgressChanged;
        if (_cart != null) _cart.OnSplineSwitched += HandleSplineSwitched;
    }

    void Start()
    {
        _carWeapons = PlayerCarWeaponHandler.Instance;
        if (_carWeapons != null) _carWeapons.OnWeaponsChanged += HandleProgressChanged;
        _progressScore = ComputeProgressScore();

        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null && spawner.WaveInProgress) EnterCombat();

        if (handFlamePrefab != null)
        {
            _handFlame = Instantiate(handFlamePrefab);
            _handFlame.transform.localScale = Vector3.one * handFlameScale;
            _handFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= EnterCombat;
        EnemySpawner.OnWaveCleared -= LeaveCombat;
        if (_items != null) _items.OnItemsChanged -= HandleProgressChanged;
        if (_perks != null) _perks.OnPerksChanged -= HandleProgressChanged;
        if (_skills != null) _skills.OnSkillsChanged -= HandleProgressChanged;
        if (_carWeapons != null) _carWeapons.OnWeaponsChanged -= HandleProgressChanged;
        if (_cart != null) _cart.OnSplineSwitched -= HandleSplineSwitched;
    }

    void OnDestroy()
    {
        if (_handFlame != null) Destroy(_handFlame.gameObject);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        TryPlayPendingQuickCelebrate();
        UpdateGestures();
    }

    void LateUpdate()
    {
        UpdateHandFlame();
    }

    public void PlayAttackAnimation()
    {
        _animator.SetTrigger(AttackHash);
    }

    public void BeginAimHold()
    {
        _animator.ResetTrigger(AimReleaseHash);
        _animator.SetBool(AimingHash, true);
        _animator.SetTrigger(AimStartHash);
    }

    public void ReleaseAimHold()
    {
        _animator.ResetTrigger(AimStartHash);
        _animator.SetBool(AimingHash, false);
        _animator.SetTrigger(AimReleaseHash);
    }

    public void CancelAimHold()
    {
        _animator.ResetTrigger(AimStartHash);
        _animator.ResetTrigger(AimReleaseHash);
        _animator.SetBool(AimingHash, false);
    }

    public void EnterCombat()
    {
        _inCombat = true;
        _animator.ResetTrigger(CelebrateHash);
        _animator.SetBool(InCombatHash, true);
    }

    public void LeaveCombat()
    {
        _inCombat = false;
        _animator.SetBool(InCombatHash, false);
        _animator.SetTrigger(CelebrateHash);
        ResetGestureTimer();
    }

    public void PlayQuickCelebrate()
    {
        _pendingQuickCelebrate = true;
    }

    void HandleSplineSwitched()
    {
        _animator.SetTrigger(JoltHash);
    }

    void HandleProgressChanged()
    {
        int score = ComputeProgressScore();
        if (score > _progressScore) _pendingQuickCelebrate = true;
        _progressScore = score;
    }

    int ComputeProgressScore()
    {
        int score = 0;

        if (_items != null) score += _items.AcquiredItems.Count;

        if (_perks != null)
            foreach (var perk in _perks.AcquiredPerks)
                score += _perks.GetRarity(perk) + 1;

        if (_skills != null)
            foreach (var skill in _skills.Owned)
                score += skill.Level.Number;

        if (_carWeapons != null)
            foreach (var weapon in _carWeapons.AcquiredWeapons)
            {
                score += _carWeapons.GetRarity(weapon) + 1;
                foreach (var weaponPerk in _carWeapons.GetAppliedPerks(weapon))
                    score += _carWeapons.GetRarity(weaponPerk) + 1;
            }

        return score;
    }

    void TryPlayPendingQuickCelebrate()
    {
        if (!_pendingQuickCelebrate) return;

        var baseState = _animator.GetCurrentAnimatorStateInfo(BaseLayer);
        if (baseState.shortNameHash == CelebrateStateHash || _animator.IsInTransition(BaseLayer)) return;

        _pendingQuickCelebrate = false;
        _animator.SetTrigger(QuickCelebrateHash);
    }

    void UpdateGestures()
    {
        bool relaxedAndIdle = !_inCombat && IsInIdlePose
                              && _animator.GetCurrentAnimatorStateInfo(BaseLayer).shortNameHash == RelaxedStateHash;
        if (!relaxedAndIdle)
        {
            if (_gestureTimer < gestureInterval.x * 0.5f) ResetGestureTimer();
            return;
        }

        _gestureTimer -= Time.deltaTime;
        if (_gestureTimer > 0f) return;

        _animator.SetInteger(GestureIndexHash, Random.Range(0, Mathf.Max(1, gestureCount)));
        _animator.SetTrigger(GestureHash);
        ResetGestureTimer();
    }

    void ResetGestureTimer()
    {
        _gestureTimer = Random.Range(gestureInterval.x, gestureInterval.y);
    }

    void UpdateHandFlame()
    {
        if (_handFlame == null || handFlameBone == null) return;

        bool shouldBurn = IsInFlameWindow(BaseLayer, CelebrateStateHash, celebrateFlameWindow)
                          || IsInFlameWindow(ActionLayer, QuickCelebrateStateHash, quickCelebrateFlameWindow);

        _handFlame.transform.position = handFlameBone.position + handFlameOffset;

        if (shouldBurn == _handFlameOn) return;
        _handFlameOn = shouldBurn;

        if (shouldBurn) _handFlame.Play(true);
        else _handFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    bool IsInFlameWindow(int layer, int stateHash, Vector2 window)
    {
        var state = _animator.GetCurrentAnimatorStateInfo(layer);
        if (state.shortNameHash != stateHash) return false;

        float t = state.normalizedTime;
        return t >= window.x && t <= window.y;
    }
}
