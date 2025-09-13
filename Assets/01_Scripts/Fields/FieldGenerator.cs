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
        int foundedCells = 0;
        if (generateOnStart) GenerateField();
        else
        {
            // Read existing children
            var existingCells = GetComponentsInChildren<FieldCellData>(true); // include inactive just in case
            if (existingCells.Length == 0) return;

            // Prepare arrays / lookups
            cells = new FieldCellData[width, height];
            idLookup.Clear();
            holderLookup.Clear();

            foreach (var cell in existingCells)
            {
                // Defensive: ensure the component is valid
                if (cell == null)
                {
                    Debug.LogWarning("Found null FieldCellData in children; skipping.");
                    continue;
                }

                int cellId = cell.ID;

                // Validate ID
                if (cellId < 0)
                {
                    Debug.LogWarning($"Cell '{cell.name}' has invalid ID {cellId}; skipping.");
                    continue;
                }

                // Determine x,y from id (deterministic)
                int x = cellId % width;
                int y = cellId / width; // integer division

                // If computed y is outside, it's out of bounds
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    Debug.LogWarning($"Cell '{cell.name}' (ID={cellId}) maps to out-of-bounds coords ({x},{y}) for width={width},height={height}. Skipping.");
                    continue;
                }

                // Check duplicates: if there's already a cell at that id/slot, warn and skip or replace
                if (idLookup.ContainsKey(cellId))
                {
                    Debug.LogWarning($"Duplicate cell ID {cellId} found on '{cell.name}'. An earlier cell with same ID is already registered. Skipping this one.");
                    continue;
                }

                // Compute canonical world position for that grid cell and snap holder to it
                Vector3 canonicalPos = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

                // Ensure holder/gameobject reference
                var holder = cell.gameObject;
                holder.name = $"CellHolder_{cellId}"; // rename to canonical holder name

                // If the holder is not parented correctly, set parent
                if (holder.transform.parent != transform)
                    holder.transform.SetParent(transform, true);

                // Snap to canonical position (keeps rotation/scale)
                holder.transform.position = canonicalPos;

                // Put the cell into the arrays and lookups
                cells[x, y] = cell;
                idLookup[cellId] = cell;
                holderLookup[cellId] = holder;
                cell.WorldPosition = holder.transform.position;

                // Ensure FieldCellView exists and set gizmo flag
                var view = holder.GetComponent<FieldCellView>() ?? holder.AddComponent<FieldCellView>();
                view.showGizmos = showGizmosOnHolder;

            }

        }
        Debug.Log($"Succesefully added {foundedCells}/{cells.Length} cells");
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
                // pick a prefab set by weighted random (or null if none)
                PrefabSet chosen = null;
                if (prefabSets != null && prefabSets.Count > 0 && totalWeight > 0f)
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

                // determine position
                Vector3 pos = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

                // create holder (instantiate with parent once)
                GameObject holder;
                if (cellHolderPrefab != null)
                {
                    holder = Instantiate(cellHolderPrefab, pos, Quaternion.identity, transform);
                    holder.name = $"CellHolder_{id}";
                }
                else
                {
                    holder = new GameObject($"CellHolder_{id}");
                    holder.transform.position = pos;
                    holder.transform.parent = transform;
                }

                // attach/get FieldCellData component
                var cell = holder.GetComponent<FieldCellData>();
                if (cell == null) cell = holder.AddComponent<FieldCellData>();

                // pick color from chosen prefab set, fallback to defaultColor
                CellColor cellColor = (chosen != null && chosen.color != null) ? chosen.color : defaultColor;
                // Note: prefabColor likely a Color, not nullable — adjust if needed

                // Setup cell with the prefab-specific color
                cell.Setup(id, pos, cellColor, defaultMaxDurability, defaultInitialDur, defaultRegen, defaultPollin);

                // store references
                cells[x, y] = cell;
                idLookup[id] = cell;

                // attach FieldCellView if not present and initialize
                var view = holder.GetComponent<FieldCellView>() ?? holder.AddComponent<FieldCellView>();
                view.showGizmos = showGizmosOnHolder;

                // give the view the chosen prefab set and the configured cell
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
                if (cells[x, y] != null)
                {
                    cells[x, y].TickRegeneration(dt);
                }
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
