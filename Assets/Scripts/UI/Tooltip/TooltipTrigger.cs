using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    IDrawable _source;
    int _currentRarity;
    bool _hovering;
    TooltipUI _tooltip;

    public void SetSource(IDrawable source, int currentRarity)
    {
        _source = source;
        _currentRarity = currentRarity;
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
        HideTooltip();
    }

    void OnTransformParentChanged() => _tooltip = null;

    void OnDisable()
    {
        _hovering = false;
        HideTooltip();
    }

    void HideTooltip()
    {
        if (_tooltip != null) _tooltip.Hide();
    }

    void Refresh()
    {
        if (_source == null) return;

        var data = TooltipBuilder.Build(_source, _currentRarity);
        if (data == null) return;

        if (_tooltip == null) _tooltip = TooltipUI.For(this);
        _tooltip?.Show(data);
    }
}
