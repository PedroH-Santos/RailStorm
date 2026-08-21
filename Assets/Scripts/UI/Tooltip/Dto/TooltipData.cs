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
    public bool HasAbility;
    public string AbilityName;
    public string AbilityDescription;
    public int HiddenUpgrades;

    public readonly List<TooltipStatLine> Stats = new();
    public readonly List<TooltipUpgradeLine> Upgrades = new();

}
