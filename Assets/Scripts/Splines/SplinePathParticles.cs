using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class SplinePathParticles : MonoBehaviour
{
    static SplinePathParticles _instance;
    public static SplinePathParticles Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<SplinePathParticles>(FindObjectsInactive.Include);
                if (found != null)
                {
                    found.gameObject.SetActive(true);
                    _instance = found;
                }
            }

            return _instance;
        }
    }

    [SerializeField] private ParticleSystem particles;

    [Header("Trilha")]
    [SerializeField] private float travelSpeed = 0.35f;
    [SerializeField] private int trailCount = 3;
    [SerializeField] private float trailSpacing = 0.28f;
    [SerializeField] private float puffsPerSecond = 14f;

    [Header("Baforada")]
    [SerializeField] private Vector2 puffSizeRange = new Vector2(0.85f, 1.35f);
    [SerializeField] private Vector2 puffLifetimeRange = new Vector2(0.9f, 1.4f);
    [SerializeField] private float sideScatter = 0.3f;
    [SerializeField] private float riseSpeed = 0.55f;
    [SerializeField] private float driftSpeed = 0.9f;
    [SerializeField] private float whiteMix = 0.4f;

    SplineContainer _container;
    int _splineIndex = -1;
    bool _reversed;
    float _cursor;
    float _emitTimer;
    Color _smokeColor = Color.white;

    void Awake()
    {
        _instance = this;
        ConfigureSystem();
    }

    void ConfigureSystem()
    {
        if (particles == null) return;

        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        main.startLifetime = puffLifetimeRange.y;
        main.startSize = puffSizeRange.y;
        main.startSpeed = 0f;
        main.gravityModifier = 0f;
        main.loop = true;
        main.maxParticles = 400;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = false;

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.45f),
            new Keyframe(0.22f, 1f),
            new Keyframe(1f, 1.35f)));

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.14f),
                new GradientAlphaKey(0.85f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var textureSheet = particles.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Grid;
        textureSheet.numTilesX = 2;
        textureSheet.numTilesY = 2;
        textureSheet.animation = ParticleSystemAnimationType.WholeSheet;
        textureSheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        textureSheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 0.999f);
        textureSheet.cycleCount = 1;
    }

    public void SetPath(SplineContainer container, int splineIndex, bool reversed, Color accent)
    {
        if (particles == null || container == null) return;

        _container = container;
        _splineIndex = splineIndex;
        _reversed = reversed;
        _cursor = 0f;
        _emitTimer = 0f;
        _smokeColor = SmokeColorFrom(accent);

        particles.Clear();
        particles.Play();
    }

    public void StopPath()
    {
        _splineIndex = -1;
        _container = null;

        if (particles == null) return;

        particles.Clear();
        particles.Stop();
    }

    Color SmokeColorFrom(Color accent)
    {
        accent.r = Mathf.Min(accent.r, 1f);
        accent.g = Mathf.Min(accent.g, 1f);
        accent.b = Mathf.Min(accent.b, 1f);
        accent.a = 1f;

        return Color.Lerp(accent, Color.white, Mathf.Clamp01(whiteMix));
    }

    void Update()
    {
        if (_container == null || _splineIndex < 0 || particles == null) return;

        Spline spline = _container.Splines[_splineIndex];
        if (spline == null) return;

        float delta = Time.unscaledDeltaTime;

        _cursor += travelSpeed * delta;
        if (_cursor > 1f) _cursor -= 1f;

        float interval = 1f / Mathf.Max(1f, puffsPerSecond);
        _emitTimer += delta;

        while (_emitTimer >= interval)
        {
            _emitTimer -= interval;

            for (int i = 0; i < trailCount; i++)
            {
                float t = _cursor - i * trailSpacing;
                t -= Mathf.Floor(t);

                float sampleT = _reversed ? 1f - t : t;
                EmitAt(spline, sampleT);
            }
        }
    }

    void EmitAt(Spline spline, float t)
    {
        SplineUtility.Evaluate(spline, t, out float3 localPosition, out float3 localTangent, out _);

        Vector3 worldPosition = _container.transform.TransformPoint((Vector3)localPosition);
        Vector3 worldTangent = _container.transform.TransformDirection((Vector3)localTangent).normalized;
        if (_reversed) worldTangent = -worldTangent;

        Vector3 side = Vector3.Cross(Vector3.up, worldTangent).normalized;
        float scatter = Random.Range(-sideScatter, sideScatter);

        Color color = _smokeColor * Random.Range(0.9f, 1f);
        color.a = 1f;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = worldPosition + side * scatter + Vector3.up * Random.Range(0.05f, 0.25f),
            velocity = worldTangent * driftSpeed + Vector3.up * riseSpeed * Random.Range(0.7f, 1.3f) + side * scatter,
            startSize = Random.Range(puffSizeRange.x, puffSizeRange.y),
            startLifetime = Random.Range(puffLifetimeRange.x, puffLifetimeRange.y),
            rotation = Random.Range(0f, 360f),
            startColor = color,
        };

        particles.Emit(emitParams, 1);
    }
}
