using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Decide, a cada início/fim de wave, qual evento (se algum) dispara em cada
/// categoria. Não conhece tipos concretos de evento — só a lista de
/// <see cref="EventController"/> presentes na cena. Adicionar um tipo de evento
/// novo nunca exige alterar esta classe: basta criar uma nova subclasse de
/// EventController e colocá-la na lista abaixo.
/// </summary>
public class EventOrchestrator : MonoBehaviour
{
    [Header("Um EventController por tipo de evento presente na cena.")]
    [SerializeField] private List<EventController> controllers = new();

    void OnEnable()
    {
        EnemySpawner.OnWaveStarted += HandleWaveStarted;
        EnemySpawner.OnWaveCleared += HandleWaveCleared;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= HandleWaveStarted;
        EnemySpawner.OnWaveCleared -= HandleWaveCleared;
    }

    void HandleWaveStarted() => HandleTrigger(EEventTiming.DuringWave);
    void HandleWaveCleared() => HandleTrigger(EEventTiming.AfterWave);

    void HandleTrigger(EEventTiming trigger)
    {
        // Quem não pertence a este gatilho perde elegibilidade agora: cancela
        // um spawn pendente e despawna a instância ativa, se houver.
        foreach (var controller in controllers)
        {
            if (controller == null || controller.Definition == null) continue;
            if (controller.Timing != trigger) controller.Cancel();
        }

        // Entre os elegíveis para este gatilho, sorteia no máximo 1 vencedor POR
        // CATEGORIA, de forma independente. Uma categoria com algum evento já
        // ativo (IsOccupied) nem participa do sorteio neste gatilho — equivale a
        // descartar o resultado, sem fila e sem interromper o evento em andamento.
        var eligible = controllers.Where(c => c != null && c.Definition != null && c.Timing == trigger);

        foreach (var group in eligible.GroupBy(c => c.Category))
        {
            var candidates = group.ToList();
            if (candidates.Any(c => c.IsOccupied)) continue;

            RollWinner(candidates)?.Activate();
        }
    }

    /// <summary>
    /// Roleta idêntica à lógica original: cada candidato ocupa uma fatia de
    /// 0..1 do tamanho do seu SpawnChance; o que sobra é "ninguém vence".
    /// </summary>
    static EventController RollWinner(IReadOnlyList<EventController> candidates)
    {
        float roll = Random.value;
        float acc = 0f;

        foreach (var candidate in candidates)
        {
            acc += Mathf.Max(0f, candidate.SpawnChance);
            if (roll < acc) return candidate;
        }

        return null;
    }
}
