using System.Collections.Generic;
using UnityEngine;

public class TooltipData
{
    public string HeaderLabel;
    public string Title;
    public string RarityLabel;
    public Color RarityColor = Color.white;
    public Sprite Icon;
    public string Description;
    public readonly List<TooltipStatLine> Stats = new();
}
