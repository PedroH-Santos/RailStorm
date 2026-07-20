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

    void OnEnable() => SplineRuntimeState.OnSplineUnblocked += HandleUnblocked;
    void OnDisable() => SplineRuntimeState.OnSplineUnblocked -= HandleUnblocked;


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


        var cam = Camera.main;
        if (cam == null) return;

        foreach (var slot in slots)
        {
            if (slot.view == null) continue;
            float dist = Vector3.Distance(cam.transform.position, slot.view.transform.position);
            slot.view.SetCanvasSortOrder(Mathf.RoundToInt(-dist * 10f));
        }
    }
}