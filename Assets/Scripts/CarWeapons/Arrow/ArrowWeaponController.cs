using UnityEngine;

[RequireComponent(typeof(PlayerCarWeaponHandler))]
public class ArrowWeaponController : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private Transform rightFirePoint;

    [SerializeField][Range(0f, 0.5f)] private float edgeMarginPercent = 0.1f;

    private PlayerCarWeaponHandler _weaponHandler;
    private CarWeaponDefinition _weaponDefinition;
    private ArrowLevelData _currentStats;
    private bool _active;
    private float _fireTimer;
    private float _cartLength = -1f;

    private void Awake()
    {
        _weaponHandler = GetComponent<PlayerCarWeaponHandler>();
    }

    private void OnEnable()
    {
        _weaponHandler.OnWeaponsChanged += HandleWeaponsChanged;
    }

    private void OnDisable()
    {
        _weaponHandler.OnWeaponsChanged -= HandleWeaponsChanged;
    }

    private void Update()
    {
        if (!_active) return;

        _currentStats = _weaponHandler.GetEffectiveStats<ArrowLevelData>(_weaponDefinition);
        if (_currentStats == null) return;

        _fireTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(0.01f, _currentStats.attackRate);

        if (_fireTimer >= interval)
        {
            FireSalvo();
            _fireTimer = 0f;
        }
    }

    private void HandleWeaponsChanged()
    {
        CarWeaponDefinition found = FindArrowWeapon();

        if (found == null) { Deactivate(); return; }

        _weaponDefinition = found;
        _currentStats = _weaponHandler.GetEffectiveStats<ArrowLevelData>(_weaponDefinition);

        if (!_active)
        {
            _fireTimer = 1f / Mathf.Max(0.01f, _currentStats.attackRate);
            _active = true;
        }
    }

    private void FireSalvo()
    {
        var stats = _weaponHandler.GetEffectiveStats<ArrowLevelData>(_weaponDefinition);
        if (stats == null) return;

        SpawnArrowFan(leftFirePoint, -transform.right, stats);
        SpawnArrowFan(rightFirePoint, transform.right, stats);
    }

    private void SpawnArrowFan(Transform firePoint, Vector3 direction, ArrowLevelData stats)
    {
        if (firePoint == null) return;

        int count = Mathf.Max(1, stats.arrowCount);
        Vector3 spreadAxis = transform.forward;

        float usableLength = GetCartLength() * (1f - edgeMarginPercent * 2f);
        float actualSpacing = count > 1 ? usableLength / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float offsetFactor = i - (count - 1) / 2f;
            Vector3 offset = spreadAxis * (offsetFactor * actualSpacing);

            var obj = Instantiate(arrowPrefab, firePoint.position + offset, firePoint.rotation);
            if (obj.TryGetComponent<ArrowProjectile>(out var arrow))
                arrow.Init(direction, stats.speed, stats.range, stats.damage);
        }
    }

    private float GetCartLength()
    {
        if (_cartLength > 0f) return _cartLength;

        var renderer = GetComponent<Renderer>();

        _cartLength = renderer.bounds.size.z;
    
        return _cartLength;
    }

    private void Deactivate()
    {
        if (!_active) return;
        _active = false;
        _weaponDefinition = null;
    }

    private CarWeaponDefinition FindArrowWeapon()
    {
        foreach (var w in _weaponHandler.AcquiredWeapons)
            if (w.weaponType == ECarWeaponType.Arrow)
                return w;
        return null;
    }
}