using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    VisualElement root;
    public static BattleManager Instance { get; private set; }
    public List<Unit> m_units = new();
    public Unit m_selectedUnit;
    int m_selectedTeam = 1;
    Transform m_unitsTransform;
    public Inventory m_itemInventory;
    public Inventory m_unitInventory;
    public Inventory m_materialInventory;
    public Inventory m_craftingInventory;
    public bool m_ready = false;

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Start()
    {
        Setup();
    }

    void Setup()
    {
        Instance.m_unitsTransform = SceneManager.GetActiveScene().GetRootGameObjects().First(go => go.name == "Units").transform;
        m_ready = true;
    }

    public int GetSelectedTeam() => m_selectedTeam;
    public void SetSelectedTeam(int team) { m_selectedTeam = team; }
    public Unit GetSelectedUnit() => m_selectedUnit;
    public void SetSelectedUnit(Unit unit) { m_selectedUnit = unit; }
    public List<Unit> GetEnemyUnits(int team) => m_units.FindAll(u => u.m_team != team);

    public bool PlaceUnit(Unit unit, Vector3Int coords, int team)
    {
        Cell cell = BattleGrid.Instance.TryGetCell(coords);
        if (unit == null || unit.gameObject == null || cell == null || cell.IsBlocked()) return false;
        GameObject go = Instantiate(unit.gameObject, BattleGrid.Instance.m_grid.GetCellCenterWorld(coords), Quaternion.identity, Instance.m_unitsTransform);
        Unit u = go.GetComponent<Unit>();
        u.SetTeam(team);
        m_units.Add(u);
        Debug.Log("Placing unit at: " + coords);
        return true;
    }

    public void KillUnit(Unit unit)
    {
        m_units.Remove(unit);
        Destroy(unit.gameObject);
    }

    public List<TeamBase> GetEnemyBases(int team) => FindObjectsByType<TeamBase>(FindObjectsSortMode.None).ToList().FindAll(b => b.m_team != team);

}
