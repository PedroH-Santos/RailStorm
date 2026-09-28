using System;
using System.Collections.Generic;

public static class MinimapMarkerRegistry
{
    static readonly List<MinimapMarker> Markers = new();

    public static event Action<MinimapMarker> OnMarkerAdded;
    public static event Action<MinimapMarker> OnMarkerRemoved;

    public static IReadOnlyList<MinimapMarker> All => Markers;

    public static void Register(MinimapMarker marker)
    {
        if (marker == null || Markers.Contains(marker)) return;
        Markers.Add(marker);
        OnMarkerAdded?.Invoke(marker);
    }

    public static void Unregister(MinimapMarker marker)
    {
        if (!Markers.Remove(marker)) return;
        OnMarkerRemoved?.Invoke(marker);
    }
}
