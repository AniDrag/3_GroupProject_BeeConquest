using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabSet
{
    [Tooltip("Name for reference")]
    public string name;
    [Tooltip("Relative weight for picking this set")]
    public float weight = 1f;
    public CellColor color;

    [Header("Buckets (order matters)")]
    [Tooltip("Bucket values, e.g. 90,65,35,5")]
    public List<int> buckets = new List<int>() { 90, 65, 35, 5 };

    [Header("Variant prefabs (index-aligned with buckets list)")]
    public List<GameObject> variants = new List<GameObject>();

    // Return prefab by exact bucket value (buckets list must match)
    public GameObject GetPrefabForBucket(int bucket)
    {
        int idx = buckets.IndexOf(bucket);
        if (idx >= 0 && idx < variants.Count) return variants[idx];
        return null;
    }

#if UNITY_EDITOR
    // Ensure editor defaults align with BucketUtils
    public void OnValidate()
    {
        // If user hasn't set buckets, populate with defaults from BucketUtils.
        if (buckets == null || buckets.Count == 0)
        {
            buckets = new List<int>(BucketUtils.Buckets);
        }
        // Ensure variants has same length (fill with nulls as needed)
        while (variants.Count < buckets.Count) variants.Add(null);
    }
#endif
}
