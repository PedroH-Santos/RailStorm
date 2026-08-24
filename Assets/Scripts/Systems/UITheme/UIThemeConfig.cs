using TMPro;
using UnityEngine;

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

        [Header("Acentos")]
        [Tooltip("Copper (#BC621B) — acento vivo: réguas, bullets, bordas de destaque.")]
        public Color panelBorder = new Color32(0xBC, 0x62, 0x1B, 0xFF);

        [Tooltip("Brown Deep (#663300) — versão escura do acento: sombras e contornos de moldura.")]
        public Color panelBorderDark = new Color32(0x66, 0x33, 0x00, 0xFF);

        [Header("Texto")]
        [Tooltip("Cream (#FCF8E6) — títulos e valores em destaque.")]
        public Color textTitle = new Color32(0xFC, 0xF8, 0xE6, 0xFF);

        [Tooltip("Steel Light (#ACBDC0) — corpo e descrições.")]
        public Color textBody = new Color32(0xAC, 0xBD, 0xC0, 0xFF);

        [Tooltip("Steel (#5D839B) — texto secundário, rótulos de apoio.")]
        public Color textMuted = new Color32(0x5D, 0x83, 0x9B, 0xFF);

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

        void Apply(TMP_Text text, TMP_FontAsset font, Color color)
        {
            if (text == null) return;
            text.color = color;
            if (font != null) text.font = font;
        }
    }
}
