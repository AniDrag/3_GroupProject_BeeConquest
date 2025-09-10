using System;
using UnityEngine;

[Serializable]
// : MonoBehaviour IS MANDATORY, SO WE CAN COPY/PASTE AND CHANGE DATA
public class FieldCellData : MonoBehaviour
{
    public int ID;
    public CellColor Color;

    public Vector3 WorldPosition;
    public float MaxDurability;
    private float currentDurability;
    public float RegenPerSecond;
    public float PollinMultiplier;

    // events (subscribers: views, network, etc.)
    public event Action<FieldCellData, int> OnPercentBucketChanged;
    public event Action<FieldCellData> OnStatsChanged;

    private int lastPercentBucket = -1;
    private void Awake()
    {
        currentDurability = MaxDurability;
    }

    public FieldCellData(int id, Vector3 worldPos, CellColor color,
        float maxDur, float initialDur, float regen, float pollinMul)
    {
        ID = id;
        WorldPosition = worldPos;
        Color = color;
        MaxDurability = Mathf.Max(0.0001f, maxDur);
        currentDurability = Mathf.Clamp(initialDur, 0f, MaxDurability);
        RegenPerSecond = regen;
        PollinMultiplier = pollinMul;
        UpdateBucketIfNeeded(); // triggers percent event once
        OnStatsChanged?.Invoke(this);
    }

    public void Setup(int id, Vector3 worldPos, CellColor color,
    float maxDur, float initialDur, float regen, float pollinMul)
    {
        ID = id;
        WorldPosition = worldPos;
        Color = color;
        MaxDurability = Mathf.Max(0.0001f, maxDur);
        currentDurability = Mathf.Clamp(initialDur, 0f, MaxDurability);
        RegenPerSecond = regen;
        PollinMultiplier = pollinMul;
        UpdateBucketIfNeeded(); // triggers percent event once
        OnStatsChanged?.Invoke(this);
    }

    public float CurrentDurability => currentDurability;
    public float GetDurabilityPercent() => (currentDurability / MaxDurability) * 100f;

    public void DecreaseDurability(float amount)
    {
        if (amount <= 0f) return;
        currentDurability = Mathf.Max(0f, currentDurability - amount);
        UpdateBucketIfNeeded();
        OnStatsChanged?.Invoke(this);
    }

    public void IncreaseDurability(float amount)
    {
        if (amount <= 0f) return;
        currentDurability = Mathf.Min(MaxDurability, currentDurability + amount);
        UpdateBucketIfNeeded();
        OnStatsChanged?.Invoke(this);
    }

    public void TickRegeneration(float dt)
    {
        //Debug.Log("Trying to tickRegen");
        if (dt <= 0f || Mathf.Approximately(RegenPerSecond, 0f)) return;
        //Debug.Log("Successefull tick Regen");
        currentDurability = Mathf.Clamp(currentDurability + RegenPerSecond * dt, 0f, MaxDurability);
        UpdateBucketIfNeeded();
        OnStatsChanged?.Invoke(this);
    }

    private void UpdateBucketIfNeeded()
    {
        int bucket = BucketUtils.PercentToBucket(GetDurabilityPercent());
        if (bucket != lastPercentBucket)
        {
            lastPercentBucket = bucket;
            OnPercentBucketChanged?.Invoke(this, bucket);
        }
    }


    // Force-send current bucket (invoked from outside safely)
    public void ForceNotifyCurrentBucket()
    {
        int bucket = BucketUtils.PercentToBucket(GetDurabilityPercent());
        OnPercentBucketChanged?.Invoke(this, bucket);
    }

    // debugging helper
    public string GetDebugInfo()
    {
        return $"ID:{ID}\nDur:{currentDurability:F1}/{MaxDurability:F1}\nRegen:{RegenPerSecond:F2}\nMul:{PollinMultiplier:F2}\nCol:{Color}";
    }
}

