using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FieldCellView : MonoBehaviour
{
    [Header("Gizmos / visual")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    public float gizmoSize = 0.15f;

    [Header("Debug text")]
    public bool showDebugText = true;
    public int fontSize = 72;
    public Vector3 textOffset = new Vector3(0f, 1.0f, 0f);

    // VariantInstances keyed by bucket value
    private readonly Dictionary<int, GameObject> variantInstances = new();
    private FieldCellData boundData;
    private TextMesh debugText;

    private void Start()
    {
        // If we already exist, rewrite our references
        variantInstances.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("variant_"))
            {
                if (int.TryParse(child.name.Replace("variant_", ""), out int bucket))
                {
                    variantInstances[bucket] = child.gameObject;
                    child.gameObject.SetActive(false); // reset before enabling correct one
                    //Debug.Log($"{child.name} is put in a bucket: {bucket}");
                }
            }
            else if (child.name == "DebugText")
                debugText = child.GetComponent<TextMesh>();
        }

        boundData = this.transform.GetComponent<FieldCellData>();
        if (boundData != null)
        {
            boundData.OnPercentBucketChanged += OnBucketChanged;
            boundData.OnStatsChanged += OnStatsChanged;

            // Ask the data object to notify the correct bucket now (uses BucketUtils)
            boundData.ForceNotifyCurrentBucket();

            // Update text immediately
            UpdateDebugText();
        }
    }

    public void InitializeWithPrefabSet(PrefabSet set, FieldCellData data)
    {
        boundData = data;

        // Clear previous instances if any
        foreach (var kv in variantInstances) if (kv.Value != null) Destroy(kv.Value);
        variantInstances.Clear();

        // Instantiate variants based on the set's bucket list (if provided)
        if (set != null && set.buckets != null)
        {
            // Ensure variants list length is safe (Editor OnValidate in PrefabSet already does this)
            for (int i = 0; i < set.buckets.Count; i++)
            {
                int bucket = set.buckets[i];
                GameObject prefab = (i < set.variants.Count) ? set.variants[i] : null;
                TryInstantiateVariant(bucket, prefab);
            }
        }

        // Setup debug text
        if (showDebugText)
        {
            if (debugText == null)
            {
                var txtGO = new GameObject("DebugText");
                txtGO.transform.SetParent(transform, false);
                txtGO.transform.localPosition = textOffset;
                debugText = txtGO.AddComponent<TextMesh>();
                debugText.anchor = TextAnchor.MiddleCenter;
                debugText.alignment = TextAlignment.Center;
                debugText.fontSize = fontSize;
                debugText.characterSize = 0.02f;
                debugText.color = Color.white;
            }
            UpdateDebugText();
        }

        // Subscribe
        if (boundData != null)
        {
            boundData.OnPercentBucketChanged += OnBucketChanged;
            boundData.OnStatsChanged += OnStatsChanged;

            // Ask the data object to notify the correct bucket now (uses BucketUtils)
            boundData.ForceNotifyCurrentBucket();

            // Update text immediately
            UpdateDebugText();
        }
    }

    private void TryInstantiateVariant(int bucket, GameObject prefab)
    {
        if (prefab == null) return;
        var inst = Instantiate(prefab, transform);
        inst.name = $"variant_{bucket}";
        inst.transform.localPosition = Vector3.zero;
        inst.SetActive(false);
        variantInstances[bucket] = inst;
    }

    private void OnBucketChanged(FieldCellData data, int bucket)
    {
        //Debug.Log($"[FieldCellView] Cell {data.ID} bucket={bucket} pct={data.GetDurabilityPercent():F1}");

        // Enable matching variant, disable others
        bool activated = false;
        foreach (var kv in variantInstances)
        {
            var go = kv.Value;
            if (go == null) continue;
            bool should = kv.Key == bucket && bucket != 0;
            go.SetActive(should);
            if (should) activated = true;
        }

        if (!activated)
        {
            // Log if no prefab matched this bucket (or basically has no pollen left)
            Debug.Log($"[FieldCellView] No prefab for bucket {bucket} on cell {data.ID}, cell has no pollen");
        }

        UpdateDebugText();
    }

    private void OnStatsChanged(FieldCellData data)
    {
        //Debug.Log("Stats are changing");
        UpdateDebugText();
        // Add a method here for the client text.
    }

    private void UpdateDebugText()
    {
        //Debug.Log($"Updating DEBUG text, false = {showDebugText}, the debugText is {debugText == null}, boundData is {boundData == null}");
        if (!showDebugText || debugText == null || boundData == null) return;
        debugText.text = boundData.GetDebugInfo();
        var cam = Camera.main;
        if (cam != null)
        {
            debugText.transform.rotation = Quaternion.LookRotation(debugText.transform.position - cam.transform.position);
        }
    }

    private void OnDestroy()
    {
        if (boundData != null)
        {
            boundData.OnPercentBucketChanged -= OnBucketChanged;
            boundData.OnStatsChanged -= OnStatsChanged;
            boundData = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}

