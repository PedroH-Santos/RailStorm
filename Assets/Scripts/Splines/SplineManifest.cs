using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class SplineEntry
{
    public int index;
    public string displayName;
    public bool isBlockedByDefault;
    public int unlockCost = 10;
}

[CreateAssetMenu(fileName = "SplineManifest", menuName = "Splines/Spline Manifest")]
public class SplineManifest : ScriptableObject
{
    public List<SplineEntry> entries = new();

    public SplineEntry GetEntry(int splineIndex)
    {
        foreach (var e in entries)
            if (e.index == splineIndex) return e;
        return null;
    }

#if UNITY_EDITOR
    public void PopulateFromContainer(SplineContainer container)
    {
        if (container == null) return;

        for (int i = 0; i < container.Splines.Count; i++)
        {
            bool exists = false;
            foreach (var e in entries)
            {
                if (e.index == i) { exists = true; break; }
            }

            if (!exists)
            {
                entries.Add(new SplineEntry
                {
                    index = i,
                    displayName = $"Spline {i}",
                    isBlockedByDefault = false,
                    unlockCost = 10
                });
            }
        }

        entries.Sort((a, b) => a.index.CompareTo(b.index));

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}