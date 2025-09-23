using AniDrag.Utility;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FieldGenerator : MonoBehaviour, IInteract
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
    public ColorAtribute defaultColor = ColorAtribute.Green;

    // internal storage
    private FieldCellData[,] allCells;
    private Dictionary<int, FieldCellData> idLookup = new();
    private Dictionary<int, GameObject> holderLookup = new(); // id -> holder gameObject
    private FieldCellData[] existingCells;

    // public accessor
    public int TotalCells => width * height;

    private System.Random rng = new System.Random();

    [Header("Debug / Testing")]
    public bool debugClickDamageMode = false;   // when true, click to damage cells at mouse
    public float debugDamageAmount = 10f;
    public bool damageRandomCell = false;
    public bool showDebugText = false;
    private bool lastKnownDebugText = true; // Should be the opposite of showDebugText.

    private PlayerCore player;
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
        Debug.Log($"Player: {interactor.name} || Has entered field: {transform.name}");

        player = interactor.GetComponent<PlayerCore>();
        Game_Manager.instance.AsignCurrentFieldToPlayer(player.playerID, this);
        //interactor.GetComponent<PlayerCore>().AsignField(this);
    }
    public void DeInteract(GameObject interactor)
    {
        Debug.Log($"Player: {interactor.name} || Has Exited field: {transform.name}");
        interactor.GetComponent<PlayerCore>().RemoveField();
        Game_Manager.instance.ExitCurrentFieldFromPlayer(player.playerID);
        player = null;
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
        /* 
         // If no children are in the container → generate fresh field
    if (fieldContainer.childCount == 0)
    {
        GenerateField();

        // After GenerateField, allCells is already filled → reuse it
        existingCells = allCells;
    }
    else
    {
        // Pull already placed children into allCells
        allCells = new FieldCellData[fieldContainer.childCount];
        for (int i = 0; i < fieldContainer.childCount; i++)
        {
            allCells[i] = fieldContainer.GetChild(i).GetComponent<FieldCellData>();
        }

        // Filter out nulls into existingCells
        List<FieldCellData> valid = new List<FieldCellData>();
        foreach (var cell in allCells)
        {
            if (cell != null) valid.Add(cell);
        }
        existingCells = valid.ToArray();
    }*/ // U could use this ? idk

        origin = transform.position;
        if (generateOnStart) GenerateField();
        else
        {
            // Read existing children
            var existingCells = GetComponentsInChildren<FieldCellData>(true); // include inactive just in case
            if (existingCells.Length == 0) return;

            // Prepare arrays / lookups
            allCells = new FieldCellData[width, height];
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
                //holder.transform.position = canonicalPos;

                // Put the cell into the arrays and lookups
                allCells[x, y] = cell;
                idLookup[cellId] = cell;
                holderLookup[cellId] = holder;
                cell.WorldPosition = holder.transform.position;

                // Ensure FieldCellView exists and set gizmo flag
                var view = holder.GetComponent<FieldCellView>() ?? holder.AddComponent<FieldCellView>();
                view.showGizmos = showGizmosOnHolder;

            }

        }

        int foundedCells = 0;
        foreach ( var cell in allCells )
            if ( cell != null )
                foundedCells++;

        Debug.Log($"Trying to add {foundedCells}/{allCells.Length} cells");
        existingCells = new FieldCellData[foundedCells];
        int i = 0;
        foreach (var cell in allCells)
        {
            if (cell != null)
            {
                existingCells[i++] = cell;
            }
        }
        //Debug.Log($"Succesefully added {foundedCells}/{allCells.Length} cells");
        Game_Manager.instance.AsignFieldToServer(this);
    }

    private void Update()
    {
        if (player != null && debugClickDamageMode && Input.GetMouseButtonDown(0))
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

        allCells = new FieldCellData[width, height];
        idLookup.Clear();
        holderLookup.Clear();
        //BoxCollider collider = this.GetComponent<BoxCollider>();
        //if (collider == null)
        //{
        //    collider = this.gameObject.AddComponent<BoxCollider>();
        //}
        //collider.center = new Vector3(width / 2f, 0, height / 2f);
        //collider.size = new Vector3(width, 0, height);
        //collider.isTrigger = true;


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
                    holder = Instantiate(cellHolderPrefab, pos, Quaternion.Euler(0, Random.Range(0, 180), 0), transform);
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
                ColorAtribute cellColor = (chosen != null && chosen.color != null) ? chosen.color : defaultColor;
                // Note: prefabColor likely a Color, not nullable — adjust if needed

                // Setup cell with the prefab-specific color
                cell.Setup(id, pos, cellColor, defaultMaxDurability, defaultInitialDur, defaultRegen, defaultPollin);

                // store references
                allCells[x, y] = cell;
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
        allCells = null;
        idLookup.Clear();
        holderLookup.Clear();
    }

    // quick accessors
    public FieldCellData GetCellByXY(int x, int y)
    {
        if (allCells == null) return null;
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return allCells[x, y];
    }

    public FieldCellData GetCellById(int id)
    {
        idLookup.TryGetValue(id, out var c);
        return c;
    }

    public FieldCellData GetCellAtWorldPos(Vector3 worldPos)
    {
        if (allCells == null) return null;

        // local coords relative to origin (same origin used when generating cells)
        Vector3 local = worldPos - origin;
        float half = cellSize * 0.5f;

        // Map world pos -> grid index using the same center-offset convention used when generating:
        // generation used: origin + (x + 0.5f) * cellSize for cell centers,
        // so subtract half-cell before flooring to get the correct index.
        int x = Mathf.FloorToInt((local.x - half) / cellSize);
        int y = Mathf.FloorToInt((local.z - half) / cellSize);

        // Quick direct hit
        var cell = GetCellByXY(x, y);
        if (cell != null) return cell;

        // Fallback: check small neighborhood around (x,y) to tolerate FP / tiny offsets
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var n = GetCellByXY(x + dx, y + dy);
                if (n != null) return n;
            }
        }

        // Helpful diagnostic log when nothing found (remove or comment out later)
        Debug.LogWarningFormat("GetCellAtWorldPos: not found. world={0} local={1} mapped=({2},{3})",
                               worldPos, local, x, y);

        return null;
    }

    public Vector2Int GetCellArrayPosition(FieldCellData cellData)
    {
        if (cellData == null) return Vector2Int.zero;
        int x = cellData.ID % width;
        int y = cellData.ID / width;
        return new Vector2Int(x,y);
    }
    // tick that you call from Game_Manager.FixedUpdate
    public void Tick(float dt)
    {
        //Debug.Log("Tick");
        //if (allCells == null) return;
        //Debug.Log("Tick is not null");
        //for (int y = 0; y < height; y++)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        if (allCells[x, y] != null)
        //        {
        //            allCells[x, y].TickRegeneration(dt);
        //        }
        //    }
        //}

        if (existingCells  == null) return;
        foreach (var cell in existingCells)
        {
            cell.TickRegeneration(dt);
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
        if (allCells == null) yield break;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                yield return allCells[x, y];
    }

    /// <summary>
    /// Collects candidate cells inside world circle and returns one chosen at random.
    /// If weightByPollin==true, chooses with probability proportional to cell.PollinMultiplier (and >0 dur).
    /// Returns null if none found.
    /// </summary>
    public FieldCellData GetRandomCellInRadius(Vector3 worldCenter, float radius, bool onlyPositiveDurability = true, bool weightByPollin = false)
    {
        if (allCells == null) return null;
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
                var c = allCells[x, y];
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
