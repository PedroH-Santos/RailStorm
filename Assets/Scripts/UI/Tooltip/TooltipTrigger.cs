using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    IDrawable _source;
    bool _hovering;

    public void SetSource(IDrawable source)
    {
        _source = source;
        if (_hovering) Refresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        if (TooltipUI.HasInstance) TooltipUI.Instance.Hide();
    }

    void OnDisable()
    {
        _hovering = false;
        if (TooltipUI.HasInstance) TooltipUI.Instance.Hide();
    }

    void Refresh()
    {
        if (_source == null) return;

        var data = TooltipBuilder.Build(_source);
        if (data == null) return;

        TooltipUI.Instance?.Show(data);
    }
}
