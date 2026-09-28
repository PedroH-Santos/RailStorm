using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Splines;

public class MinimapUI : MonoBehaviour
{
    [Header("Fontes")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform player;

    [Header("Peças")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private MinimapTrackGraphic trackTemplate;
    [SerializeField] private RectTransform markersRoot;
    [SerializeField] private MinimapIconUI iconTemplate;
    [SerializeField] private RectTransform playerPointerPivot;

    [Header("Escala")]
    [SerializeField] private float pixelsPerUnit = 2.5f;
    [SerializeField] private float sampleStep = 1f;

    [Header("Trilhos")]
    [SerializeField] private Color unlockedColor = new(0.675f, 0.741f, 0.753f, 0.6f);
    [SerializeField] private float unlockedWidth = 5f;
    [SerializeField] private Color blockedFallbackColor = new(0.365f, 0.514f, 0.608f, 1f);
    [SerializeField] private float blockedWidth = 4f;
    [Range(0f, 1f)][SerializeField] private float blockedDarken = 0.35f;
    [Range(0f, 1f)][SerializeField] private float blockedAlpha = 0.45f;

    [Header("Borda")]
    [SerializeField] private float edgeMargin = 16f;
    [SerializeField] private float edgeScale = 0.8f;
    [Range(0f, 1f)][SerializeField] private float edgeAlpha = 0.75f;

    readonly List<MinimapTrackGraphic> _tracks = new();
    readonly Dictionary<MinimapMarker, MinimapIconUI> _icons = new();
    readonly Stack<MinimapIconUI> _freeIcons = new();
    readonly List<Vector2> _samples = new();

    Vector3 _axisRight = Vector3.right;
    Vector3 _axisUp = Vector3.forward;
    bool _axesResolved;

    void Awake()
    {
        if (splineContainer == null) splineContainer = FindFirstObjectByType<SplineContainer>();
        if (player == null)
        {
            var controller = FindFirstObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
        }

        if (trackTemplate != null) trackTemplate.gameObject.SetActive(false);
        if (iconTemplate != null) iconTemplate.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        ResolveAxes();
        BuildTracks();

        SplineRuntimeState.OnSplineUnblocked += HandleSplineUnblocked;
        MinimapMarkerRegistry.OnMarkerAdded += AddIcon;
        MinimapMarkerRegistry.OnMarkerRemoved += RemoveIcon;

        foreach (var marker in MinimapMarkerRegistry.All)
            AddIcon(marker);
    }

    void OnDisable()
    {
        SplineRuntimeState.OnSplineUnblocked -= HandleSplineUnblocked;
        MinimapMarkerRegistry.OnMarkerAdded -= AddIcon;
        MinimapMarkerRegistry.OnMarkerRemoved -= RemoveIcon;

        foreach (var icon in _icons.Values)
            ReleaseIcon(icon);
        _icons.Clear();
    }

    void LateUpdate()
    {
        if (player == null || viewport == null) return;
        if (!_axesResolved) ResolveAxes();

        Vector2 center = ToMap(player.position);
        if (content != null) content.anchoredPosition = -center;

        float radius = Mathf.Min(viewport.rect.width, viewport.rect.height) * 0.5f;
        float limit = Mathf.Max(0f, radius - edgeMargin);

        foreach (var pair in _icons)
            PlaceIcon(pair.Key, pair.Value, center, limit);

        RotatePointer();
    }

    void PlaceIcon(MinimapMarker marker, MinimapIconUI icon, Vector2 center, float limit)
    {
        if (marker == null) return;

        Vector2 offset = ToMap(marker.WorldPosition) - center;
        bool outside = offset.sqrMagnitude > limit * limit;

        if (outside && !marker.ClampToEdge)
        {
            icon.SetVisible(false);
            return;
        }

        icon.SetVisible(true);
        if (outside) icon.Place(offset.normalized * limit, edgeScale, edgeAlpha);
        else icon.Place(offset, 1f, 1f);
    }

    void RotatePointer()
    {
        if (playerPointerPivot == null) return;

        Vector3 forward = player.forward;
        Vector2 direction = new(Vector3.Dot(forward, _axisRight), Vector3.Dot(forward, _axisUp));
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        playerPointerPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ResolveAxes()
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 right = cam.transform.right;
        right.y = 0f;

        Vector3 up = cam.transform.forward;
        up.y = 0f;
        if (up.sqrMagnitude < 0.0001f)
        {
            up = cam.transform.up;
            up.y = 0f;
        }

        if (right.sqrMagnitude < 0.0001f || up.sqrMagnitude < 0.0001f) return;

        _axisRight = right.normalized;
        _axisUp = up.normalized;
        _axesResolved = true;
    }

    Vector2 ToMap(Vector3 world) =>
        new Vector2(Vector3.Dot(world, _axisRight), Vector3.Dot(world, _axisUp)) * pixelsPerUnit;

    void BuildTracks()
    {
        if (splineContainer == null || trackTemplate == null || content == null) return;

        foreach (var track in _tracks)
            if (track != null) Destroy(track.gameObject);
        _tracks.Clear();

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            var track = Instantiate(trackTemplate, content);
            track.name = $"Track{i}";
            track.gameObject.SetActive(true);
            track.SetPoints(SampleSpline(i));
            _tracks.Add(track);
        }

        RefreshTrackStyles();
    }

    List<Vector2> SampleSpline(int splineIndex)
    {
        _samples.Clear();

        float length = splineContainer.CalculateLength(splineIndex);
        int count = Mathf.Max(2, Mathf.CeilToInt(length / Mathf.Max(0.1f, sampleStep)) + 1);

        for (int s = 0; s < count; s++)
        {
            float t = s / (float)(count - 1);
            Vector3 world = splineContainer.EvaluatePosition(splineIndex, t);
            _samples.Add(ToMap(world));
        }

        return _samples;
    }

    void RefreshTrackStyles()
    {
        var state = SplineRuntimeState.Instance;

        for (int i = 0; i < _tracks.Count; i++)
        {
            bool blocked = state != null && state.IsBlocked(i);
            var track = _tracks[i];

            if (blocked)
            {
                track.SetStyle(BlockedColor(state, i), blockedWidth, true);
                track.transform.SetAsFirstSibling();
            }
            else
            {
                track.SetStyle(unlockedColor, unlockedWidth, false);
                track.transform.SetAsLastSibling();
            }
        }
    }

    Color BlockedColor(SplineRuntimeState state, int splineIndex)
    {
        var entry = state.manifest != null ? state.manifest.GetEntry(splineIndex) : null;
        Color baseColor = entry != null && entry.themeColor != Color.white ? entry.themeColor : blockedFallbackColor;
        Color darkened = Color.Lerp(baseColor, Color.black, blockedDarken);
        darkened.a = blockedAlpha;
        return darkened;
    }

    void HandleSplineUnblocked(int splineIndex) => RefreshTrackStyles();

    void AddIcon(MinimapMarker marker)
    {
        if (marker == null || iconTemplate == null || markersRoot == null) return;
        if (_icons.ContainsKey(marker)) return;

        var icon = _freeIcons.Count > 0 ? _freeIcons.Pop() : Instantiate(iconTemplate, markersRoot);
        icon.Bind(marker);
        icon.SetVisible(false);
        _icons[marker] = icon;
    }

    void RemoveIcon(MinimapMarker marker)
    {
        if (marker == null || !_icons.TryGetValue(marker, out var icon)) return;
        _icons.Remove(marker);
        ReleaseIcon(icon);
    }

    void ReleaseIcon(MinimapIconUI icon)
    {
        if (icon == null) return;
        icon.SetVisible(false);
        _freeIcons.Push(icon);
    }
}
