using UnityEngine;

public class HordeTotemInteractable : InteractableObject
{
    [Header("Horda")]
    [SerializeField] private HordeSpawner hordeSpawner;

    public static event System.Action OnHordeAccepted;

    public void SetHordeSpawner(HordeSpawner spawner) => hordeSpawner = spawner;

    protected override void OnInteract() => Accept();

    void Accept()
    {
        if (Consumed || hordeSpawner == null || hordeSpawner.IsActive) return;

        SuppressInteraction();
        hordeSpawner.TriggerHorde();
        OnHordeAccepted?.Invoke();
        FinishLifecycle();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.15f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
