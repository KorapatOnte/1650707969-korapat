using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum UnitState
{
    Idle,
    Move,
    AttackUnit,
    MoveToBuild,
    BuildProgress,
    MoveToResource,
    Gather,
    DeliverToHQ,
    StoreAtHQ,
    MoveToEnemy,
    MoveToEnemyBuilding,
    AttackBuilding,
    Die
}

[System.Serializable]
public struct UnitCost
{
    public int food;
    public int wood;
    public int gold;
    public int stone;
}


public class Unit : MonoBehaviour
{
    [SerializeField] private int id;
    public int ID { get { return id; } set { id = value; } }

    [SerializeField] private string unitName;
    public string UnitName { get { return unitName; } }

    [SerializeField] private Sprite unitPic;
    public Sprite UnitPic { get { return unitPic; } }

    [SerializeField] private int curHP;
    public int CurHP { get { return curHP; } set { curHP = value; } }

    [SerializeField] private int maxHP = 100;
    public int MaxHP { get { return maxHP; } }

    [SerializeField] private int moveSpeed = 5;
    public int MoveSpeed { get { return moveSpeed; } }

    [SerializeField] private int minWpnDamage;
    public int MinWpnDamage { get { return minWpnDamage; } }

    [SerializeField] private int maxWpnDamage;
    public int MaxWpnDamage { get { return maxWpnDamage; } }

    [SerializeField] private int armour;
    public int Armour { get { return armour; } }

    [SerializeField] private float visualRange;
    public float VisualRange { get { return visualRange; } }

    [SerializeField] private float weaponRange;
    public float WeaponRange { get { return weaponRange; } }

    [SerializeField] private UnitState state;
    public UnitState State { get { return state; } set { state = value; } }

    private NavMeshAgent navAgent;
    public NavMeshAgent NavAgent { get { return navAgent; } }

    [SerializeField] private Faction faction;
    public Faction Faction { get { return faction; } set { faction = value; } }

    [SerializeField] private GameObject selectionVisual;
    public GameObject SelectionVisual { get { return selectionVisual; } }

    [SerializeField] private UnitCost unitCost;
    public UnitCost UnitCost { get { return unitCost; } }

    //time for increasing progress 1% for this unit, less is faster
    [SerializeField] private float unitWaitTime = 0.1f;
    public float UnitWaitTime { get { return unitWaitTime; } }

    [SerializeField] private bool isBuilder;
    public bool IsBuilder { get { return isBuilder; } set { isBuilder = value; } }

    [SerializeField] private Builder builder;
    public Builder Builder { get { return builder; } }

    [SerializeField] private bool isWorker;
    public bool IsWorker { get { return isWorker; } set { isWorker = value; } }

    [SerializeField] private Worker worker;
    public Worker Worker { get { return worker; } }

    [SerializeField]
    private float pathUpdateRate = 1.0f;
    public float PathUpdateRate { get { return pathUpdateRate; } }

    [SerializeField]
    private float lastPathUpdateTime;
    public float LastPathUpdateTime { get { return lastPathUpdateTime; } set { lastPathUpdateTime = value; } }

    [SerializeField]
    private Unit curEnemyUnitTarget;

    [SerializeField]
    private Building curEnemyBuildingTarget;

    [SerializeField]
    private float attackRate = 1f; //how frequent this unit attacks in second

    [SerializeField]
    private float lastAttackTime;

    [SerializeField] private float defendRange = 30f; //the range that a unit will defensively auto-attack
    public float DefendRange { get { return defendRange; } }


    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();

        if(isBuilder)
        {
            builder = GetComponent<Builder>();
        }

        if(isWorker)
        {
            worker = GetComponent<Worker>();
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        switch (state)
        {
            case UnitState.Move:
                MoveUpdate();
                break;
            case UnitState.MoveToEnemy:
                MoveToEnemyUpdate();
                break;
            case UnitState.AttackUnit:
                AttackUpdate();
                break;
            case UnitState.MoveToEnemyBuilding:
                MoveToEnemyBuildingUpdate();
                break;
            case UnitState.AttackBuilding:
                AttackBuildingUpdate();
                break;
        }
    }

    public void ToggleSelectionVisual(bool flag)
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(flag);
    }

    public void SetState(UnitState toState)
    {
        state = toState;

        if (state == UnitState.Idle)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }

    public void MoveToPosition(Vector3 dest)
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(dest);
            navAgent.isStopped = false;
        }

        SetState(UnitState.Move);
    }

    public void LookAt(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void MoveUpdate()
    {
        float distance = Vector3.Distance(transform.position, navAgent.destination);

        if (distance <= 1f)
            SetState(UnitState.Idle);
    }
    protected virtual IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
    protected virtual void Die()
    {
        navAgent.isStopped = true;

        SetState(UnitState.Die);

        if (faction != null)
            faction.AliveUnits.Remove(this);

        InfoManager.instance.ClearAllInfo();
        //Debug.Log(gameObject + " dies.");
        StartCoroutine("DestroyObject");
    }
    public void ToAttackUnit(Unit target)
    {
        if (curHP <= 0 || state == UnitState.Die)
            return;
        curEnemyUnitTarget = target;
        SetState(UnitState.MoveToEnemy);
    }
    public void TakeDamage(Unit enemy, int damage)
    {
        //I'm already dead
        if (curHP <= 0 || state == UnitState.Die)
            return;

        curHP -= damage;

        if (curHP <= 0)
        {
            curHP = 0;
            Die();
        }

        if (!IsWorker) //if this unit is not worker
            ToAttackUnit(enemy); //always counter-attack
    }
    public void MoveToEnemyUpdate()
    {
        // if our target is null, go idle
        if (curEnemyUnitTarget == null)
        {
            SetState(UnitState.Idle);
            return;
        }
        if (Time.time - lastPathUpdateTime > pathUpdateRate)
        {
            lastPathUpdateTime = Time.time;
            navAgent.isStopped = false;

            if (curEnemyUnitTarget != null)
                navAgent.SetDestination(curEnemyUnitTarget.transform.position);
        }

        if (Vector3.Distance(transform.position, curEnemyUnitTarget.transform.position) <= WeaponRange)
            SetState(UnitState.AttackUnit);
    }
    protected void AttackUpdate()
    {
        // if our target is dead, go idle
        if (curEnemyUnitTarget == null || curEnemyUnitTarget.CurHP <= 0)
        {
            //DisableAllWeapons();
            SetState(UnitState.Idle);
            return;
        }

        // if we're still moving, stop
        if (!navAgent.isStopped)
            navAgent.isStopped = true;

        // look at the enemy
        LookAt(curEnemyUnitTarget.transform.position);

        // attack every 'attackRate' seconds
        if (Time.time - lastAttackTime > attackRate)
        {
            lastAttackTime = Time.time;
            curEnemyUnitTarget.TakeDamage(this, UnityEngine.Random.Range(minWpnDamage, maxWpnDamage + 1));
        }

        // if we're too far away, move towards the enemy
        if (Vector3.Distance(transform.position, curEnemyUnitTarget.transform.position) > weaponRange)
        {
            SetState(UnitState.MoveToEnemy);
            //Debug.Log($"{unitName} - From Attack Update");
        }
    }
    // move to an enemy building and attack them
    public void ToAttackBuilding(Building target)
    {
        curEnemyBuildingTarget = target;
        SetState(UnitState.MoveToEnemyBuilding);
    }

    private void MoveToEnemyBuildingUpdate()
    {
        if (curEnemyBuildingTarget == null)
        {
            SetState(UnitState.Idle);
            return;
        }

        if (Time.time - lastPathUpdateTime > pathUpdateRate)
        {
            lastPathUpdateTime = Time.time;
            navAgent.isStopped = false;
            navAgent.SetDestination(curEnemyBuildingTarget.transform.position);
        }

        if ((Vector3.Distance(transform.position, curEnemyBuildingTarget.transform.position) - 4f) <= WeaponRange)
        {
            SetState(UnitState.AttackBuilding);
        }
    }
    private void AttackBuildingUpdate()
    {
        // if our target is dead, go idle
        if (curEnemyBuildingTarget == null)
        {
            SetState(UnitState.Idle);
            return;
        }

        // if we're still moving, stop
        if (!navAgent.isStopped)
        {
            navAgent.isStopped = true;
        }

        // look at the enemy
        LookAt(curEnemyBuildingTarget.transform.position);

        // attack every 'attackRate' seconds
        if (Time.time - lastAttackTime > attackRate)
        {
            lastAttackTime = Time.time;

            curEnemyBuildingTarget.TakeDamage(UnityEngine.Random.Range(minWpnDamage, maxWpnDamage + 1));
        }
        if ((Vector3.Distance(transform.position, curEnemyBuildingTarget.transform.position) - 4f) > WeaponRange)
        {
            SetState(UnitState.MoveToEnemyBuilding);
        }
    }
    public void ToAttackTurret(Turret turret)
    {
        if (curHP <= 0 || state == UnitState.Die)
            return;
        curEnemyBuildingTarget = turret;
        SetState(UnitState.MoveToEnemyBuilding);
    }

    // called when an enemy turret attacks us
    public void TakeDamage(Turret turret, int damage)
    {
        //I'm already dead
        if (curHP <= 0 || state == UnitState.Die)
            return;

        curHP -= damage;

        if (curHP <= 0)
        {
            curHP = 0;
            Die();
        }

        if (!IsWorker) //if this unit is not worker
            ToAttackTurret(turret); //counter-attack at turret
    }
}
