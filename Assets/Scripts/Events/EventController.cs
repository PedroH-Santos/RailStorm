using System.Collections;
using UnityEngine;

/// <summary>
/// Base de todo "tipo de evento" plugável no <see cref="EventOrchestrator"/>.
/// Um tipo de evento novo é uma nova subclasse aqui (+ opcionalmente uma
/// subclasse de <see cref="EventDefinition"/>, se precisar de campos extras de
/// configuração) — o orquestrador nunca precisa ser alterado.
///
/// Concentra a lógica hoje duplicada por tipo no antigo EventOrchestrator: gestão
/// do delay de spawn, do cancelamento e do ciclo de repetição. Cada subclasse
/// concreta só implementa <see cref="IsOccupied"/>, <see cref="ExecuteActivation"/>
/// e <see cref="DespawnActive"/>.
/// </summary>
public abstract class EventController : MonoBehaviour
{
    [SerializeField] protected EventDefinition definition;

    Coroutine _activeLoop;

    public EventDefinition Definition => definition;
    public EEventCategory Category => definition.category;
    public EEventTiming Timing => definition.timing;
    public float SpawnChance => definition.spawnChance;

    /// True enquanto a instância ativa deste evento (objeto no mundo, efeito em
    /// andamento etc.) ainda ocupa a categoria e deve bloquear o sorteio de
    /// eventos irmãos.
    public abstract bool IsOccupied { get; }

    /// <summary>
    /// Chamado pelo orchestrator quando este controller vence o sorteio da categoria.
    /// Não-op se já houver um ciclo em andamento (spawn pendente, instância ativa
    /// aguardando o jogador, ou repetição agendada) — o sorteio só decide se o
    /// ciclo COMEÇA; uma vez começado, quem controla os spawns seguintes (inclusive
    /// repetições) é este próprio ciclo, não novos sorteios.
    /// </summary>
    public void Activate()
    {
        if (definition == null || _activeLoop != null || IsOccupied) return;

        _activeLoop = StartCoroutine(ActivationLoop());
    }

    /// <summary>
    /// Chamado pelo orchestrator quando este evento deixa de ser elegível para o
    /// gatilho atual: interrompe o ciclo (em qualquer fase: aguardando spawnar,
    /// aguardando o jogador consumir a instância ativa, ou aguardando repetir) e
    /// despawna a instância ativa, se houver.
    /// </summary>
    public void Cancel()
    {
        if (_activeLoop != null)
        {
            StopCoroutine(_activeLoop);
            _activeLoop = null;
        }

        DespawnActive();
    }

    IEnumerator ActivationLoop()
    {
        do
        {
            yield return WaitDelay(definition.spawnDelayRange);
            ExecuteActivation();

            if (!definition.repeats) break;

            // Aguarda a instância spawnada deixar de ocupar a categoria (consumida
            // pelo jogador ou despawnada) antes de agendar a próxima.
            yield return new WaitUntil(() => !IsOccupied);
            yield return WaitDelay(definition.repeatDelayRange);
        }
        while (true);

        _activeLoop = null;
    }

    static IEnumerator WaitDelay(Vector2 range)
    {
        float min = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, range.y);
        float delay = Random.Range(min, max);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);
    }

    /// Executa o efeito/spawn de fato (delay já decorrido).
    protected abstract void ExecuteActivation();

    /// Cancela/despawna qualquer instância ativa deste evento agora mesmo.
    protected abstract void DespawnActive();
}
