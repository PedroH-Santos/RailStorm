using UnityEngine;

public class SelectionBracketsAnimator : MonoBehaviour
{
    [Tooltip("Quantos pixels cada cantoneira avança na direção do centro do card.")]
    public float amplitude = 8f;

    [Tooltip("Duração de um ciclo completo (entra e volta), em segundos.")]
    public float cycleDuration = 1.1f;

    RectTransform[] _brackets;
    Vector2[] _restPositions;
    Vector2[] _inwardDirections;
    float _time;

    void Awake() => Cache();

    void OnEnable()
    {
        _time = 0f;
        ResetPositions();
    }

    void OnDisable() => ResetPositions();

    void Update()
    {
        if (_brackets == null || cycleDuration <= 0f) return;

        _time += Time.unscaledDeltaTime;

        float phase = (1f - Mathf.Cos(_time / cycleDuration * Mathf.PI * 2f)) * 0.5f;

        for (int i = 0; i < _brackets.Length; i++)
        {
            if (_brackets[i] == null) continue;
            _brackets[i].anchoredPosition = _restPositions[i] + _inwardDirections[i] * (amplitude * phase);
        }
    }

    void Cache()
    {
        if (_brackets != null) return;

        _brackets = new RectTransform[transform.childCount];
        _restPositions = new Vector2[transform.childCount];
        _inwardDirections = new Vector2[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            var rect = transform.GetChild(i) as RectTransform;
            if (rect == null) continue;

            _brackets[i] = rect;
            _restPositions[i] = rect.anchoredPosition;

            Vector2 anchor = rect.anchorMin;
            _inwardDirections[i] = new Vector2(anchor.x < 0.5f ? 1f : -1f, anchor.y < 0.5f ? 1f : -1f).normalized;
        }
    }

    void ResetPositions()
    {
        Cache();
        if (_brackets == null) return;

        for (int i = 0; i < _brackets.Length; i++)
            if (_brackets[i] != null) _brackets[i].anchoredPosition = _restPositions[i];
    }
}
