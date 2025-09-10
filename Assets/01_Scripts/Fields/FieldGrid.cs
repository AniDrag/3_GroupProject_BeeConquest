using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldGrid
{
    //public readonly int Width;
    //public readonly int Height;
    //public readonly float CellSize;        // world units per cell
    //public readonly Vector3 Origin;        // bottom-left (or whatever you choose)

    //// storage: 2D array for direct O(1) access
    //private FieldCellData[,] cells;

    //// convenience dictionaries if you want O(1) id lookup
    //private Dictionary<int, FieldCellData> idLookup;

    //public FieldGrid(int width, int height, Vector3 origin, float cellSize)
    //{
    //    if (width <= 0 || height <= 0) throw new ArgumentException("width/height must be > 0");
    //    Width = width;
    //    Height = height;
    //    Origin = origin;
    //    CellSize = Mathf.Max(0.0001f, cellSize);

    //    cells = new FieldCellData[width, height];
    //    idLookup = new Dictionary<int, FieldCellData>(width * height);
    //}

    //// simple initializer: fill grid row-major with same default values (you can pass a factory lambda below)
    ////public void Initialize(float defaultMaxDurability = 100f, float defaultInitialDur = 100f, float defaultRegen = 0f, float defaultPollin = 1f, CellColor color = CellColor.Green)
    ////{
    ////    int id = 0;
    ////    for (int y = 0; y < Height; y++)
    ////    {
    ////        for (int x = 0; x < Width; x++)
    ////        {
    ////            var cell = new FieldCellData(id, color, defaultMaxDurability, defaultInitialDur, defaultRegen, defaultPollin);
    ////            cells[x, y] = cell;
    ////            idLookup[id] = cell;
    ////            id++;
    ////        }
    ////    }
    ////}

    //// more flexible initializer using a factory (x,y,id) => FieldCellData
    //public void Initialize(Func<int,int,int,FieldCellData> factory)
    //{
    //    int id = 0;
    //    for (int y = 0; y < Height; y++)
    //    {
    //        for (int x = 0; x < Width; x++)
    //        {
    //            var cell = factory(x, y, id);
    //            cells[x, y] = cell;
    //            idLookup[id] = cell;
    //            id++;
    //        }
    //    }
    //}

    //// direct access
    //public FieldCellData GetCell(int x, int y)
    //{
    //    if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
    //    return cells[x, y];
    //}

    //public FieldCellData GetCellById(int id)
    //{
    //    idLookup.TryGetValue(id, out var c);
    //    return c;
    //}

    //// world position -> grid coordinate -> cell
    //public FieldCellData GetCellAtWorldPos(Vector3 worldPos)
    //{
    //    var local = worldPos - Origin;
    //    int x = Mathf.FloorToInt(local.x / CellSize);
    //    int y = Mathf.FloorToInt(local.z / CellSize); // assume XZ plane; change if Y is used
    //    return GetCell(x, y);
    //}

    //// get cells inside circle (world center, radius in world units)
    //public List<FieldCellData> GetCellsInCircle(Vector3 worldCenter, float radius)
    //{
    //    var list = new List<FieldCellData>();
    //    if (radius <= 0f) return list;

    //    // convert to grid bounds
    //    var local = worldCenter - Origin;
    //    int minX = Mathf.Clamp(Mathf.FloorToInt((local.x - radius) / CellSize), 0, Width - 1);
    //    int maxX = Mathf.Clamp(Mathf.FloorToInt((local.x + radius) / CellSize), 0, Width - 1);
    //    int minY = Mathf.Clamp(Mathf.FloorToInt((local.z - radius) / CellSize), 0, Height - 1);
    //    int maxY = Mathf.Clamp(Mathf.FloorToInt((local.z + radius) / CellSize), 0, Height - 1);

    //    float radiusSqr = radius * radius;
    //    for (int y = minY; y <= maxY; y++)
    //    {
    //        for (int x = minX; x <= maxX; x++)
    //        {
    //            // cell center position
    //            Vector3 cellCenter = Origin + new Vector3((x + 0.5f) * CellSize, 0f, (y + 0.5f) * CellSize);
    //            if ((cellCenter - worldCenter).sqrMagnitude <= radiusSqr)
    //            {
    //                list.Add(cells[x, y]);
    //            }
    //        }
    //    }
    //    return list;
    //}

    //public void Tick(float dt)
    //{
    //    if (dt <= 0f) return;
    //    for (int y = 0; y < Height; y++)
    //    {
    //        for (int x = 0; x < Width; x++)
    //        {
    //            cells[x, y].TickRegeneration(dt);
    //        }
    //    }
    //}
}
