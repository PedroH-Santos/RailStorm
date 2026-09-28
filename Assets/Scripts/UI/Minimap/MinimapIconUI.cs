using UnityEngine;
using UnityEngine.UI;

public class MinimapIconUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image disc;
    [SerializeField] private Image glyph;
    [SerializeField] private CanvasGroup group;

    public void Bind(MinimapMarker marker)
    {
        if (disc != null) disc.color = marker.ResolveDiscColor();

        if (glyph != null)
        {
            glyph.sprite = marker.Icon;
            glyph.enabled = marker.Icon != null;
        }

        rect.sizeDelta = Vector2.one * marker.Size;
    }

    public void Place(Vector2 position, float scale, float alpha)
    {
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one * scale;
        if (group != null) group.alpha = alpha;
    }

    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
    }
}
