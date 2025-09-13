using AniDrag.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FieldGenerator : MonoBehaviour, Iinteract
{
    [Header("Generation settings, DO NOT CHANGE AFTER GENERATING.")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    private Vector3 origin = Vector3.zero;
    public bool generateOnStart = true;

    [Header("Prefab sets (each set has 4 variants)")]
    public List<PrefabSet> prefabSets = new();

    [Header("Holder settings")]
    public GameObject cellHolderPrefab; // optional empty holder prefab (used to add scripts/gizmos)
    public bool showGizmosOnHolder = true;

    [Header("Default cell settings")]
    public float defaultMaxDurability = 100f;
    public float defaultInitialDur = 100f;
    public float defaultRegen = 1f;
    public float defaultPollin = 1f;
    public CellColor defaultColor = CellColor.Green;

    // internal storage
    private FieldCellData[,] cells;
    private Dictionary<int, FieldCellData> idLookup = new();
    private Dictionary<int, GameObject> holderLookup = new(); // id -> holder gameObject

    // public accessor
    public int TotalCells => width * height;

    private System.Random rng = new System.Random();

    [Header("Debug / Testing")]
    public bool debugClickDamageMode = false;   // when true, click to damage cells at mouse
    public float debugDamageAmount = 10f;
    public bool damageRandomCell = false;
    public bool showDebugText = false;
    private bool lastKnownDebugText = true; // Should be the opposite of showDebugText.
    [ContextMenu("Debug - Damage Random Cell")]

    [Button]
    private void DebugDamageRandomCell()
    {
        if (TotalCells == 0) return;
        int randomId = UnityEngine.Random.Range(0, TotalCells);
        var c = GetCellById(randomId);
        if (c != null)
        {
            c.DecreaseDurability(debugDamageAmount);
            RefreshCellVisual(c.ID);
        }
    }



    public string GetInteractionText() => "";

    public void Interact(GameObject interactor)
    {
        interactor.GetComponent<PlayerCore>().currentField = this;
    }
    public void DeInteract(GameObject interactor)
    {
        interactor.GetComponent<PlayerCore>().currentField = null;
    }
    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;    
    public InteractionType Type() => InteractionType.WhenInRange;



    private void OnEnable()
    {
        Game_Manager.OnFixedTick += HandleFixedTick;
    }

    private void OnDisable()
    {
        Game_Manager.OnFixedTick -= HandleFixedTick;
    }

    private void HandleFixedTick(float dt)
    {
        // do whatever ticks the field (regeneration etc)
        Tick(dt);
    }

    private void Start()
    {
        origin = transform.position;
        if (generateOnStart) GenerateField();
        else
        {
            // TODO: read children's FieldCellData and add it accordingly to the arrays: cells, idLookup, holderLookup.
            // Read existing children
            var existingCells = GetComponentsInChildren<FieldCellData>();
            if (existingCells.Length == 0) return;

            // Determine grid size based on positions (optional: you can assume width/height is correct)
            cells = new FieldCellData[width, height];
            idLookup.Clear();
            holderLookup.Clear();

            foreach (var cell in existingCells)
            {
                Vector3 local = cell.transform.position - origin;
                int x = Mathf.FloorToInt(local.x / cellSize);
                int y = Mathf.FloorToInt(local.z / cellSize);

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    Debug.LogWarning($"Cell {cell.name} is out of bounds ({x},{y}), did you change the original width and height?");
                    continue;
                }

                // assign to arrays
                cells[x, y] = cell;
                idLookup[cell.ID] = cell;

                // assign holder lookup
                holderLookup[cell.ID] = cell.gameObject;

                // Ensure it has a FieldCellView
                var view = cell.GetComponent<FieldCellView>() ?? cell.gameObject.AddComponent<FieldCellView>();
                view.showGizmos = showGizmosOnHolder;
            }
        }
        Game_Manager.instance.AsignFieldToServer(this);
        Game_Manager.instance.AsignCurrentFieldToPlayer(0,this);
    }

    private void Update()
    {
        if (debugClickDamageMode && Input.GetMouseButtonDown(0))
        {
            // raycast into XZ plane at y = origin.y
            Camera cam = Camera.main;
            if (cam == null) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, origin.y, 0));
            if (plane.Raycast(ray, out float enter))// how to acces cell
            {
                Vector3 hit = ray.GetPoint(enter);
                var cell = GetCellAtWorldPos(hit);
                if (cell != null)
                {
                    cell.DecreaseDurability(debugDamageAmount);
                    // optional: force update visuals
                    RefreshCellVisual(cell.ID);// moight not be needed
                }
            }
        }

        //if (damageRandomCell)
        //{
        //    DebugDamageRandomCell();
        //}

        UpdateDebugTextVisibility();
    }

    private void UpdateDebugTextVisibility()
    {
        if (showDebugText == lastKnownDebugText) return;

        lastKnownDebugText = showDebugText;

        // find all TextMesh components in children
        TextMesh[] debugTexts = GetComponentsInChildren<TextMesh>(true); // 'true' to include inactive
        foreach (var tm in debugTexts)
        {
            tm.gameObject.SetActive(showDebugText); // enable/disable the GameObject
        }
    }


    public void GenerateField()
    {
        ClearPreviousField();

        cells = new FieldCellData[width, height];
        idLookup.Clear();
        holderLookup.Clear();

        int id = 0;
        // precompute total weight for weighted random
        float totalWeight = 0f;
        foreach (var s in prefabSets) totalWeight += Mathf.Max(0f, s.weight);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // world pos = origin + (x+0.5)*cellSize on X, (y+0.5)*cellSize on Z
                Vector3 pos = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

                // create holder
                var holder = (cellHolderPrefab != null) ? Instantiate(cellHolderPrefab, pos, Quaternion.identity, transform)
                                                      : new GameObject($"CellHolder_{id}");
                if (cellHolderPrefab == null) holder.transform.position = pos;
                holder.name = $"CellHolder_{id}";
                holder.transform.parent = transform;

                //var cell = new FieldCellData(id, pos, defaultColor, defaultMaxDurability, defaultInitialDur, defaultRegen, defaultPollin);
                var cell = holder.GetComponent<FieldCellData>();
                if (cell == null) cell = holder.AddComponent<FieldCellData>();
                cell.Setup(id, pos, defaultColor, defaultMaxDurability, defaultInitialDur, defaultRegen, defaultPollin);
                cells[x, y] = cell;
                idLookup[id] = cell;

                // attach FieldCellView if not present
                var view = holder.GetComponent<FieldCellView>() ?? holder.AddComponent<FieldCellView>();
                view.showGizmos = showGizmosOnHolder;

                // pick a prefab set by weighted random (or none if no sets)
                PrefabSet chosen = null;
                if (prefabSets.Count > 0 && totalWeight > 0f)
                {
                    float r = (float)rng.NextDouble() * totalWeight;
                    float acc = 0f;
                    foreach (var s in prefabSets)
                    {
                        acc += Mathf.Max(0f, s.weight);
                        if (r <= acc) { chosen = s; break; }
                    }
                    if (chosen == null) chosen = prefabSets[prefabSets.Count - 1]; // fallback
                }

                view.InitializeWithPrefabSet(chosen, cell);
                holderLookup[id] = holder;

                id++;
            }
        }
    }

    public void ClearPreviousField()
    {
        // remove holders
        foreach (Transform t in transform)
        {
            DestroyImmediate(t.gameObject);
        }
        cells = null;
        idLookup.Clear();
        holderLookup.Clear();
    }

    // quick accessors
    public FieldCellData GetCellByXY(int x, int y)
    {
        if (cells == null) return null;
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return cells[x, y];
    }

    public FieldCellData GetCellById(int id)
    {
        idLookup.TryGetValue(id, out var c);
        return c;
    }

    public FieldCellData GetCellAtWorldPos(Vector3 worldPos)
    {
        if (cells == null) return null;
        Vector3 local = worldPos - origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.z / cellSize);
        return GetCellByXY(x, y);
    }

    // tick that you call from Game_Manager.FixedUpdate
    public void Tick(float dt)
    {
        //Debug.Log("Tick");
        if (cells == null) return;
        //Debug.Log("Tick is not null");
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                cells[x, y].TickRegeneration(dt);
            }
        }
    }

    // Helper to manually refresh a single cell's visual (for example after direct change)
    public void RefreshCellVisual(int id)
    {
        if (!idLookup.ContainsKey(id)) return;
        var cell = idLookup[id];
        // ask the data to compute & notify its bucket (uses BucketUtils internally)
        cell.ForceNotifyCurrentBucket();
    }


    // expose all cells if needed
    public IEnumerable<FieldCellData> AllCells()
    {
        if (cells == null) yield break;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                yield return cells[x, y];
    }

    /// <summary>
    /// Collects candidate cells inside world circle and returns one chosen at random.
    /// If weightByPollin==true, chooses with probability proportional to cell.PollinMultiplier (and >0 dur).
    /// Returns null if none found.
    /// </summary>
    public FieldCellData GetRandomCellInRadius(Vector3 worldCenter, float radius, bool onlyPositiveDurability = true, bool weightByPollin = false)
    {
        if (cells == null) return null;
        // convert to local grid coords (assuming Origin and cellSize)
        Vector3 local = worldCenter - origin;
        int minX = Mathf.Clamp(Mathf.FloorToInt((local.x - radius) / cellSize), 0, width - 1);
        int maxX = Mathf.Clamp(Mathf.FloorToInt((local.x + radius) / cellSize), 0, width - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt((local.z - radius) / cellSize), 0, height - 1);
        int maxY = Mathf.Clamp(Mathf.FloorToInt((local.z + radius) / cellSize), 0, height - 1);

        float radiusSqr = radius * radius;
        var candidates = new List<FieldCellData>();

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var c = cells[x, y];
                if (c == null) continue;

                // quick circle check against cell center
                Vector3 center = c.WorldPosition; // your FieldCellDataBehaviour uses transform.position; for pure data use stored WorldPosition
                if ((center - worldCenter).sqrMagnitude > radiusSqr) continue;

                if (onlyPositiveDurability && c.CurrentDurability <= 0f) continue;

                candidates.Add(c);
            }
        }

        if (candidates.Count == 0) return null;

        if (!weightByPollin)
        {
            // uniform random
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
        else
        {
            // weighted by PollinMultiplier (or fallback to 1)
            float total = 0f;
            for (int i = 0; i < candidates.Count; i++) total += Mathf.Max(0.0001f, candidates[i].PollinMultiplier);
            float r = UnityEngine.Random.value * total;
            float acc = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                acc += Mathf.Max(0.0001f, candidates[i].PollinMultiplier);
                if (r <= acc) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }
    }
}
