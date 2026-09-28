using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    [SerializeField] private Sprite icon;
    [SerializeField] private Color discColor = Color.white;
    [SerializeField] private float size = 26f;
    [SerializeField] private bool clampToEdge = true;
    [SerializeField] private Vector3 worldOffset;

    [Header("Cor do caminho (totens)")]
    [SerializeField] private bool useSplineThemeColor;
    [SerializeField] private int splineIndex = -1;

    public Sprite Icon => icon;
    public float Size => size;
    public bool ClampToEdge => clampToEdge;
    public Vector3 WorldPosition => transform.position + worldOffset;

    public Color ResolveDiscColor()
    {
        if (!useSplineThemeColor) return discColor;

        var state = SplineRuntimeState.Instance;
        if (state == null || state.manifest == null) return discColor;

        var entry = state.manifest.GetEntry(splineIndex);
        if (entry == null || entry.themeColor == Color.white) return discColor;

        return entry.themeColor;
    }

    void OnEnable() => MinimapMarkerRegistry.Register(this);

    void OnDisable() => MinimapMarkerRegistry.Unregister(this);
}
