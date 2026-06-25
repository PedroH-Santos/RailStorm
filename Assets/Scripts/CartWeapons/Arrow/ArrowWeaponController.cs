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

    private bool _active;           // true only when the arrow weapon is equipped
    private float _fireTimer;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

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

        // fireRate is in salvos-per-second; interval = 1 / fireRate.
        _fireTimer += Time.deltaTime;

        float interval = 1f / Mathf.Max(0.01f, _weaponDefinition.CurrentStats.fireRate);

        if (_fireTimer >= interval)
        {
            FireSalvo();
            _fireTimer = 0f;
        }
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    /// <summary>
    /// Reacts to any change in the weapon list.
    /// Searches for a weapon of type Arrow and enables/disables accordingly.
    /// </summary>
    private void HandleWeaponsChanged()
    {
        WeaponDefinition found = FindArrowWeapon();

        if (found == null)
        {
            // Arrow was exiled or not yet acquired — disable silently.
            Deactivate();
            return;
        }

        if (!_active)
        {
            // First acquisition.
            _weaponDefinition = found;
            _fireTimer = 1f / Mathf.Max(0.01f, _weaponDefinition.CurrentStats.fireRate); // fire immediately
            _active = true;
            Debug.Log($"[ArrowWeaponBehaviour] Activated — rarity {_weaponDefinition.CurrentRarity}, " +
                      $"DMG {_weaponDefinition.CurrentStats.damage}, " +
                      $"rate {_weaponDefinition.CurrentStats.fireRate}/s, " +
                      $"range {_weaponDefinition.CurrentStats.range}, " +
                      $"speed {_weaponDefinition.CurrentStats.speed}");
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

    /// <summary>
    /// Fires one salvo: 2 arrows to the left, 2 to the right.
    /// All values come from <see cref="WeaponDefinition.CurrentStats"/>.
    /// </summary>
    private void FireSalvo()
    {
        WeaponLevelData stats = _weaponDefinition.CurrentStats;

        SpawnArrows(leftFirePoints, -transform.right, stats);
        SpawnArrows(rightFirePoints, transform.right, stats);
    }

    /// <summary>Spawns one arrow per fire point in the given lateral direction.</summary>
    private void SpawnArrows(Transform[] firePoints, Vector3 direction, WeaponLevelData stats)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("[ArrowWeaponBehaviour] arrowPrefab is not assigned.");
            return;
        }

        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue;

            GameObject obj = Instantiate(arrowPrefab, fp.position, fp.rotation);

            if (obj.TryGetComponent<ArrowProjectile>(out var arrow))
                arrow.Init(direction, stats.speed, stats.range, stats.damage);
            else
                Debug.LogWarning("[ArrowWeaponBehaviour] arrowPrefab is missing ArrowProjectile component.");
        }
    }
}