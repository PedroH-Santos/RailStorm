/// <summary>
/// Como um evento passa a existir no mundo quando é sorteado. Metadado de autoria:
/// quem decide o que fazer com isso é a própria subclasse de <see cref="EventController"/>
/// (spawna um objeto interativo ou aplica um efeito direto) — o orquestrador nunca
/// bifurca com base neste valor.
/// </summary>
public enum EEventActivation
{
    /// Aplica o efeito direto, sem nenhum objeto no mundo (ex.: futuro evento de clima).
    Automatic,

    /// Spawna um objeto e espera o jogador chegar perto e apertar E.
    Interact
}
