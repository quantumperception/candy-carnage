using UnityEngine;
public class Cell : MonoBehaviour
{
    public Vector3Int m_coords;
    bool m_hovering;
    int m_team = 1;

    [Header("Pathfinding")]
    public int G = 0;
    public int H = 0;
    public int F { get { return G + H; } }
    public Cell m_previous;
    void Start()
    {
    }

    void FixedUpdate()
    {
        //if (!m_hovering) SetCellColor(IsBlocked() ? Color.yellow : Color.white);
    }

    private void OnMouseEnter()
    {
        SetHovering(true);
        BattleGrid.Instance.m_hoveredCell = this;
    }

    private void OnMouseExit()
    {
        SetHovering(false);
    }
    private void OnMouseUp()
    {
        if (!m_hovering) return;
        Unit selectedUnit = BattleManager.Instance.GetSelectedUnit();
        if (selectedUnit == null) return;
        BattleManager.Instance.PlaceUnit(selectedUnit, m_coords, BattleManager.Instance.GetSelectedTeam());
        BattleUI.Instance.PlayHoverSound();
        BattleManager.Instance.SetSelectedUnit(null);
        BattleUI.Instance.SetDraggedElement(null as InventoryItem);
    }

    public Vector3 GetWorldCenter() => BattleGrid.Instance.m_grid.GetCellCenterWorld(m_coords);

    public void SetHovering(bool hover)
    {
        m_hovering = hover;
        SetCellColor(hover ? Color.red : Color.white);
    }
    public void SetCellColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    public bool IsBlocked()
    {
        float halfCellSize = BattleGrid.Instance.m_cellSize * 0.5f;
        Collider[] unitsHit = Physics.OverlapSphere(GetWorldCenter() + new Vector3(0f, halfCellSize, 0f), halfCellSize, 1 << LayerMask.NameToLayer("Units"));
        return unitsHit.Length > 0;
    }

}
