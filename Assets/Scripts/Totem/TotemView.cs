using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotemView : MonoBehaviour
{

    [Header("Partículas")]
    [SerializeField] private ParticleSystem idleParticles;
    [SerializeField] private ParticleSystem selectParticles;
    [SerializeField] private ParticleSystem unlockBurstParticles;

    [Header("UI (World Space Canvas)")]
    [SerializeField] private Canvas totemCanvas;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image costIcon;
    [SerializeField] private Image iconHeader;
    [SerializeField] private Image panelFrameImage;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private Button unlockButton;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock _mpb;

    Color _baseEmissionColor = Color.white;
    float _baseLightIntensity;
    Vector3 _baseScale;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _baseScale = transform.localScale;

        // Partículas precisam continuar animando mesmo com Time.timeScale = 0
        ForceUnscaledTime(idleParticles);
        ForceUnscaledTime(selectParticles);
        ForceUnscaledTime(unlockBurstParticles);

        if (panelRoot != null) panelRoot.SetActive(false);
        if (selectedFrame != null) selectedFrame.SetActive(false);
    }

    void ForceUnscaledTime(ParticleSystem ps)
    {
        if (ps == null) return;
        var main = ps.main;
        main.useUnscaledTime = true;
    }

    public void SetThemeColor(Color color)
    {
        _baseEmissionColor = color;
        ApplyEmission(color, 1f);
        if (panelFrameImage != null) panelFrameImage.color = color;
    }

    public void SetCanvasSortOrder(int order)
    {
        if (totemCanvas != null) totemCanvas.sortingOrder = order;
    }

    public void Bind(SplineEntry entry, bool affordable, System.Action onUnlockClicked)
    {
        if (titleText != null) titleText.text = entry.destinationName;
        if (descriptionText != null) descriptionText.text = entry.description;

        if (costText != null)
        {
            costText.text = entry.unlockCost.ToString();
            costText.color = affordable ? Color.white : new Color(1f, 0.4f, 0.4f);
        }

        if (iconHeader != null && entry.themeIcon != null)
            iconHeader.sprite = entry.themeIcon;

        if (unlockButton != null)
        {
            Debug.Log("cLICK");
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(() => onUnlockClicked?.Invoke());
            unlockButton.interactable = affordable;
        }
    }

    public void Show()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        SetSelected(false);
    } 

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null) selectedFrame.SetActive(selected);

        if (selected)
        {
            if (selectParticles != null) selectParticles.Play();
            ApplyEmission(_baseEmissionColor, 2.5f);
        }
        else
        {
            if (selectParticles != null) selectParticles.Stop();
            ApplyEmission(_baseEmissionColor, 1f);
        }
    }

    public void PlayUnlockEffect(System.Action onComplete = null)
    {
        StartCoroutine(UnlockRoutine(onComplete));
    }

    IEnumerator UnlockRoutine(System.Action onComplete)
    {
        if (unlockBurstParticles != null) unlockBurstParticles.Play();

        float t = 0f;
        float duration = 0.5f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(t / duration * Mathf.PI) * 0.15f;
            transform.localScale = _baseScale * pulse;
            ApplyEmission(_baseEmissionColor, Mathf.Lerp(2.5f, 4f, t / duration));
            yield return null;
        }

        transform.localScale = _baseScale;
        onComplete?.Invoke();
    }

    public void PlayDeniedEffect()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        Vector3 basePos = transform.localPosition;
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            float offset = Mathf.Sin(t * 60f) * 0.05f;
            transform.localPosition = basePos + new Vector3(offset, 0f, 0f);
            yield return null;
        }
        transform.localPosition = basePos;
    }

    void ApplyEmission(Color color, float intensity)
    {
        //ApplyEmissionTo(emblemRenderer, color, intensity);
        //ApplyEmissionTo(crystalLeftRenderer, color, intensity);
        //ApplyEmissionTo(crystalRightRenderer, color, intensity);
    }

    void ApplyEmissionTo(Renderer r, Color color, float intensity)
    {
        if (r == null) return;
        r.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionColorId, color * intensity);
        r.SetPropertyBlock(_mpb);
    }
}