using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestRevealEffect : MonoBehaviour
{
    static ChestRevealEffect _instance;

    public static ChestRevealEffect Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<ChestRevealEffect>(FindObjectsInactive.Include);
                if (found != null && !found.gameObject.activeSelf)
                    found.gameObject.SetActive(true);
            }

            return _instance;
        }
    }

    [Header("Fundo escurecido (atrás do painel)")]
    [SerializeField] private Image dimBackground;
    [SerializeField] private float dimTargetAlpha = 0.65f;
    [SerializeField] private float dimFadeDuration = 0.15f;

    [Header("Painel (não cobre a tela toda)")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image panelFrame;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image itemIcon;

    [Header("Botões de decisão")]
    [SerializeField] private Button btnTake;
    [SerializeField] private Button btnExile;
    [SerializeField] private Button btnSkip;

    [Header("Burst de raios atrás do ícone")]
    [SerializeField] private Image raysImage;
    [SerializeField] private float raysSpinSpeed = 20f;

    [Header("Timings")]
    [SerializeField] private float panelInDuration = 0.25f;
    [SerializeField] private float panelOutDuration = 0.2f;

    [Header("Partículas (opcional, na posição do baú no mundo)")]
    [SerializeField] private ParticleSystem burstParticlesPrefab;

    Coroutine _spinRoutine;
    Action _onTake, _onExile, _onSkip;
    bool _waitingForDecision;

    void Awake()
    {
        _instance = this;

        if (dimBackground != null)
        {
            var c = dimBackground.color;
            c.a = 0f;
            dimBackground.color = c;
            dimBackground.gameObject.SetActive(false);
        }

        if (panelRoot != null) panelRoot.SetActive(false);

        if (btnTake != null) btnTake.onClick.AddListener(() => Decide(_onTake));
        if (btnExile != null) btnExile.onClick.AddListener(() => Decide(_onExile));
        if (btnSkip != null) btnSkip.onClick.AddListener(() => Decide(_onSkip));
    }

    /// <summary>
    /// Mostra o painel de revelação e aguarda o player escolher Pegar (Take) / Exilar (Exile) / Pular (Skip).
    /// Cada callback é invocado quando o botão correspondente é clicado, e então o painel fecha.
    /// </summary>
    public void Show(ItemDefinition item, int rarityIndex, Vector3 worldPos, Action onTake, Action onExile, Action onSkip)
    {
        _onTake = onTake;
        _onExile = onExile;
        _onSkip = onSkip;

        if (burstParticlesPrefab != null)
        {
            var ps = Instantiate(burstParticlesPrefab, worldPos, Quaternion.identity);
            var main = ps.main;
            main.useUnscaledTime = true;
            main.startColor = RarityHelper.Color(rarityIndex);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        SetupPanelContent(item, rarityIndex);
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        _waitingForDecision = true;

        yield return StartCoroutine(FadeDim(dimTargetAlpha, dimFadeDuration));
        yield return StartCoroutine(PanelIn());

        if (_spinRoutine != null) StopCoroutine(_spinRoutine);
        _spinRoutine = StartCoroutine(SpinRays());
    }

    IEnumerator SpinRays()
    {
        while (_waitingForDecision)
        {
            if (raysImage != null)
                raysImage.transform.Rotate(0f, 0f, raysSpinSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    void Decide(Action callback)
    {
        if (!_waitingForDecision) return;
        _waitingForDecision = false;

        callback?.Invoke();
        StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        yield return StartCoroutine(PanelOut());
        yield return StartCoroutine(FadeDim(0f, dimFadeDuration));

        if (dimBackground != null) dimBackground.gameObject.SetActive(false);
    }

    void SetupPanelContent(ItemDefinition item, int rarityIndex)
    {
        Color rarityColor = RarityHelper.Color(rarityIndex);

        if (rarityText != null)
        {
            rarityText.text = RarityHelper.DisplayName(rarityIndex);
            rarityText.color = rarityColor;
        }

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (panelFrame != null) panelFrame.color = rarityColor;
        if (raysImage != null) raysImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.5f);
    }

    IEnumerator FadeDim(float target, float duration)
    {
        if (dimBackground == null) yield break;

        dimBackground.gameObject.SetActive(true);
        float start = dimBackground.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, target, t / duration);
            var c = dimBackground.color;
            dimBackground.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        var final = dimBackground.color;
        dimBackground.color = new Color(final.r, final.g, final.b, target);
    }

    IEnumerator PanelIn()
    {
        if (panelRoot == null) yield break;

        panelRoot.SetActive(true);
        Vector3 baseScale = Vector3.one;

        float t = 0f;
        while (t < panelInDuration)
        {
            t += Time.unscaledDeltaTime;
            float pulse = Mathf.Sin(Mathf.Clamp01(t / panelInDuration) * Mathf.PI * 0.5f) * 1.08f;
            panelRoot.transform.localScale = baseScale * Mathf.Max(pulse, 0.01f);
            yield return null;
        }

        panelRoot.transform.localScale = baseScale;
    }

    IEnumerator PanelOut()
    {
        if (panelRoot == null) yield break;

        Vector3 baseScale = panelRoot.transform.localScale;
        float t = 0f;
        while (t < panelOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1f, 0f, t / panelOutDuration);
            panelRoot.transform.localScale = baseScale * scale;
            yield return null;
        }

        panelRoot.SetActive(false);
        panelRoot.transform.localScale = baseScale;
    }
}
