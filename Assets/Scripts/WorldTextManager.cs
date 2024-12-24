using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldTextManager : MonoBehaviour
{
    public static WorldTextManager Instance { get; private set; }
    enum MessageType
    {
        World,
        Damage,
        Chat
    }
    Transform m_canvas;
    List<TextMeshProUGUI> m_texts = new List<TextMeshProUGUI>();
    List<float> m_textSpawnTimes = new List<float>();
    public Vector3 m_offset = new Vector3(0f, 1f, 0f);
    GameObject m_prefab;
    public float m_riseSpeed = 1f;
    public SerializableDictionary<string, AnimationCurve> m_animationCurves = new SerializableDictionary<string, AnimationCurve>();

    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    void Start()
    {
        CreatePrefab();
        Instance.m_canvas = FindFirstObjectByType<Canvas>().transform;
    }

    void Update()
    {
        for (int i = 0; i < Instance.m_texts.Count; i++)
        {
            TextMeshProUGUI tmp = Instance.m_texts[i];
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, m_animationCurves["fadeOut"].Evaluate(Time.time - m_textSpawnTimes[i]));
            tmp.transform.position = Vector3.MoveTowards(tmp.transform.position, tmp.transform.position + new Vector3(0f, m_riseSpeed, 0f), Time.deltaTime);
            tmp.transform.LookAt(Camera.main.transform);
            tmp.transform.Rotate(0f, 180f, 0f);
        }
    }

    void CreatePrefab()
    {
        GameObject go = new GameObject("WorldText");
        go.transform.LookAt(Camera.main.transform);
        go.transform.Rotate(0f, 180f, 0f);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 0.25f;
        tmp.autoSizeTextContainer = true;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.maxVisibleLines = 1;
        tmp.alignment = TextAlignmentOptions.Center;
        Instance.m_prefab = go;
    }

    public void ShowMessage(string message, Vector3 position, Color color, FontStyles fontStyle = FontStyles.Normal, float duration = 5f)
    {
        if (Instance.m_canvas == null) return;
        GameObject go = Instantiate(m_prefab, position + m_offset, Quaternion.identity);
        go.transform.LookAt(Camera.main.transform);
        go.transform.Rotate(0f, 180f, 0f);
        go.transform.SetParent(m_canvas);
        go.name = $"Text: {message}";
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.color = color;
        tmp.text = message;
        tmp.fontStyle = fontStyle;
        float timestamp = Time.time;
        Instance.m_texts.Add(tmp);
        Instance.m_textSpawnTimes.Add(timestamp);
        StartCoroutine(DestroyMessage(tmp, timestamp, duration));
    }

    IEnumerator DestroyMessage(TextMeshProUGUI tmp, float timestamp, float duration)
    {
        yield return new WaitForSeconds(duration);
        Instance.m_texts.Remove(tmp);
        Instance.m_textSpawnTimes.Remove(timestamp);
        Destroy(tmp.gameObject);
    }
}
