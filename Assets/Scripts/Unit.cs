using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Unit : IIData
{
    public enum UnitType
    {
        None,
        Melee,
        Ranged,
        Hybrid
    }

    [System.Serializable]
    public struct UnitDrop
    {
        public Item m_item;
        public int m_amount;
    }

    public UnitType m_unitType = UnitType.None;


    [Header("Movement")]
    public float m_movementSpeed = 1f;
    public Cell m_cell;
    List<Cell> m_path;
    Vector3Int m_gridPosition { get { return new Vector3Int(Mathf.FloorToInt(transform.position.x), 0, Mathf.FloorToInt(transform.position.z)); } }
    Unit m_target;
    public bool m_stationary = false;


    [Header("Combat")]
    public int m_team = 0;
    float m_hp;
    float m_energy = 0f;
    float m_attackSpeed = 0f;
    public bool m_dead = false;
    public float m_attackRange = 2f;
    public float m_maxHp;
    public float m_maxEnergy = 0f;
    public float m_baseAttackSpeed = 0f;

    [Header("Attacks & Skills")]
    public Attack[] m_attacks = new Attack[] { };
    Attack m_currentAttack = null;
    int[] m_lastAttacks = new int[5];
    float m_attackCooldown = 0f;

    public Skill[] m_skills = new Skill[] { };
    Skill m_currentSkill = null;
    Skill[] m_lastSkills = new Skill[5];

    [Header("Drops")]
    public List<UnitDrop> m_drops;

    [Header("Sounds")]
    public AudioSource m_audioSource;
    public List<AudioClip> m_deathSounds = new();
    public List<AudioClip> m_attackSounds = new();

    [SerializeField] HealthBar m_healthBar;
    [SerializeField] AreaIndicator m_areaIndicator;
    bool m_attacking = false;

    void Start()
    {
        name = $"T{m_team} - {m_name}";
        m_hp = m_maxHp;
        m_areaIndicator = GetComponentInChildren<AreaIndicator>();
        if (m_areaIndicator != null)
        {
            m_areaIndicator.SetColor(Team.colors[m_team]);
            m_areaIndicator.SetRadius(m_attackRange);
        }
        if (m_healthBar != null)
        {
            m_healthBar.SetOwner(transform);
            m_healthBar.UpdateHp(m_hp, m_maxHp);
            m_healthBar.UpdateHpColor(Team.colors[m_team]);
        }
        //StartCoroutine(PeriodicDamage());
        m_target = FindTarget();
    }

    private void Update()
    {
        m_target = FindTarget();
        ProcessMovement();
        ProcessCooldowns();
    }

    private float Eerp(float a, float b, float t)
    {
        return a * Mathf.Exp(t * Mathf.Log(b / a));
    }
    private void PlayRandomizedPitchSfx(AudioClip clip)
    {
        float randomPitch = Eerp(0.7f, 1.1f, Random.value);
        m_audioSource.pitch = randomPitch;
        m_audioSource.PlayOneShot(clip);
    }

    void PlayAttackSound()
    {
        if (m_attackSounds.Count == 0) return;
        int randomIndex = Random.Range(0, m_attackSounds.Count);
        PlayRandomizedPitchSfx(m_attackSounds[randomIndex]);
    }

    void PlayDeathSound()
    {
        if (m_attackSounds.Count == 0) return;
        int randomIndex = Random.Range(0, m_deathSounds.Count);
        PlayRandomizedPitchSfx(m_deathSounds[randomIndex]);
    }
    IEnumerator PeriodicAttack()
    {
        float cooldown = 0f;
        while (true)
        {
            if (m_target != null)
            {
                cooldown = AttackTarget(m_target);
            }
            yield return new WaitForSeconds(cooldown);
        }
    }

    Unit FindTarget()
    {
        List<Unit> enemies = BattleManager.Instance.GetEnemyUnits(m_team);
        if (enemies.Count == 0) return null;
        return enemies.OrderBy(e => GetDistanceToUnit(e)).ToList().First();
    }

    void ProcessCooldowns()
    {
        if (m_attackCooldown == 0) return;
        m_attackCooldown -= Time.deltaTime;
        if (m_attackCooldown < 0) m_attackCooldown = 0;
    }

    void ProcessMovement()
    {
        if (m_target == null || m_stationary) return;
        if (GetDistanceToUnit(m_target) > m_attackRange)
        {
            if (m_attacking) StopAttacking();
            MoveAlongPath();
        }
        else if (!m_attacking && m_attackCooldown == 0) StartAttacking();
        else StopAttacking();

    }
    void StartAttacking()
    {
        StartCoroutine(PeriodicAttack());
    }
    void StopAttacking()
    {
        StopCoroutine(PeriodicAttack());
    }

    float GetDistanceToUnit(Unit target) => Vector3.Distance(transform.position, target.transform.position);
    Cell GetCurrentCell() => BattleGrid.Instance.TryGetCell(m_gridPosition);


    void UpdatePathToTarget(Unit target)
    {
        //Debug.Log($"{name} finding path from {m_gridPosition} to {target.m_gridPosition}");
        m_path = BattleGrid.Instance.m_pathFinder.FindPath(m_gridPosition, target.m_gridPosition);
        for (int i = 0; i < m_path.Count; i++)
        {
            if (i == m_path.Count - 1) return;
            Debug.DrawLine(m_path[i].GetWorldCenter() + new Vector3(0f, 1f, 0f), m_path[i + 1].GetWorldCenter() + new Vector3(0f, 1f, 0f), Team.colors[m_team], 10);
        }
    }

    void MoveAlongPath()
    {
        if (m_path == null) UpdatePathToTarget(m_target);
        if (m_path.Count == 0) return;
        transform.position = Vector3.MoveTowards(transform.position, m_path[0].GetWorldCenter(), m_movementSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, m_path[0].GetWorldCenter()) <= Mathf.Epsilon)
        {
            TeleportToCell(m_path[0]);
            m_path.RemoveAt(0);
            if (m_path.Count > 0) transform.LookAt(m_path[0].transform);
            UpdatePathToTarget(m_target);
        }
    }

    public void SetTeam(int team) { m_team = team; }

    float AttackTarget(Unit target)
    {
        if (m_target == null || m_attackCooldown > 0) return 0;
        Debug.Log($"{name} attacking {target.name}");
        transform.LookAt(target.transform);
        PlayAttackSound();
        m_attacking = true;
        target.Damage(m_attacks[0]);
        Attack m_randomAttack = m_attacks[Random.Range(0, m_attacks.Length)];
        m_attacking = false;
        m_attackCooldown = m_randomAttack.m_cooldown;
        return m_attackCooldown;
    }


    Skill[] GetAvailableSkills() { return null; }


    bool UseSkill(int skill) { return false; }


    void Damage(Attack attack)
    {
        Damage(attack.m_damage);
    }
    void Damage(float damage)
    {
        Debug.Log($"{name} takes {damage} damage. {m_hp}/{m_maxHp}");
        m_hp -= damage;
        WorldTextManager.Instance.ShowMessage($"{damage}", transform.position, Team.colors[m_team], FontStyles.Bold);
        if (m_hp <= 0) { m_hp = 0; Die(); }
        UpdateHealthBar();
    }

    void Die()
    {
        if (m_dead) return;
        m_dead = true;
        PlayDeathSound();
        StopAllCoroutines();
        if (m_team == 2) SpawnDrops();
        Destroy(m_healthBar.gameObject);
        BattleManager.Instance.KillUnit(this);
    }

    void SpawnDrops()
    {
        foreach (UnitDrop drop in m_drops)
            for (int i = 0; i < m_drops.Count; i++)
            {
                GameObject d = Instantiate(drop.m_item.gameObject, GetRandomPointInRadius(transform.position, 1), Quaternion.identity);
                d.GetComponent<Item>().m_team = m_team == 1 ? 2 : 1;
                d.name = drop.m_item.gameObject.name;
            }

    }

    Vector3 GetRandomPointInRadius(Vector3 origin, float radius)
    {
        Vector2 random = Random.insideUnitCircle;
        return origin + new Vector3(random.x * radius, 0.5f, random.y * radius);
    }

    void UpdateHealthBar()
    {
        m_healthBar.UpdateHp(m_hp, m_maxHp);
    }

    void TeleportToCell(Cell cell, float delay = 0f)
    {
        if (cell.IsBlocked()) return;
        StopAllCoroutines();
        StartCoroutine(StartTeleportToCell(cell, delay));
    }
    IEnumerator StartTeleportToCell(Cell cell, float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = cell.GetWorldCenter();
    }
    public override string ToString() => m_name;
}

#if UNITY_EDITOR
[CustomEditor(typeof(Unit))]
public class UnitEditor : IIDataEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif