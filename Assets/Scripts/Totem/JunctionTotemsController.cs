using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TotemSlot
{
    public int splineIndex;
    public TotemView view;
}

public class JunctionTotemsController : MonoBehaviour
{
    [Tooltip("Monte manualmente: arraste cada totem já posicionado na cena e associe ao índice da spline que ele representa.")]
    public List<TotemSlot> slots;

    public TotemView GetView(int splineIndex)
    {
        var slot = slots.Find(s => s.splineIndex == splineIndex);
        return slot?.view;
    }

    void OnEnable()
    {
        SplineRuntimeState.OnSplineUnblocked += HandleUnblocked;

        foreach (var slot in slots)
            TotemRegistry.Register(slot.splineIndex, slot.view);
    }

    void OnDisable()
    {
        SplineRuntimeState.OnSplineUnblocked -= HandleUnblocked;

        foreach (var slot in slots)
            TotemRegistry.Unregister(slot.splineIndex, slot.view);
    }

    void HandleUnblocked(int splineIndex)
    {
        var slot = slots.Find(s => s.splineIndex == splineIndex);
        if (slot?.view != null)
            slot.view.gameObject.SetActive(false);
    }

    void Start()
    {
        foreach (var slot in slots)
        {
            var entry = SplineRuntimeState.Instance?.manifest?.GetEntry(slot.splineIndex);
            if (entry != null && slot.view != null)
                slot.view.SetThemeColor(entry.themeColor);
        }
    }
}
