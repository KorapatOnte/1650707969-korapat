using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
    [SerializeField] private ResourceSource curResourceSource;
    public ResourceSource CurResourceSource { get { return curResourceSource; } set { curResourceSource = value; } }

    [SerializeField] private float gatherRate = 0.5f;
    [SerializeField] private int gatherAmount = 1;

    [SerializeField] private int amountCarry;
    public int AmountCarry { get { return amountCarry; } set { amountCarry = value; } }

    [SerializeField] private int maxCarry = 25;
    public int MaxCarry { get { return maxCarry; } set { maxCarry = value; } }

    [SerializeField] private ResourceType carryType;
    public ResourceType CarryType { get { return carryType; } set { carryType = value; } }

    private float lastGatherTime;
    private Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();
    }

    void Update()
    {
        switch( unit.State)
        {
            case UnitState.MoveToResource:
                MoveToResourceUpdate();
                break;
            case UnitState.Gather:
                GatherUpdate();
                break;
            case UnitState.DeliverToHQ:
                DeliverToHQUpdate();
                break;
            case UnitState.StoreAtHQ:
                StoreAtHQUpdate();
                break;
        }
    }

    public void ToGatherResource(ResourceSource resource, Vector3 pos)
    {
        curResourceSource = resource;

        if(curResourceSource.RsrcType != carryType)
        {
            carryType = curResourceSource.RsrcType;
            amountCarry = 0;
        }

        unit.SetState(UnitState.MoveToResource);

        unit.NavAgent.isStopped = false;
        unit.NavAgent.SetDestination(pos);
            
    }

    private void MoveToResourceUpdate()
    {
        CheckForResource();

        if (Vector3.Distance(transform.position, unit.NavAgent.destination) <= 2f)
        {
            if (curResourceSource != null)
            {
                unit.LookAt(curResourceSource.transform.position);
                unit.NavAgent.isStopped = true;
                unit.SetState(UnitState.Gather);
            }
        }
    }

    private void GatherUpdate()
    {
        if (Time.time - lastGatherTime > gatherRate)
        {
            lastGatherTime = Time.time;

            if (amountCarry < maxCarry)
            {
                if (curResourceSource != null)
                {
                    curResourceSource.GatherResource(gatherAmount);

                    carryType = curResourceSource.RsrcType;
                    amountCarry += gatherAmount;
                }
                else
                    CheckForResource();
            }
            else //amount is full, go back to deliver at HQ
                unit.SetState(UnitState.DeliverToHQ);
        }
    }

    private void DeliverToHQUpdate()
    {
        if (Time.time - unit.LastPathUpdateTime > unit.PathUpdateRate)
        {
            unit.LastPathUpdateTime = Time.time;

            unit.NavAgent.SetDestination(unit.Faction.GetHQSpawnPos());
            unit.NavAgent.isStopped = false;
        }

        if (Vector3.Distance(transform.position, unit.Faction.GetHQSpawnPos()) <= 1f)
            unit.SetState(UnitState.StoreAtHQ);
    }
    private void StoreAtHQUpdate()
    {
        unit.LookAt(unit.Faction.GetHQSpawnPos());

        if (amountCarry > 0)
        {
            // Deliver the resource to Faction
            unit.Faction.GainResource(carryType, amountCarry);
            amountCarry = 0;

            //Debug.Log("Delivered");
        }
        CheckForResource();
    }
    private void CheckForResource()
    {
        if (curResourceSource != null) //that resource still exists
            ToGatherResource(curResourceSource, curResourceSource.transform.position);
        else
        {
            //try to find a new resource
            curResourceSource = unit.Faction.GetClosestResource(transform.position, carryType);

            //CheckAgain, if found a new one, go to it
            if (curResourceSource != null)
                ToGatherResource(curResourceSource, curResourceSource.transform.position);
            else //can't find a new one
            {
                Debug.Log($"{unit.name} can't find a new tree");
                unit.SetState(UnitState.Idle);
            }
        }
    }

}
