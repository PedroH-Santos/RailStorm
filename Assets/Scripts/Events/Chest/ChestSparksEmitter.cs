using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChestSparksEmitter : MonoBehaviour
{
    [SerializeField] private Image sparkTemplate;
    [SerializeField] private int poolSize = 48;
    [Tooltip("Distância do centro onde as estrelas nascem. Deve coincidir com o anel.")]
    [SerializeField] private float radius = 170f;
    [SerializeField] private float spinningRate = 22f;
    [SerializeField] private float idleRate = 8f;

    readonly List<Image> _all = new();
    readonly Queue<Image> _free = new();
    float _rate;
    float _timer;
    float _angle;

    public Color Color { get; set; } = Color.white;

    void Awake()
    {
        sparkTemplate.gameObject.SetActive(false);
        for (int i = 0; i < poolSize; i++)
            _all.Add(Instantiate(sparkTemplate, transform));
    }

    void OnEnable()
    {
        _free.Clear();
        foreach (var spark in _all)
        {
            spark.gameObject.SetActive(false);
            _free.Enqueue(spark);
        }

        _rate = idleRate;
    }

    void Update()
    {
        _timer += Time.unscaledDeltaTime * _rate;
        while (_timer >= 1f)
        {
            _timer -= 1f;
            _angle += 46.6f;
            Launch(_angle, radius, 130f, Vector2.up * 60f, Random.Range(0.7f, 1.1f), 1f);
        }
    }

    public void SetSpinning(bool spinning) => _rate = spinning ? spinningRate : idleRate;

    public void Burst(int count)
    {
        float offset = Random.Range(0f, 360f);
        for (int i = 0; i < count; i++)
            Launch(offset + i * 360f / count, radius * 0.5f, 300f, Vector2.zero, 0.65f, 1.3f);
    }

    void Launch(float angle, float startDistance, float travel, Vector2 drift, float lifetime, float size)
    {
        if (_free.Count == 0) return;

        var spark = _free.Dequeue();
        var rect = spark.rectTransform;
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        rect.anchoredPosition = direction * startDistance;
        rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 90f));
        rect.localScale = Vector3.zero;
        spark.color = Color;
        spark.gameObject.SetActive(true);

        float scale = Random.Range(0.7f, 1.2f) * size;
        DOTween.Sequence()
            .Join(rect.DOAnchorPos(direction * (startDistance + travel) + drift, lifetime).SetEase(Ease.OutCubic))
            .Join(rect.DOScale(scale, lifetime * 0.25f).SetEase(Ease.OutBack))
            .Insert(lifetime * 0.55f, rect.DOScale(0f, lifetime * 0.45f))
            .AsUI(spark.gameObject)
            .OnComplete(() =>
            {
                spark.gameObject.SetActive(false);
                _free.Enqueue(spark);
            });
    }
}
