using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VfxPoolConfig
{
    public string key;           // unique key you'll use at runtime, e.g. "movement", "explosion"
    public GameObject prefab;    // prefab assigned in inspector
    public int prewarm = 0;      // optional: how many to create up-front
}

public class AbilityVfxPooler : MonoBehaviour
{
    public static AbilityVfxPooler Instance { get; private set; }

    [Header("VFX pool configs")]
    [SerializeField] private List<VfxPoolConfig> configs = new List<VfxPoolConfig>();

    // internal state
    private readonly Dictionary<string, Stack<GameObject>> pools = new Dictionary<string, Stack<GameObject>>();
    private readonly Dictionary<string, GameObject> prefabByKey = new Dictionary<string, GameObject>();
    private readonly List<(GameObject go, float returnAt)> scheduledReturns = new List<(GameObject, float)>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // prepare dictionaries and optionally prewarm
        foreach (var cfg in configs)
        {
            if (string.IsNullOrEmpty(cfg.key) || cfg.prefab == null) continue;
            if (!prefabByKey.ContainsKey(cfg.key))
            {
                prefabByKey[cfg.key] = cfg.prefab;
                pools[cfg.key] = new Stack<GameObject>();

                for (int i = 0; i < cfg.prewarm; i++)
                {
                    var go = CreateNew(cfg.key);
                    Return(go);
                }
            }
            else
            {
                Debug.LogWarning($"Duplicate VFX pool key '{cfg.key}' ignored.");
            }
        }
    }

    GameObject CreateNew(string key)
    {
        if (!prefabByKey.TryGetValue(key, out var prefab) || prefab == null) return null;

        var go = Instantiate(prefab, transform);
        go.name = $"{prefab.name}_{key}";
        go.SetActive(false);

        // Ensure a PoolableVfx component exists and contains the key
        var poolable = go.GetComponent<PoolableVfx>() ?? go.AddComponent<PoolableVfx>();
        poolable.PoolKey = key;
        poolable.RootPooler = this; // let Poolable request return if needed

        return go;
    }

    /// <summary>
    /// Get an instance from the pool by key. Returns active GameObject (or null if key unknown).
    /// </summary>
    public GameObject Get(string key)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"VFX pool key '{key}' not registered.");
            return null;
        }

        GameObject go = pools[key].Count > 0 ? pools[key].Pop() : CreateNew(key);
        if (go == null) return null;

        // detach from pooler so it can live where caller wants
        go.transform.SetParent(null, true);
        go.SetActive(true);

        return go;
    }


    /// <summary>
    /// Schedule this pooled GameObject to be returned after `seconds` (measured in realtime seconds).
    /// If the object is already scheduled, the schedule will be updated.
    /// </summary>
    public void ScheduleReturn(GameObject go, float seconds)
    {
        if (go == null) return;
        // remove existing schedule if present
        CancelScheduledReturn(go);
        float when = Time.time + Mathf.Max(0f, seconds);
        scheduledReturns.Add((go, when));
    }

    /// <summary>
    /// Cancel a previously scheduled return for this object (if any).
    /// </summary>
    public void CancelScheduledReturn(GameObject go)
    {
        if (go == null) return;
        for (int i = scheduledReturns.Count - 1; i >= 0; i--)
        {
            if (scheduledReturns[i].go == go)
                scheduledReturns.RemoveAt(i);
        }
    }

    /// <summary>
    /// Call this from Return() to ensure no leftover scheduled returns exist for the object.
    /// </summary>
    void RemoveScheduledOnReturn(GameObject go)
    {
        CancelScheduledReturn(go);
    }


    /// <summary>
    /// Return a pooled GameObject. If the object was not created by this pooler, it will be deactivated and parented to the pooler root.
    /// </summary>
    public void Return(GameObject go)
    {
        if (go == null) return;

        RemoveScheduledOnReturn(go);

        var poolable = go.GetComponent<PoolableVfx>();
        string key = poolable != null ? poolable.PoolKey : null;

        // deactivate and parent back
        go.SetActive(false);
        go.transform.SetParent(transform, false);

        if (!string.IsNullOrEmpty(key) && pools.ContainsKey(key))
        {
            pools[key].Push(go);
        }
        else
        {
            // unknown key -> keep as loose child of pooler (so it won't pollute scene root)
            // don't destroy: we might want to manually reassign in inspector later
        }
    }

    // small helper to check availability (optional)
    public bool HasKey(string key) => prefabByKey.ContainsKey(key);
}
