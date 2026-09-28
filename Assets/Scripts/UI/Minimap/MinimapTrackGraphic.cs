using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class MinimapTrackGraphic : MaskableGraphic
{
    [SerializeField] private float width = 5f;
    [SerializeField] private bool dashed;
    [SerializeField] private float dashLength = 10f;
    [SerializeField] private float gapLength = 7f;

    readonly List<Vector2> _points = new();

    public void SetPoints(IReadOnlyList<Vector2> points)
    {
        _points.Clear();
        _points.AddRange(points);
        SetVerticesDirty();
    }

    public void SetStyle(Color lineColor, float lineWidth, bool isDashed)
    {
        color = lineColor;
        width = lineWidth;
        dashed = isDashed;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_points.Count < 2 || width <= 0f) return;

        if (dashed) PopulateDashed(vh);
        else PopulateSolid(vh);
    }

    void PopulateSolid(VertexHelper vh)
    {
        for (int i = 0; i < _points.Count - 1; i++)
            AddQuad(vh, _points[i], _points[i + 1], true);
    }

    void PopulateDashed(VertexHelper vh)
    {
        float pattern = Mathf.Max(0.01f, dashLength + gapLength);
        float traveled = 0f;

        for (int i = 0; i < _points.Count - 1; i++)
        {
            Vector2 a = _points[i];
            Vector2 b = _points[i + 1];
            float length = Vector2.Distance(a, b);
            if (length < 0.0001f) continue;

            float along = 0f;
            while (along < length - 0.0001f)
            {
                float phase = traveled % pattern;
                bool inDash = phase < dashLength;
                float remaining = inDash ? dashLength - phase : pattern - phase;
                float step = Mathf.Max(0.0001f, Mathf.Min(remaining, length - along));

                if (inDash)
                    AddQuad(vh, Vector2.Lerp(a, b, along / length), Vector2.Lerp(a, b, (along + step) / length), false);

                along += step;
                traveled += step;
            }
        }
    }

    void AddQuad(VertexHelper vh, Vector2 from, Vector2 to, bool squareCaps)
    {
        Vector2 direction = to - from;
        if (direction.sqrMagnitude < 0.00001f) return;
        direction.Normalize();

        float half = width * 0.5f;
        if (squareCaps)
        {
            from -= direction * half;
            to += direction * half;
        }

        Vector2 normal = new Vector2(-direction.y, direction.x) * half;
        int start = vh.currentVertCount;
        Color32 vertexColor = color;

        vh.AddVert(from - normal, vertexColor, Vector2.zero);
        vh.AddVert(from + normal, vertexColor, Vector2.zero);
        vh.AddVert(to + normal, vertexColor, Vector2.zero);
        vh.AddVert(to - normal, vertexColor, Vector2.zero);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start);
    }
}
