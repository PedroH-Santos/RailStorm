using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotHUD : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownOverlay;
    public TMP_Text cooldownText;
    public TMP_Text keyText;
    public Image readyFlash;
    public CanvasGroup group;

    [Range(0f, 1f)]
    [Tooltip("Opacidade do slot sem skill equipada.")]
    public float emptyAlpha = 0.45f;

    [Tooltip("Recargas menores que isso não mostram o tempo restante dentro do slot.")]
    public float minCooldownForNumber = 0f;

    [Tooltip("Recargas menores que isso não pulam nem piscam ao ficar prontas, para disparo contínuo não piscar a cada tiro.")]
    public float minCooldownForReadyFlash = 1f;

    [Range(0f, 1f)]
    [Tooltip("Opacidade do ícone enquanto o tempo da recarga aparece por cima dele.")]
    public float coolingIconAlpha = 0.3f;

    [Header("Animação")]
    public float castPunch = 0.12f;
    public float readyPunch = 0.22f;

    SkillDefinition _skill;
    bool _cooling;
    float _lastDuration;
    string _key;
    int _shownTenths = int.MinValue;

    public void Bind(SkillDefinition skill)
    {
        _skill = skill;
        _cooling = false;
        _shownTenths = int.MinValue;

        if (iconImage != null)
        {
            iconImage.enabled = skill != null && skill.icon != null;
            if (skill != null) iconImage.sprite = skill.icon;
        }

        if (group != null) group.alpha = skill != null ? 1f : emptyAlpha;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        if (readyFlash != null) readyFlash.color = WithAlpha(readyFlash.color, 0f);
    }

    public void SetKey(string key)
    {
        if (key == _key || keyText == null) return;
        _key = key;
        keyText.text = key;
    }

    public void Tick(bool hasCooldown, float remaining, float normalized, float duration)
    {
        if (_skill == null || !hasCooldown)
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
            if (cooldownText != null && cooldownText.gameObject.activeSelf) cooldownText.gameObject.SetActive(false);
            SetIconAlpha(1f);
            _cooling = false;
            return;
        }

        bool cooling = remaining > 0f;
        if (cooling) _lastDuration = duration;

        if (cooldownOverlay != null) cooldownOverlay.fillAmount = cooling ? normalized : 0f;

        bool showNumber = cooling && _lastDuration >= minCooldownForNumber;
        SetIconAlpha(showNumber ? coolingIconAlpha : 1f);
        if (cooldownText != null)
        {
            if (cooldownText.gameObject.activeSelf != showNumber) cooldownText.gameObject.SetActive(showNumber);
            if (showNumber)
            {
                int tenths = Mathf.CeilToInt(remaining * 10f);
                if (tenths != _shownTenths)
                {
                    _shownTenths = tenths;
                    cooldownText.text = remaining < 1f ? (tenths / 10f).ToString("0.0") : Mathf.CeilToInt(remaining).ToString();
                }
            }
        }

        if (_cooling && !cooling && _lastDuration >= minCooldownForReadyFlash) PlayReady();
        _cooling = cooling;
    }

    public void PlayCast()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * -castPunch, 0.2f, 6, 0.5f).AsUI(gameObject);
    }

    void PlayReady()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * readyPunch, 0.3f, 6, 0.5f).AsUI(gameObject);

        if (readyFlash == null) return;
        readyFlash.DOKill();
        readyFlash.color = WithAlpha(readyFlash.color, 0.85f);
        readyFlash.DOFade(0f, 0.35f).AsUI(readyFlash.gameObject);
    }

    void SetIconAlpha(float alpha)
    {
        if (iconImage == null || Mathf.Approximately(iconImage.color.a, alpha)) return;
        iconImage.color = WithAlpha(iconImage.color, alpha);
    }

    static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}
