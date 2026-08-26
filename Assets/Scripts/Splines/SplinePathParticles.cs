using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

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
    [SerializeField] private float travelSpeed = 0.35f;
    [SerializeField] private int trailCount = 3;
    [SerializeField] private float trailSpacing = 0.28f;

    SplineContainer _container;
    int _splineIndex = -1;
    bool _reversed;
    float _cursor;

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
        main.startLifetime = 0.85f;
        main.startSize = 0.35f;
        main.startSpeed = 0f;
        main.gravityModifier = -0.05f;
        main.loop = true;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = false;
    }

    public void SetPath(SplineContainer container, int splineIndex, bool reversed, Color accent)
    {
        if (particles == null || container == null) return;

        _container = container;
        _splineIndex = splineIndex;
        _reversed = reversed;
        _cursor = 0f;

        var main = particles.main;
        Color color = accent;
        color.r = Mathf.Min(color.r, 1f);
        color.g = Mathf.Min(color.g, 1f);
        color.b = Mathf.Min(color.b, 1f);
        main.startColor = color;

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

    void Update()
    {
        if (_container == null || _splineIndex < 0 || particles == null) return;

        Spline spline = _container.Splines[_splineIndex];
        if (spline == null) return;

        _cursor += travelSpeed * Time.unscaledDeltaTime;
        if (_cursor > 1f) _cursor -= 1f;

        for (int i = 0; i < trailCount; i++)
        {
            float t = _cursor - i * trailSpacing;
            t -= Mathf.Floor(t);

            float sampleT = _reversed ? 1f - t : t;
            EmitAt(spline, sampleT);
        }
    }

    void EmitAt(Spline spline, float t)
    {
        SplineUtility.Evaluate(spline, t, out float3 localPosition, out float3 localTangent, out _);

        Vector3 worldPosition = _container.transform.TransformPoint((Vector3)localPosition);
        Vector3 worldTangent = _container.transform.TransformDirection((Vector3)localTangent).normalized;
        if (_reversed) worldTangent = -worldTangent;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = worldPosition,
            velocity = worldTangent * 1.5f,
        };

        particles.Emit(emitParams, 1);
    }
}
