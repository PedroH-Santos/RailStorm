/// <summary>
/// Grupo de exclusão mútua: eventos da MESMA categoria nunca ficam ativos ao mesmo
/// tempo (o <see cref="EventOrchestrator"/> sorteia no máximo 1 vencedor por
/// categoria, por gatilho). Categorias diferentes rodam de forma 100% independente
/// entre si.
///
/// Extensão futura (não implementado ainda): ao criar Fogo e Eletrocutado, use um
/// valor novo compartilhado por ambos (ex.: "EnemyStatus") para que se excluam entre
/// si. Ao criar Clima, use um valor próprio (ex.: "Weather") para que rode em
/// paralelo com tudo o mais. Nenhum desses casos exige alterar EventOrchestrator.cs
/// — só adicionar o valor aqui e a subclasse de EventController correspondente.
/// </summary>
public enum EEventCategory
{
    /// Baú de itens.
    Loot,

    /// Horda de inimigos.
    Combat
}
