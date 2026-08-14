using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class SellZone : MonoBehaviour
{
    [Header("Interação")]
    [SerializeField] private float interactRadius = 3f;
    [SerializeField] private Vector3 interactCenter = Vector3.zero;

    [Header("References")]
    [SerializeField] private SellUI sellUI;
    [SerializeField] private SellManager sellManager;

    bool _playerInside;
    bool _sellOpen;
    PlayerController _player;

    void Awake() => ConfigureCollider();

    void OnValidate() => ConfigureCollider();

    void OnDrawGizmosSelected()
    {
        ConfigureCollider();

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.TransformPoint(interactCenter), interactRadius);
    }

    void ConfigureCollider()
    {
        var col = GetComponent<SphereCollider>();
        if (col == null) return;

        Vector3 worldScale = transform.lossyScale;
        float largestScaleAxis = Mathf.Max(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y), Mathf.Abs(worldScale.z));

        col.radius = largestScaleAxis > 0.0001f ? interactRadius / largestScaleAxis : interactRadius;
        col.center = DivideComponentWise(interactCenter, worldScale);
        col.isTrigger = true;
    }

    static Vector3 DivideComponentWise(Vector3 value, Vector3 scale) => new Vector3(
        Mathf.Abs(scale.x) > 0.0001f ? value.x / scale.x : value.x,
        Mathf.Abs(scale.y) > 0.0001f ? value.y / scale.y : value.y,
        Mathf.Abs(scale.z) > 0.0001f ? value.z / scale.z : value.z);

    void Update()
    {
        if (_sellOpen)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseSellUI();

            return;
        }

        if (!_playerInside) return;

        InteractPromptUI.Instance?.Show();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            OpenSellUI();
    }

    void OpenSellUI()
    {
        _sellOpen = true;
        InteractPromptUI.Instance?.Hide();
        _player.SetMovementLocked(true);

        SellManager manager = sellManager != null ? sellManager : SellManager.Instance;

        sellUI.Open(
            _player.GetComponent<PlayerStatsAggregator>(),
            _player.GetComponent<PlayerItemHandler>(),
            manager);
    }

    void CloseSellUI()
    {
        _sellOpen = false;
        _player?.SetMovementLocked(false);
        sellUI.Close();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        _player = other.GetComponent<PlayerController>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;
        InteractPromptUI.Instance?.Hide();

        if (_sellOpen) CloseSellUI();
    }
}
