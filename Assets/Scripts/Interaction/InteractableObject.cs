using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base para objetos "chegue perto e aperte E": controla o range via
/// SphereCollider trigger, mostra/esconde o <see cref="InteractPromptUI"/>, faz o
/// polling da tecla E e resolve o ciclo de vida (consumido -> destruído). Usada
/// por eventos com ativação "Interact" (ex.: baú, totem de horda).
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public abstract class InteractableObject : MonoBehaviour
{
    [Header("Interação")]
    [SerializeField] protected float interactRadius = 2f;

    public event System.Action OnConsumedOrDespawned;

    protected bool PlayerInRange { get; private set; }
    protected bool Consumed { get; private set; }

    protected virtual void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = interactRadius;
        col.isTrigger = true;
    }

    protected virtual void Update()
    {
        if (Consumed || !PlayerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OnInteract();
    }

    /// Implementa o efeito da interação (abrir baú, aceitar totem, etc.).
    protected abstract void OnInteract();

    /// Marca a interação como resolvida (some com o prompt, para de reagir a E)
    /// sem necessariamente destruir o objeto já (ex.: baú com popup de escolha aberto).
    protected void SuppressInteraction()
    {
        Consumed = true;
        InteractPromptUI.Instance?.Hide();
    }

    /// Encerra de vez o ciclo de vida: notifica listeners (spawner) e destrói.
    protected void FinishLifecycle()
    {
        OnConsumedOrDespawned?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Cancelamento externo (spawner/orchestrator) de uma instância ainda não
    /// consumida. Não-op se o jogador já iniciou a interação (ex.: baú com popup
    /// de escolha aberto — nesse caso o próprio fluxo de decisão encerra o ciclo
    /// de vida via FinishLifecycle, não via Despawn externo).
    /// </summary>
    public virtual void Despawn()
    {
        if (Consumed) return;

        SuppressInteraction();
        FinishLifecycle();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (Consumed || !other.CompareTag("Player")) return;

        PlayerInRange = true;
        InteractPromptUI.Instance?.Show();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInRange = false;
        if (!Consumed) InteractPromptUI.Instance?.Hide();
    }
}
