using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Systems.UITheme
{
    [CreateAssetMenu(fileName = "UIThemeConfig", menuName = "Config/UI Theme Config")]
    public class UIThemeConfig : ScriptableObject
    {
        private static UIThemeConfig _instance;
        public static UIThemeConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<UIThemeConfig>("UIThemeConfig");
                if (_instance == null)
                    Debug.LogError("[UIThemeConfig] Asset não encontrado em Resources/UIThemeConfig.");
                return _instance;
            }
        }

        [Header("Superfícies")]
        [Tooltip("Navy Deep (#0A1E33) — fundo padrão de painel e card.")]
        public Color panelBackground = new Color32(0x0A, 0x1E, 0x33, 0xFF);

        [Tooltip("Navy Raised (#112942) — faixas de título e cabeçalhos de grupo, um degrau acima do fundo.")]
        public Color panelSurface = new Color32(0x11, 0x29, 0x42, 0xFF);

        [Tooltip("Navy Recess (#050F1C) — encaixes afundados, como a base dos slots de inventário.")]
        public Color panelRecess = new Color32(0x05, 0x0F, 0x1C, 0xFF);

        [Tooltip("Bandeja do slot de inventário (#0C2238) — um degrau acima do fundo do painel, o suficiente para o slot ler como peça sem roubar atenção do ícone.")]
        public Color slotTray = new Color32(0x0C, 0x22, 0x38, 0xFF);

        [Header("Acentos")]
        [Tooltip("Copper (#BC621B) — acento vivo: réguas, bullets, bordas de destaque.")]
        public Color panelBorder = new Color32(0xBC, 0x62, 0x1B, 0xFF);

        [Tooltip("Brown Deep (#663300) — versão escura do acento: sombras e contornos de moldura.")]
        public Color panelBorderDark = new Color32(0x66, 0x33, 0x00, 0xFF);

        [Header("Ações")]
        [Tooltip("Ação principal (#BC621B) — o botão que o jogador mais usa, como Atualizar.")]
        public Color actionPrimary = new Color32(0xBC, 0x62, 0x1B, 0xFF);

        [Tooltip("Ação neutra (#5D839B) — sair sem consequência, como Pular.")]
        public Color actionNeutral = new Color32(0x5D, 0x83, 0x9B, 0xFF);

        [Tooltip("Ação destrutiva (#A8392A) — única cor fora da paleta base, reservada a Exilar.")]
        public Color actionDestructive = new Color32(0xA8, 0x39, 0x2A, 0xFF);

        [Header("Texto")]
        [Tooltip("Cream (#FCF8E6) — títulos e valores em destaque.")]
        public Color textTitle = new Color32(0xFC, 0xF8, 0xE6, 0xFF);

        [Tooltip("Steel Light (#ACBDC0) — corpo e descrições.")]
        public Color textBody = new Color32(0xAC, 0xBD, 0xC0, 0xFF);

        [Tooltip("Steel (#5D839B) — texto secundário, rótulos de apoio.")]
        public Color textMuted = new Color32(0x5D, 0x83, 0x9B, 0xFF);

        [Header("Profundidade")]
        [Tooltip("Sombra projetada de peças de UI (preto 55%) — o deslocamento fica no componente, aqui só a cor.")]
        public Color dropShadow = new Color32(0x00, 0x00, 0x00, 0x8C);

        [Tooltip("Contorno escuro (#0A0805) de ícones brancos e de números sobre fundo colorido.")]
        public Color outlineDark = new Color32(0x0A, 0x08, 0x05, 0xD9);

        [Tooltip("Sombra de texto (#150A03) — dá a leitura de entalhe em títulos, níveis e rótulos de botão.")]
        public Color textShadow = new Color32(0x15, 0x0A, 0x03, 0xCC);

        [Header("Madeira (amostrada de ImphenziaPalette02-Albedo)")]
        [Tooltip("Wood Light (#DEAD8B) — destaque no topo da face de peças de madeira (placas, postes).")]
        public Color woodLight = new Color32(0xDE, 0xAD, 0x8B, 0xFF);

        [Tooltip("Wood Face (#C69F7A) — cor principal da face de madeira.")]
        public Color woodFace = new Color32(0xC6, 0x9F, 0x7A, 0xFF);

        [Tooltip("Wood Mid (#A0754B) — lábio/lateral, dá a leitura de espessura da peça.")]
        public Color woodMid = new Color32(0xA0, 0x75, 0x4B, 0xFF);

        [Tooltip("Wood Dark (#724E21) — sombra funda da madeira.")]
        public Color woodDark = new Color32(0x72, 0x4E, 0x21, 0xFF);

        [Tooltip("Wood Outline (#2A190B) — contorno quase preto-marrom de toda peça de madeira.")]
        public Color woodOutline = new Color32(0x2A, 0x19, 0x0B, 0xFF);

        [Header("Fundo de tela modal")]
        [Tooltip("Escurecimento do jogo atrás de uma tela modal.")]
        public Color screenDim = new Color32(0x04, 0x10, 0x1F, 0xB1);

        [Tooltip("Escurecimento durante o modo de exílio, avisando que o próximo clique é destrutivo.")]
        public Color screenDimExile = new Color32(0x99, 0x00, 0x00, 0xD9);

        [Header("Fontes")]
        [Tooltip("Lilita One — títulos importantes: nome de painel, nome de carta, rótulo de botão.")]
        public TMP_FontAsset titleFont;

        [Tooltip("Fredoka — fonte oficial do jogo. Padrão para tudo que não é título importante nem estatística.")]
        public TMP_FontAsset bodyFont;

        [Tooltip("Nunito — números e rótulos de estatística de itens, armas e do painel de status.")]
        public TMP_FontAsset statsFont;

        [Header("Forma")]
        [Tooltip("Raio de canto usado nos sprites 9-slice de painel/card/botão. Documentativo — o valor real vem do sprite importado.")]
        public float cornerRadiusReference = 4f;

        public void ApplyTitle(TMP_Text text) => Apply(text, titleFont, textTitle);

        public void ApplyBody(TMP_Text text) => Apply(text, bodyFont, textBody);

        public void ApplyBodyHighlight(TMP_Text text) => Apply(text, bodyFont, textTitle);

        public void ApplyMuted(TMP_Text text) => Apply(text, bodyFont, textMuted);

        public void ApplyStatLabel(TMP_Text text) => Apply(text, statsFont, textBody);

        public void ApplyStatValue(TMP_Text text) => Apply(text, statsFont, textTitle);

        public void ApplyIconOutline(Shadow effect) => ApplyEffect(effect, outlineDark);

        public void ApplyTextShadow(Shadow effect) => ApplyEffect(effect, textShadow);

        public void ApplyPrimaryAction(Button button) => ApplyFill(button, actionPrimary);

        public void ApplyNeutralAction(Button button) => ApplyFill(button, actionNeutral);

        public void ApplyDestructiveAction(Button button) => ApplyFill(button, actionDestructive);

        static void ApplyEffect(Shadow effect, Color color)
        {
            if (effect == null) return;
            effect.effectColor = color;
        }

        static void ApplyFill(Button button, Color color)
        {
            if (button == null || button.image == null) return;
            button.image.color = color;
        }

        void Apply(TMP_Text text, TMP_FontAsset font, Color color)
        {
            if (text == null) return;
            text.color = color;
            if (font != null) text.font = font;
        }
    }
}
