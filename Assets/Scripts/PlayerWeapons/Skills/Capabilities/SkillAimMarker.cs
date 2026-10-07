using DG.Tweening;
using UnityEngine;

public class SkillAimMarker : MonoBehaviour
{
    [Tooltip("Objeto escalado pelo raio da área. A escala 1 deve cobrir raio 1.")]
    [SerializeField] private Transform areaVisual;
    [SerializeField] private float groundOffset = 0.05f;

    [Header("Animação")]
    [SerializeField] private float showDuration = 0.15f;
    [SerializeField] private float reachedMaxPunch = 0.15f;

    float _radius = 1f;
    bool _reachedMax; 

    Transform Visual => areaVisual != null ? areaVisual : transform;

    public void Show(float radius)
    {
        _radius = Mathf.Max(0.01f, radius);
        _reachedMax = false;
        gameObject.SetActive(true);

        Visual.DOKill();
        Visual.localScale = Vector3.zero;
        Visual.DOScale(Vector3.one * _radius, showDuration).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    public void MoveTo(Vector3 groundPoint)
    {
        transform.position = groundPoint + Vector3.up * groundOffset;
    }

    public void SetReachedMax(bool reachedMax)
    {
        if (reachedMax == _reachedMax) return;
        _reachedMax = reachedMax;
        if (!reachedMax) return;

        Visual.DOKill(true);
        Visual.localScale = Vector3.one * _radius;
        Visual.DOPunchScale(Vector3.one * (_radius * reachedMaxPunch), 0.25f, 6, 0.5f).SetLink(gameObject);
    }

    public void Hide()
    {
        Visual.DOKill();
        gameObject.SetActive(false);
    }
}
