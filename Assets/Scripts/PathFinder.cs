using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder
{
    public class PathCell
    {
        public Vector3Int coords;
        public int G, H;
        public int F { get { return G + H; } }
        public PathCell(Vector3Int coords) { this.coords = coords; }
    }

    public List<Cell> FindPath(Vector3Int from, Vector3Int to)
    {
        List<Cell> result = new List<Cell>();
        if (BattleGrid.Instance.TryGetCell(from) == null || BattleGrid.Instance.TryGetCell(to) == null)
        {
            Debug.LogWarning("FindPath failed: Can't find origin or destination cell");
            return result;
        }

        List<Cell> cellsToScan = new List<Cell>();
        List<Cell> scannedCells = new List<Cell>();
        cellsToScan.Add(BattleGrid.Instance.TryGetCell(from));

        while (cellsToScan.Count > 0)
        {
            Cell currentCell = cellsToScan.OrderBy(c => c.F).First();
            cellsToScan.Remove(currentCell);
            scannedCells.Add(currentCell);
            if (currentCell.m_coords == to)
                return GetFinalPath(from, to);

            var neighbourCells = GetNeighbourCells(currentCell);
            foreach (Cell cell in neighbourCells)
            {
                if (scannedCells.Contains(cell) || (cell.m_coords != to && cell.IsBlocked())) continue;
                cell.G = GetManhattanDistance(from, cell.m_coords);
                cell.H = GetManhattanDistance(to, cell.m_coords);
                cell.m_previous = currentCell;
                if (!cellsToScan.Contains(cell)) { cellsToScan.Add(cell); }
            }
        }
        Debug.LogWarning("Returning empty path");
        return result;
    }

    public List<Cell> GetFinalPath(Vector3Int from, Vector3Int to)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = BattleGrid.Instance.TryGetCell(to);
        while (currentCell.m_coords != from)
        {
            path.Add(currentCell);
            currentCell = currentCell.m_previous;
        }
        path.Reverse();
        return path;
    }

    public int GetManhattanDistance(Vector3Int from, Vector3Int to) => Mathf.Abs(from.x - to.x) + Mathf.Abs(from.z - to.z);

    public List<Cell> GetNeighbourCells(Cell cell)
    {
        Dictionary<Vector3Int, Cell> cells = BattleGrid.Instance.GetCells();
        List<Cell> neighbours = new List<Cell>();
        Vector3Int[] neighbourCoords = new Vector3Int[] {
            cell.m_coords + new Vector3Int(0,0, 1),
            cell.m_coords + new Vector3Int(1,0, 0),
            cell.m_coords + new Vector3Int(0, 0,-1),
            cell.m_coords + new Vector3Int(-1, 0,0),
        };
        for (int i = 0; i < neighbourCoords.Length; i++)
            if (cells.ContainsKey(neighbourCoords[i])) neighbours.Add(cells[neighbourCoords[i]]);
        return neighbours;
    }
}