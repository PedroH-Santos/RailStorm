using UnityEngine;

/// <summary>
/// Dado puro de autoria de um evento: identidade, categoria (grupo de exclusão
/// mútua), timing e chance de sorteio. Não guarda nenhuma referência de cena
/// (spawners, prefabs) — isso fica no <see cref="EventController"/> concreto que
/// consome esta definição.
/// </summary>
[CreateAssetMenu(fileName = "NewEvent", menuName = "Events/Event Definition")]
public class EventDefinition : ScriptableObject
{
    [Header("Identidade")]
    public string eventName = "Novo Evento";
    [TextArea] public string description = "";

    [Header("Categoria (grupo de exclusão mútua)")]
    public EEventCategory category = EEventCategory.Loot;

    [Header("Quando dispara")]
    public EEventTiming timing = EEventTiming.DuringWave;
    public EEventActivation activation = EEventActivation.Interact;

    [Header("Sorteio")]
    [Tooltip("Chance (0-1) deste evento ser o vencedor do sorteio da sua categoria, quando elegível.")]
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    [Tooltip("Atraso (segundos, min/max) entre o evento ser sorteado e ele realmente ser ativado.")]
    public Vector2 spawnDelayRange = Vector2.zero;

    [Header("Repetição")]
    [Tooltip("Se marcado, assim que a instância ativa deste evento for consumida/despawnada, ele se " +
             "reativa sozinho depois de 'Repeat Delay Range' — sem precisar vencer um novo sorteio. " +
             "Continua se repetindo até o evento deixar de ser elegível (ex.: a wave terminar).")]
    public bool repeats = false;

    [Tooltip("Atraso (segundos, min/max) entre a instância anterior ser consumida e a próxima ativar, quando 'Repeats' está marcado.")]
    public Vector2 repeatDelayRange = Vector2.zero;
}
