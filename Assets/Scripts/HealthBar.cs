using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider m_slider;
    public Image m_fill;
    public TMP_Text m_text;

    float m_Hp;
    float m_maxHp;

    public Color m_hpColor = Color.green;
    public Color m_bgColor = Color.black;

    Transform m_owner;
    Transform m_canvas;

    public Vector3 m_offset;

    void Start()
    {
        m_canvas = FindFirstObjectByType<Canvas>().transform;
        m_text = GetComponentInChildren<TMP_Text>();
        if (m_canvas != null) transform.SetParent(m_canvas);
        transform.LookAt(Camera.main.transform);
        if (m_owner != null) transform.position = m_owner.position + m_offset;
        UpdateHp(m_Hp, m_maxHp);
        UpdateHpColor(m_hpColor);
    }

    void Update()
    {
        if (m_owner == null) return;
        transform.position = m_owner.position + m_offset;
        transform.LookAt(Camera.main.transform);
    }
    public void SetOwner(Transform owner) { m_owner = owner; }
    public void UpdateHp(float hp, float maxHp)
    {
        m_Hp = hp;
        m_maxHp = maxHp;
        m_text.text = $"{m_Hp} / {m_maxHp}";
        if (hp >= m_maxHp) { m_slider.value = 100; return; }
        if (hp <= 0) { m_slider.value = 0; return; }
        m_slider.value = hp / maxHp;
    }

    public void UpdateHpColor(Color hpColor)
    {
        m_hpColor = hpColor;
        m_fill.color = hpColor;
    }
    public void UpdateBgColor(Color bgColor)
    {
    }

}
