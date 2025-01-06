using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentGenerator : MonoBehaviour
{
    [SerializeField] BattleGrid m_battleGrid;
    public int m_width;
    public int m_height;
    public float m_tileSize = 1f;
    bool m_runningEditor = false;
    public float m_randomStrength = 1;
    [SerializeField] MeshFilter m_terrainFilter;
    Mesh m_terrainMesh;
    float[] m_randomYCoords;
    bool m_updating = false;
    public Vector2 m_perlinOffsetDetail;
    public Vector2 m_perlinScaleDetail;
    public Vector2 m_perlinOffsetBase;
    public Vector2 m_perlinScaleBase;
    void Start()
    {

    }

    private void OnEnable()
    {
        m_width = m_battleGrid.m_width * 30;
        m_height = m_battleGrid.m_height * 30;
        m_updating = true;
        StartTerrainUpdate();

    }
    private void OnDisable()
    {
        m_updating = false;
    }
    void Update()
    {
    }

    async void StartTerrainUpdate()
    {
        if (!m_updating) return;
        await UniTask.WaitForSeconds(0.1f);
        CreateTerrain();
        StartTerrainUpdate();
    }

    void CreateRandoms()
    {
        m_randomYCoords = new float[(m_width * 2 + 1) * (m_height * 2 + 1)];
        int i = 0;
        for (float y = 0f; y <= m_height; y += m_tileSize)
            for (float x = 0f; x <= m_width; x += m_tileSize, i++)
                m_randomYCoords[i] =
                    Mathf.PerlinNoise(
                        (x + m_perlinOffsetDetail.x) * m_perlinScaleDetail.x,
                        (y + m_perlinOffsetDetail.y) * m_perlinScaleDetail.y)
                    + (Mathf.PerlinNoise(
                        (x + m_perlinOffsetBase.x) * m_perlinScaleBase.x,
                        (y + m_perlinOffsetBase.y) * m_perlinScaleBase.y)) * 3;
    }

    void UpdateTerrain()
    {
        //Debug.Log("updating terrain");
        m_terrainMesh.Clear();
        Vector3[] vertices = new Vector3[(m_width * 2 + 1) * (m_height * 2 + 1)];
        int i = 0;
        for (float y = 0f; y <= m_height; y += m_tileSize)
            for (float x = 0f; x <= m_width; x += m_tileSize, i++)
                vertices[i] = new Vector3(y, m_randomYCoords[i], x);

        int[] triangles = new int[m_width * 2 * m_height * 2 * 6];
        int tris = 0, vert = 0;
        for (float y = 0f; y < m_height; y += m_tileSize)
        {
            for (float x = 0f; x < m_width; x += m_tileSize)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + m_width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + m_width + 1;
                triangles[tris + 5] = vert + m_width + 2;
                vert++;
                tris += 6;
            }
        }
        //for (int idx = 1; idx < triangles.Length; idx++)
        //{
        //    Debug.DrawLine(vertices[triangles[i - 1]], vertices[triangles[idx]], Color.red, 5f);
        //}
        m_terrainMesh.vertices = vertices;
        m_terrainMesh.triangles = triangles;
        m_terrainMesh.RecalculateNormals();
    }

    public void CreateTerrain()
    {
        //Debug.Log("creating terrain");
        m_terrainMesh = new() { name = "Terrain" };
        CreateRandoms();
        UpdateTerrain();
        m_terrainFilter.mesh = m_terrainMesh;
        transform.position = -2 * m_battleGrid.GetWorldCenter();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnvironmentGenerator))]
public class EnvironmentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EnvironmentGenerator eg = target as EnvironmentGenerator;
        if (GUILayout.Button("Create Terrain")) eg.CreateTerrain();
    }
}

#endif
