using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimation : MonoBehaviour
{
    private Animator anim;
    private Unit unit;

    void Start()
    {
        anim = GetComponent<Animator>();
        unit = GetComponent<Unit>();
    }

    void Update()
    {
        ChooseAnimation(unit);
    }

    private void ChooseAnimation(Unit u)
    {
        anim.SetBool("IsIdle", false);
        anim.SetBool("IsMove", false);
        anim.SetBool("IsAttack", false);
        anim.SetBool("IsBuild", false);
        anim.SetBool("IsMoveToBuild", false);
        anim.SetBool("IsMoveToResource", false);
        anim.SetBool("IsGather", false);
        anim.SetBool("IsDeliver", false);
        anim.SetBool("IsStore", false);
        anim.SetBool("IsDie", false);

        switch (u.State)
        {
            case UnitState.Idle:
                anim.SetBool("IsIdle", true);
                break;
            case UnitState.Move:
                anim.SetBool("IsMove", true);
                break;
            case UnitState.AttackUnit:
                anim.SetBool("IsAttack", true);
                break;
            case UnitState.BuildProgress:
                anim.SetBool("IsBuild", true);
                break;
            case UnitState.MoveToBuild:
                anim.SetBool("IsMoveToBuild",true);
                break;
            case UnitState.MoveToResource:
                anim.SetBool("IsMoveToResource", true);
                break;
            case UnitState.Gather:
                anim.SetBool("IsGather", true);
                break;
            case UnitState.DeliverToHQ:
                anim.SetBool("IsDeliver", true);
                break;
            case UnitState.StoreAtHQ:
                anim.SetBool("IsStore", true);
                break;
            case UnitState.Die:
                anim.SetBool("IsDie", true);
                break;
        }
    }
}
