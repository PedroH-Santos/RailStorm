using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemManifest", menuName = "Items/Item Manifest")]
public class ItemManifest : ScriptableObject
{
    public List<ItemDefinition> items = new();

    [SerializeField] private int _nextId = 0;

    public ItemDefinition GetItem(int itemId)
    {
        foreach (var item in items)
            if (item.id == itemId) return item;
        return null;
    }

#if UNITY_EDITOR
    // Atribui id apenas aos itens que ainda não têm (id == -1).
    // Itens já existentes mantêm seu id, mesmo se a lista for reordenada.
    public void AssignIdsToNewItems()
    {
        foreach (var item in items)
        {
            if (item.id != -1) continue;

            item.id = _nextId;
            _nextId++;
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}