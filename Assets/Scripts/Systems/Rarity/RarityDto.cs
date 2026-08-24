using UnityEngine;


namespace Assets.Scripts.Systems.Rarity
{
    [System.Serializable]
    public class RarityDto
    {
        [Tooltip("Identificador interno")]
        public string id = "Common";

        [Tooltip("Nome exibido na UI")]
        public string displayName = "Comum";

        [Tooltip("Cor usada nos cards e bordas")]
        public Color color = Color.white;

        [Tooltip("Placa 9-slice desenhada atrás do ícone. Em tons de cinza, tingida em runtime com a cor acima.\n" +
                 "O detalhe entalhado na placa é o que diferencia visualmente cada raridade.")]
        public Sprite iconPlate;

        [Tooltip("Peso base no sorteio (sem sorte)")]
        public float baseWeight = 60f;

        [Tooltip("Ganho/perda de peso por ponto de luckPercent (0-100).\n" +
                 "Negativo = fica menos frequente com sorte.\n" +
                 "Positivo = fica mais frequente com sorte.")]
        public float weightPerLuck = -0.30f;
    }
}
