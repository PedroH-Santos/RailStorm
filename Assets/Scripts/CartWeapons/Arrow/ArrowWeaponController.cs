using UnityEngine;

[RequireComponent(typeof(PlayerCartWeaponHandler))]
public class ArrowWeaponController : MonoBehaviour
{

    [Header("Arrow Prefab")]
    [Tooltip("Prefab that contains ArrowProjectile + Rigidbody + Collider (isTrigger).")]
    [SerializeField] private GameObject arrowPrefab;

    [Header("Fire Points — Left Side (front → rear)")]
    [SerializeField] private Transform[] leftFirePoints = new Transform[2];

    [Header("Fire Points — Right Side (front → rear)")]
    [SerializeField] private Transform[] rightFirePoints = new Transform[2];


    private PlayerCartWeaponHandler _weaponHandler;

    private WeaponDefinition _weaponDefinition;
    private ArrowLevelData _currentStats;

    private bool _active;          
    private float _fireTimer;


    private void Awake()
    {
        _weaponHandler = GetComponent<PlayerCartWeaponHandler>();
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
        WeaponDefinition found = FindArrowWeapon();

        if (found == null)
        {
            Deactivate();
            return;
        }

        if (!_active)
        {
            _weaponDefinition = found;
            _currentStats = _weaponDefinition.CurrentStats as ArrowLevelData;
            _fireTimer = 1f / Mathf.Max(0.01f, _weaponDefinition.CurrentStats.attackRate); 
            _active = true;
            Debug.Log($"[ArrowWeaponBehaviour] Activated — rarity {_weaponDefinition.CurrentRarity}, " +
                      $"DMG {_currentStats.damage}, " +
                      $"rate {_currentStats.attackRate}/s, " +
                      $"range {_currentStats.range}, " +
                      $"speed {_currentStats.speed}");
        }
        else
        {
            // Upgrade: the WeaponDefinition already updated currentRarity via Upgrade(),
            // so CurrentStats already reflects the new level — nothing else needed.
            Debug.Log($"[ArrowWeaponBehaviour] Upgraded → rarity {_weaponDefinition.CurrentRarity}");
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Iterates the acquired weapon list and returns the first Arrow weapon, or null.
    /// </summary>
    private WeaponDefinition FindArrowWeapon()
    {
        foreach (WeaponDefinition weapon in _weaponHandler.AcquiredWeapons)
            if (weapon.weaponType == EWeaponType.Arrow)
                return weapon;

        return null;
    }

    private void Deactivate()
    {
        if (!_active) return;

        _active = false;
        _weaponDefinition = null;
        Debug.Log("[ArrowWeaponBehaviour] Deactivated.");
    }


    private void FireSalvo()
    {
        var stats = _weaponDefinition.GetStats<ArrowLevelData>(_weaponDefinition.CurrentRarity);
        if (stats == null) return;

        SpawnArrows(leftFirePoints, -transform.right, stats);
        SpawnArrows(rightFirePoints, transform.right, stats);
    }

    private void SpawnArrows(Transform[] firePoints, Vector3 direction, ArrowLevelData stats)
    {
        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue;
            var obj = Instantiate(arrowPrefab, fp.position, fp.rotation);
            if (obj.TryGetComponent<ArrowProjectile>(out var arrow))
                arrow.Init(direction, stats.speed, stats.range, stats.damage);
        }
    }
}