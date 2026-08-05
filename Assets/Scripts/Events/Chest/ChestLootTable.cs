using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChestLootTable", menuName = "Events/Chest Loot Table")]
public class ChestLootTable : ScriptableObject
{
    public List<ItemDefinition> possibleItems = new();

    public int minRarity = 0;

    [Tooltip("Deixe em -1 para usar a raridade mais alta configurada em RarityConfig.")]
    public int maxRarity = -1;

    public int ResolvedMaxRarity => maxRarity >= 0 ? maxRarity : Mathf.Max(0, RarityHelper.Count - 1);
}
