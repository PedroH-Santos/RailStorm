using System.Collections.Generic;

public static class TotemRegistry
{
    static readonly Dictionary<int, TotemView> ViewsBySplineIndex = new();

    public static void Register(int splineIndex, TotemView view)
    {
        if (view == null) return;
        ViewsBySplineIndex[splineIndex] = view;
    }

    public static void Unregister(int splineIndex, TotemView view)
    {
        if (ViewsBySplineIndex.TryGetValue(splineIndex, out var current) && current == view)
            ViewsBySplineIndex.Remove(splineIndex);
    }

    public static bool TryGet(int splineIndex, out TotemView view) =>
        ViewsBySplineIndex.TryGetValue(splineIndex, out view);

    public static IEnumerable<TotemView> AllViews => ViewsBySplineIndex.Values;
}
