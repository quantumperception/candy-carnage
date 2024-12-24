using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBase : MonoBehaviour
{
    public int m_team;
    public int m_maxHp;
    public int m_hp;
    public HealthBar m_healthBar;
    // Start is called before the first frame update
    void Start()
    {
        //m_healthBar = GetComponentInChildren<HealthBar>();
        //m_healthBar.SetOwner(transform);
        //m_healthBar.UpdateHp(m_hp, m_maxHp);
        //m_healthBar.UpdateHpColor(Team.colors[m_team]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
