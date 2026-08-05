using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class HordeTotemInteractable : MonoBehaviour
{
    [Header("Horda")]
    [SerializeField] private HordeSpawner hordeSpawner;

    [Header("Interação")]
    [SerializeField] private float interactRadius = 3f;

    public static event System.Action OnHordeAccepted;
    public event System.Action OnAcceptedOrDespawned;

    bool _playerInRange;
    bool _accepted;

    public void SetHordeSpawner(HordeSpawner spawner) => hordeSpawner = spawner;

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = interactRadius;
        col.isTrigger = true;
    }

    void Update()
    {
        if (_accepted || !_playerInRange || hordeSpawner == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Accept();
    }

    void Accept()
    {
        if (_accepted || hordeSpawner == null || hordeSpawner.IsActive) return;
        _accepted = true;

        InteractPromptUI.Instance?.Hide();
        hordeSpawner.TriggerHorde();
        OnHordeAccepted?.Invoke();
        OnAcceptedOrDespawned?.Invoke();

        Destroy(gameObject);
    }

    public void Despawn()
    {
        if (_accepted) return;
        InteractPromptUI.Instance?.Hide();
        OnAcceptedOrDespawned?.Invoke();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_accepted || !other.CompareTag("Player")) return;

        _playerInRange = true;
        InteractPromptUI.Instance?.Show();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
        if (!_accepted) InteractPromptUI.Instance?.Hide();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.15f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
