using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrid : MonoBehaviour
{
    public static BattleGrid Instance { get; private set; }
    public Grid m_grid;
    Dictionary<Vector3Int, Cell> m_cells = new();
    public int m_width = 15;
    public int m_height = 30;
    public int m_cellSize = 1;
    public int m_gridMargin = 1;
    public GameObject m_cellPrefab;
    public Cell m_hoveredCell;
    public PathFinder m_pathFinder;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        m_grid = GetComponent<Grid>();
        m_pathFinder = new PathFinder();
    }

    void Start()
    {
        ClearGrid();
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (Instance.m_cellPrefab == null) return;
        for (int x = -m_gridMargin; x < Instance.m_height + m_gridMargin; x++)
        {
            for (int z = -m_gridMargin; z < Instance.m_width + m_gridMargin; z++)
            {
                Vector3Int cellCoords = new(x, 0, z);
                GameObject newCell = Instantiate(Instance.m_cellPrefab, Instance.m_grid.GetCellCenterWorld(cellCoords) + Instance.m_grid.cellGap, Quaternion.identity, transform);
                newCell.layer = LayerMask.NameToLayer("Grid");
                newCell.GetComponent<Cell>().m_coords = cellCoords;
                m_cells.Add(cellCoords, newCell.GetComponent<Cell>());
            }
        }
    }
    public Vector3 GetWorldCenter() => new Vector3(m_height * m_cellSize * 0.5f, 0f, m_width * m_cellSize * 0.5f);
    void ClearGrid()
    {
        foreach (Cell cell in m_cells.Values) try { DestroyImmediate(cell); } catch (Exception e) { Debug.LogWarning(e); }
        m_cells.Clear();
    }

    public Dictionary<Vector3Int, Cell> GetCells()
    {
        return m_cells;
    }

    public Cell TryGetCell(Vector3Int coords)
    {
        Cell result;
        Instance.m_cells.TryGetValue(coords, out result);
        if (result == null) return null;
        return result;
    }
}
