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
    float _decayTimer;
    float _burnTimer;
    float _tickTimer;

    public bool IsBurning => _burnTimer > 0f;
    public int Stacks => _stacks;
    public int StacksToIgnite => _burn != null ? _burn.StacksToIgnite(isBoss) : 1;
    public float FillNormalized => IsBurning ? 1f : Mathf.Clamp01(_stacks / (float)StacksToIgnite);
    public BurnDefinition Burn => _burn;

    public event Action<BurnReceiver> OnStacksChanged;
    public event Action<BurnReceiver> OnIgnited;
    public event Action<BurnReceiver> OnExtinguished;

    public static void ApplyStack(GameObject target, BurnDefinition burn, int stacks = 1)
    {
        if (target == null || burn == null || stacks <= 0) return;

        if (!target.TryGetComponent<BurnReceiver>(out var receiver))
            receiver = target.AddComponent<BurnReceiver>();

        receiver.AddStacks(burn, stacks);
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

    void AddStacks(BurnDefinition burn, int stacks)
    {
        if (IsBurning) return;
        if (_life != null && _life.IsDead) return;

        _burn = burn;
        _stacks += stacks;
        _decayTimer = burn.stackDecaySeconds;
        EnsureIndicator();

        if (_stacks >= burn.StacksToIgnite(isBoss))
        {
            Ignite();
            return;
        }

        OnStacksChanged?.Invoke(this);
    }

    void Ignite()
    {
        _stacks = 0;
        _burnTimer = _burn.burnDuration;
        _tickTimer = _burn.tickInterval;
        SetFlameVisible(true);
        if (_flash != null) _flash.SetSustainedGlow(_burn.burnTint, _burn.burnBodyTint, _burn.tintPulseSpeed);
        OnIgnited?.Invoke(this);
        OnStacksChanged?.Invoke(this);
    }

    void Update()
    {
        if (IsBurning)
        {
            UpdateBurn();
            return;
        }

        if (_stacks <= 0) return;

        _decayTimer -= Time.deltaTime;
        if (_decayTimer > 0f) return;

        _stacks = 0;
        OnStacksChanged?.Invoke(this);
    }

    void UpdateBurn()
    {
        _burnTimer -= Time.deltaTime;
        _tickTimer -= Time.deltaTime;

        while (_tickTimer <= 0f)
        {
            _tickTimer += _burn.tickInterval;
            if (_life != null) _life.Damage(_burn.DamagePerTick);
        }

        if (_burnTimer > 0f) return;

        _burnTimer = 0f;
        _stacks = 0;
        SetFlameVisible(false);
        if (_flash != null) _flash.ClearSustainedGlow();
        OnExtinguished?.Invoke(this);
        OnStacksChanged?.Invoke(this);
    }

    void EnsureIndicator()
    {
        if (_indicator != null || _burn.indicatorPrefab == null) return;

        _indicator = Instantiate(_burn.indicatorPrefab);
        _indicator.Bind(this);
    }

    void SetFlameVisible(bool visible)
    {
        if (visible && _flame == null && _burn != null && _burn.flamePrefab != null)
        {
            _flame = Instantiate(_burn.flamePrefab, transform);
            Vector3 scale = transform.lossyScale;
            _flame.transform.localScale = new Vector3(SafeInverse(scale.x), SafeInverse(scale.y), SafeInverse(scale.z));
            _flame.transform.position = transform.position + _burn.flameOffset;
            _flame.transform.localRotation = Quaternion.identity;
        }

        if (_flame != null) _flame.SetActive(visible);
    }

    static float SafeInverse(float value) => Mathf.Approximately(value, 0f) ? 1f : 1f / value;
}
