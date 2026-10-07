using System;
using UnityEngine;

public class BurnReceiver : MonoBehaviour
{
    [Tooltip("Guardiões e mini-chefes precisam de mais acúmulos para queimar.")]
    [SerializeField] private bool isBoss;

    BurnDefinition _burn;
    LifeSystem _life;
    HitFlash _flash;
    GameObject _flame;
    BurnIndicator _indicator;

    int _stacks;
    float _secondsUntilStacksFade;
    float _secondsOfBurnLeft;
    float _secondsUntilNextTick;

    public BurnDefinition Burn => _burn;
    public bool IsBurning => _secondsOfBurnLeft > 0f;
    public int Stacks => _stacks;
    public int StacksToIgnite => _burn != null ? _burn.StacksToIgnite(isBoss) : 1;
    public float IgnitionProgress => IsBurning ? 1f : Mathf.Clamp01(_stacks / (float)StacksToIgnite);

    public event Action<BurnReceiver> OnStacksChanged;
    public event Action<BurnReceiver> OnIgnited;
    public event Action<BurnReceiver> OnExtinguished;

    public static void AddStacks(GameObject target, BurnDefinition burn, int stacks)
    {
        if (target == null || burn == null || stacks <= 0) return;

        if (!target.TryGetComponent<BurnReceiver>(out var receiver))
            receiver = target.AddComponent<BurnReceiver>();

        receiver.ReceiveStacks(burn, stacks);
    }

    public static bool IsTargetBurning(GameObject target)
        => target != null && target.TryGetComponent<BurnReceiver>(out var receiver) && receiver.IsBurning;

    void Awake()
    {
        _life = GetComponent<LifeSystem>();
        _flash = GetComponent<HitFlash>();
    }

    void OnDestroy()
    {
        if (_indicator != null) Destroy(_indicator.gameObject);
    }

    void Update()
    {
        if (IsBurning) KeepBurning();
        else if (_stacks > 0) FadeStacksWhenNotHitForAWhile();
    }

    void ReceiveStacks(BurnDefinition burn, int stacks)
    {
        bool stacksAreIgnoredWhileBurning = IsBurning;
        if (stacksAreIgnoredWhileBurning || IsDead) return;

        _burn = burn;
        _stacks += stacks;
        _secondsUntilStacksFade = burn.stackDecaySeconds;
        ShowIndicator();

        if (_stacks >= StacksToIgnite) Ignite();
        else NotifyStacksChanged();
    }

    void Ignite()
    {
        _stacks = 0;
        _secondsOfBurnLeft = _burn.burnDuration;
        _secondsUntilNextTick = _burn.tickInterval;

        ShowFlame();
        if (_flash != null) _flash.StartGlowing(_burn.burnTint, _burn.burnBodyTint, _burn.tintPulseSpeed);

        OnIgnited?.Invoke(this);
        NotifyStacksChanged();
    }

    void KeepBurning()
    {
        _secondsOfBurnLeft -= Time.deltaTime;
        DealBurnTicksThatAreDue();

        if (_secondsOfBurnLeft <= 0f) Extinguish();
    }

    void DealBurnTicksThatAreDue()
    {
        _secondsUntilNextTick -= Time.deltaTime;

        while (_secondsUntilNextTick <= 0f)
        {
            _secondsUntilNextTick += _burn.tickInterval;
            if (_life != null) _life.Damage(_burn.DamagePerTick);
        }
    }

    void Extinguish()
    {
        _secondsOfBurnLeft = 0f;
        _stacks = 0;

        HideFlame();
        if (_flash != null) _flash.StopGlowing();

        OnExtinguished?.Invoke(this);
        NotifyStacksChanged();
    }

    void FadeStacksWhenNotHitForAWhile()
    {
        _secondsUntilStacksFade -= Time.deltaTime;
        if (_secondsUntilStacksFade > 0f) return;

        _stacks = 0;
        NotifyStacksChanged();
    }

    bool IsDead => _life != null && _life.IsDead;

    void NotifyStacksChanged() => OnStacksChanged?.Invoke(this);

    void ShowIndicator()
    {
        if (_indicator != null || _burn.indicatorPrefab == null) return;

        _indicator = Instantiate(_burn.indicatorPrefab);
        _indicator.Track(this);
    }

    void ShowFlame()
    {
        if (_flame == null) _flame = SpawnFlameOverHead();
        if (_flame != null) _flame.SetActive(true);
    }

    void HideFlame()
    {
        if (_flame != null) _flame.SetActive(false);
    }

    GameObject SpawnFlameOverHead()
    {
        if (_burn.flamePrefab == null) return null;

        var flame = Instantiate(_burn.flamePrefab, transform);
        flame.transform.localScale = InverseOfWorldScale(transform);
        flame.transform.position = transform.position + _burn.flameOffset;
        flame.transform.localRotation = Quaternion.identity;
        return flame;
    }

    static Vector3 InverseOfWorldScale(Transform parent)
    {
        Vector3 scale = parent.lossyScale;
        return new Vector3(SafeInverse(scale.x), SafeInverse(scale.y), SafeInverse(scale.z));
    }

    static float SafeInverse(float value) => Mathf.Approximately(value, 0f) ? 1f : 1f / value;
}
