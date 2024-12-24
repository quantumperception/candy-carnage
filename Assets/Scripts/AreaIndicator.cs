
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class AreaIndicator : MonoBehaviour
{
    public enum AreaType
    {
        Circle,
        Rect,
        Triangle
    }
    public GameObject m_segment;
    public float m_radius = 1f;
    public int m_segmentAmount = 16;
    GameObject[] m_segments = new GameObject[] { };
    public Color m_color = Color.red;
    public float m_rotationSpeed = 10f;
    public AreaType m_type = AreaType.Circle;


    private void Start()
    {
        if (m_segment == null) CreateSegment();
        DrawCircle(transform.position, m_radius, m_segmentAmount, m_color);
    }

    void Update()
    {
        transform.Rotate(new Vector3(0f, m_rotationSpeed * Time.deltaTime, 0f));
    }

    void CreateSegment()
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.transform.localScale = new Vector3(0.3f, 0.1f, 0.1f);
        Renderer renderer = m_segment.GetOrAddComponent<Renderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        renderer.sharedMaterial.color = m_color;
        m_segment = segment;
        Destroy(segment);
    }

    void DrawCircle(Vector3 position, float radius, int segments, Color color)
    {
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0)
        {
            return;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments) * Mathf.Deg2Rad;

        // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
        // which are required by Unity's Mathf class trigonometry methods


        for (int i = 0; i < segments; i++)
        {
            Vector3 newPosition = transform.position + new Vector3(Mathf.Cos(angleStep * i), 0f, Mathf.Sin(angleStep * i)) * radius;
            m_segments.Append(Instantiate(m_segment, newPosition, Quaternion.LookRotation(transform.position - newPosition), transform));
        }
    }

    public void SetColor(Color color)
    {
        Renderer renderer = m_segment.GetOrAddComponent<Renderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        renderer.sharedMaterial.color = color;
        foreach (GameObject segment in m_segments) { segment.GetComponent<Renderer>().sharedMaterial = renderer.sharedMaterial; }
    }
    public void SetRadius(float radius)
    {
        m_radius = radius;
        ClearSegments();
        DrawCircle(transform.position, m_radius, m_segmentAmount, m_color);
    }
    void SetSegmentAmount(int amount)
    {
        m_segmentAmount = amount; ClearSegments();
        DrawCircle(transform.position, m_radius, m_segmentAmount, m_color);
    }
    void ClearSegments()
    {
        foreach (GameObject segment in m_segments) Destroy(segment);
        Array.Clear(m_segments, 0, m_segments.Length);
    }
}