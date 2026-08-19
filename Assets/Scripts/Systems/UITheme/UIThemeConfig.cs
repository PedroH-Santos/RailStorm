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

        [Header("Paleta")]
        public Color panelBackground = new Color32(0x0A, 0x29, 0x47, 0xFF);
        public Color panelBorder = new Color32(0x8B, 0x5E, 0x3C, 0xFF);
        public Color textTitle = new Color32(0xF3, 0xE4, 0xC9, 0xFF);
        public Color textBody = new Color32(0xD3, 0xD4, 0xC0, 0xFF);

        [Header("Fontes")]
        public TMP_FontAsset titleFont;
        public TMP_FontAsset bodyFont;

        [Header("Forma")]
        [Tooltip("Raio de canto usado nos sprites 9-slice de painel/card/botão. Documentativo — o valor real vem do sprite importado.")]
        public float cornerRadiusReference = 4f;

        public void ApplyTitle(TMP_Text text)
        {
            if (text == null) return;
            text.color = textTitle;
            if (titleFont != null) text.font = titleFont;
        }

        public void ApplyBody(TMP_Text text)
        {
            if (text == null) return;
            text.color = textBody;
            if (bodyFont != null) text.font = bodyFont;
        }
    }
}
