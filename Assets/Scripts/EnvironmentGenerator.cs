using Cysharp.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentGenerator : MonoBehaviour
{
    public int m_width;
    public int m_height;
    bool m_runningEditor = false;
    [SerializeField] MeshFilter m_terrainFilter;
    Mesh m_terrainMesh;
    void Start()
    {
        m_width = BattleGrid.Instance.m_width * 3;
        m_height = BattleGrid.Instance.m_height * 3;
    }

    void Update()
    {
        CreateTerrain();
    }

    async void StartTerrainUpdate()
    {
        await UniTask.WaitForSeconds(1);
        CreateTerrain();
        StartTerrainUpdate();
    }

    void UpdateTerrain()
    {
        Debug.Log("updating terrain");

        Vector3[] vertices = new Vector3[(m_width + 1) * (m_height + 1)];
        for (int i = 0, y = 0; y <= m_height; y++)
            for (int x = 0; x <= m_width; x++, i++)
                vertices[i] = new Vector3(y, 0, x);
        m_terrainMesh.vertices = vertices;

        int[] triangles = new int[m_width * m_height * 6];
        for (int ti = 0, vi = 0, y = 0; y < m_height; y++, vi++)
        {
            for (int x = 0; x < m_width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + m_width + 1;
                triangles[ti + 5] = vi + m_width + 2;
            }
        }
        m_terrainMesh.triangles = triangles;
    }

    void CreateTerrain()
    {
        Debug.Log("creating terrain");
        m_terrainMesh = new() { name = "Terrain" };
        UpdateTerrain();
        m_terrainMesh.RecalculateNormals();
        m_terrainFilter.mesh = m_terrainMesh;
        transform.position = BattleGrid.Instance.GetWorldCenter();
    }
}
