using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChestSparksEmitter : MonoBehaviour
{
    [SerializeField] private RectTransform sparksRoot;
    [SerializeField] private Image sparkTemplate;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private float rateSpinning = 18f;
    [SerializeField] private float rateIdle = 6f;
    [SerializeField, Range(0.5f, 1.5f)] private float riseFraction = 1.1f;
    [SerializeField] private Vector2 lifetimeRange = new Vector2(0.9f, 1.6f);
    [SerializeField] private float sideDrift = 40f;

    readonly List<Image> _pool = new();
    readonly HashSet<Image> _busy = new();
    Color _color = UnityEngine.Color.white;
    float _rate;
    float _accumulator;
    bool _active;

    void Awake()
    {
        if (sparkTemplate != null) sparkTemplate.gameObject.SetActive(false);

        for (int i = 0; i < poolSize; i++)
        {
            var spark = Instantiate(sparkTemplate, sparksRoot);
            spark.gameObject.SetActive(false);
            _pool.Add(spark);
        }
    }

    void Update()
    {
        if (!_active || sparksRoot == null) return;

        _accumulator += Time.unscaledDeltaTime * _rate;
        while (_accumulator >= 1f)
        {
            _accumulator -= 1f;
            Emit();
        }
    }

    public void SetColor(Color color) => _color = color;

    public void SetSpinning(bool spinning)
    {
        _rate = spinning ? rateSpinning : rateIdle;
    }

    public void Play()
    {
        _active = true;
        _accumulator = 0f;
        SetSpinning(true);
    }

    public void Burst(int count)
    {
        for (int i = 0; i < count; i++) Emit();
    }

    public void Stop()
    {
        _active = false;

        foreach (var spark in _pool)
        {
            spark.rectTransform.DOKill();
            spark.gameObject.SetActive(false);
        }

        _busy.Clear();
    }

    void Emit()
    {
        var spark = GetFree();
        if (spark == null) return;

        _busy.Add(spark);

        float width = sparksRoot.rect.width;
        float height = sparksRoot.rect.height;
        float rise = height * riseFraction;
        var rect = spark.rectTransform;
        rect.anchoredPosition = new Vector2(Random.Range(-width * 0.5f, width * 0.5f), -height * 0.5f - rect.rect.height);
        rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        spark.color = new Color(_color.r, _color.g, _color.b, 0f);
        spark.gameObject.SetActive(true);

        float lifetime = Random.Range(lifetimeRange.x, lifetimeRange.y);
        float drift = Random.Range(-sideDrift, sideDrift);
        float rotation = Random.Range(-180f, 180f);

        var sequence = DOTween.Sequence().SetUpdate(true).SetLink(spark.gameObject);
        sequence.Join(rect.DOAnchorPos(rect.anchoredPosition + new Vector2(drift, rise), lifetime).SetEase(Ease.OutCubic));
        sequence.Join(rect.DORotate(new Vector3(0f, 0f, rotation), lifetime, RotateMode.LocalAxisAdd));
        sequence.Join(spark.DOFade(1f, lifetime * 0.15f));
        sequence.Insert(lifetime * 0.7f, spark.DOFade(0f, lifetime * 0.3f));
        sequence.OnComplete(() =>
        {
            spark.gameObject.SetActive(false);
            _busy.Remove(spark);
        });
    }

    Image GetFree()
    {
        foreach (var spark in _pool)
            if (!_busy.Contains(spark))
                return spark;

        return null;
    }

    void OnDisable() => Stop();
}
