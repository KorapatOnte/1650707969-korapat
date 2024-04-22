using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    [SerializeField] private bool toBuild = false;
    [SerializeField] private bool showGhost = false;

    [SerializeField] private GameObject[] buildingList;
    public GameObject[] BuildingList { get { return buildingList; } }

    [SerializeField] private GameObject[] ghostBuildingList;

    [SerializeField] private GameObject newBuilding;
    public GameObject NewBuilding { get { return newBuilding; } set { newBuilding = value; } }

    [SerializeField] private GameObject ghostBuilding;
    public GameObject GhostBuilding { get { return ghostBuilding; } set { ghostBuilding = value; } }

    [SerializeField] private GameObject inProgressBuilding;
    public GameObject InProgressBuilding { get { return inProgressBuilding; } set { inProgressBuilding = value; } }

    private Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();
    }

    void Update()
    {
        if(unit.State == UnitState.Die)
        {
            return;
        }

        if(toBuild)
        {
            GhostBuildingFollowsMouse();

            if(Input.GetMouseButtonDown(0))
            {
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                CheckClickOnGround();
            }

            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CancelToBuild();
            }
        }

        switch (unit.State)
        {
            case UnitState.MoveToBuild:
                MoveToBuild(inProgressBuilding);
                break;
            case UnitState.BuildProgress:
                BuildProgress();
                break;
        }
    }

    public void ToCreateNewBuilding(int i)
    {
        if (buildingList[i] == null)
        {
            return;
        }

        Building b = buildingList[i].GetComponent<Building>();

        if(!unit.Faction.CheckBuildingCost(b))
        {
            return;
        }
        else
        {
            ghostBuilding = Instantiate(ghostBuildingList[i],
                                        Input.mousePosition,
                                        Quaternion.identity,
                                        unit.Faction.GhostBuildingParent);
            toBuild = true;
            newBuilding = buildingList[i];
            showGhost = true;
        }
    }

    private void GhostBuildingFollowsMouse()
    {
        if (showGhost)
        {
            Ray ray = CameraController.instance.Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Ground")))
            {
                if (ghostBuilding != null)
                {
                    ghostBuilding.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
                }
            }
        }
    }

    private void MoveToBuild(GameObject b)
    {
        if(b == null)
        {
            return;
        }

        unit.NavAgent.SetDestination(b.transform.position);
        unit.NavAgent.isStopped = false;
    }

    private void CancelToBuild()
    {
        toBuild = false;
        showGhost = false;

        newBuilding = null;
        Destroy(ghostBuilding);
        ghostBuilding = null;
        //Debug.Log("Cancel Building");
    }

    public void BuilderStartFixBuilding(GameObject target)
    {
        inProgressBuilding = target;
        unit.SetState(UnitState.MoveToBuild);
    }

    private void StartConstruction(GameObject buildingObj)
    {
        BuilderStartFixBuilding(buildingObj);
    }

    public void CreateBuildingSite(Vector3 pos) //Set a building site
    {
        if(ghostBuilding != null)
        {
            Destroy(ghostBuilding);
            ghostBuilding = null;
        }

        GameObject buildingObj = Instantiate(newBuilding,
                                             new Vector3(pos.x, newBuilding.transform.position.y, pos.z),
                                             Quaternion.identity);
        newBuilding = null;
        Building building = buildingObj.GetComponent<Building>();

        buildingObj.transform.position = new Vector3(buildingObj.transform.position.x,
                                                     buildingObj.transform.position.y - building.IntoTheGround,
                                                     buildingObj.transform.position.z);

        buildingObj.transform.parent = unit.Faction.BuildingsParent.transform;
        inProgressBuilding = buildingObj;

        unit.Faction.AliveBuildings.Add(building);
        building.Faction = unit.Faction;
        building.IsFunctional = false;
        building.CurHP = 1;
        unit.Faction.DeductBuildingCost(building);

        toBuild = false;
        showGhost = false;

        if(unit.Faction == GameManager.instance.MyFaction)
        {
            MainUI.instance.UpdateAllResource(unit.Faction);
        }

        StartConstruction(inProgressBuilding);
    }

    private void CheckClickOnGround()
    {
        Ray ray = CameraController.instance.Cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            bool canBuild = ghostBuilding.GetComponent<FindBuildingSite>().CanBuild;
            //Debug.Log(hit.collider.tag);
            if ((hit.collider.tag == "Ground") && canBuild)
            {
                //Debug.Log("Click Ground to Build");
                CreateBuildingSite(hit.point); //Create building site with 1 HP
            }
        }
    }
    private void BuildProgress()
    {
        if(inProgressBuilding == null)
        {
            return;
        }
        unit.LookAt(inProgressBuilding.transform.position);
        Building b = inProgressBuilding.GetComponent<Building>();

        b.Timer += Time.deltaTime;

        if (b.Timer >= b.WaitTime)
        {
            b.Timer = 0;
            b.CurHP++;

            if (b.IsFunctional == false)
            {
                inProgressBuilding.transform.position += new Vector3(0f, b.IntoTheGround / (b.MaxHP - 1), 0f);
            }

            if(b.CurHP >= b.MaxHP)
            {
                b.CurHP = b.MaxHP;
                b.IsFunctional = true;

                inProgressBuilding = null;
                unit.SetState(UnitState.Idle);

                unit.Faction.UpdateHousingLimit();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(unit.State == UnitState.Die)
        {
            return;
        }

        if(unit != null)
        {
            if(other.gameObject == inProgressBuilding)
            {
                unit.NavAgent.isStopped = true;
                unit.SetState(UnitState.BuildProgress);
            }
        }
    }

    private void OnDestroy()
    {
        if(ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }
    }
}
