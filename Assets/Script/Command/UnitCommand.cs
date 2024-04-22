using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCommand : MonoBehaviour
{
    public LayerMask layerMask;
    private UnitSelect unitSelect;

    private Camera cam;

    void Awake()
    {
        unitSelect = GetComponent<UnitSelect>();
    }

    void Start()
    {
        cam = Camera.main;

        layerMask = LayerMask.GetMask("Unit", "Building", "Resource", "Ground");
    }


    void Update()
    {
        // mouse up
        if (Input.GetMouseButtonUp(1))
        {
            TryCommand(Input.mousePosition);
        }
    }

    private void UnitsMoveToPosition(Vector3 dest, List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (u != null) 
            {
                u.MoveToPosition(dest);
            }
        }
    }

    private void CommandToGround(RaycastHit hit, List<Unit> units)
    {
        UnitsMoveToPosition(hit.point, units);
        CreateVFXMarker(hit.point, MainUI.instance.SelectionMarker);
    }

    private void TryCommand(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;

        //if we left-click something
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            switch (hit.collider.tag)
            {
                case "Ground":
                    CommandToGround(hit, unitSelect.CurUnits);
                    break;
                case "Resource":
                    ResourceCommand(hit, unitSelect.CurUnits);
                    break;
                case "Unit":
                    CommandToUnit(hit, unitSelect.CurUnits);
                    break;
                case "Building":
                    BuildingCommand(hit, unitSelect.CurUnits);
                    break;
            }
        }
    }

    private void CreateVFXMarker(Vector3 pos, GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
            return;

        Instantiate(vfxPrefab, new Vector3(pos.x, 0.1f, pos.z), Quaternion.identity);
    }

    private void UnitsToGatherResource(ResourceSource resource, List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (u.IsWorker)
            {
                u.Worker.ToGatherResource(resource, resource.transform.position);
            }
            else
            {
                u.MoveToPosition(resource.transform.position);
            }
        }
    }
    private void ResourceCommand(RaycastHit hit, List<Unit> units)
    {
        UnitsToGatherResource(hit.collider.GetComponent<ResourceSource>(), units);
        CreateVFXMarker(hit.transform.position, MainUI.instance.SelectionMarker);
    }
    private void UnitAttackEnemy(Unit enemy, List<Unit> units)
    {
        foreach (Unit u in units)
        {
            u.ToAttackUnit(enemy);
        }
    }
    private void CommandToUnit(RaycastHit hit, List<Unit> units)
    {
        Unit target = hit.collider.gameObject.GetComponent<Unit>();

        if (target == null)
            return;

        if (target.Faction == GameManager.instance.EnemyFaction)// if it is our enemy
            UnitAttackEnemy(target, units);
    }
    private void UnitAttackEnemyBuilding(Building enemyBuilding, List<Unit> units)
    {
        foreach (Unit u in units)
        {
            u.ToAttackBuilding(enemyBuilding);
        }
    }
    private void BuildingCommand(RaycastHit hit, List<Unit> units)
    {
        Building building = hit.collider.gameObject.GetComponent<Building>();

        if (building == null)
            return;

        // if it is an enemy's building
        if (building.Faction == GameManager.instance.EnemyFaction)
            UnitAttackEnemyBuilding(building, units);
        else
        {
            if (building.CurHP < building.MaxHP)
            {
                HelpFixBuilding(hit.collider.gameObject, units);
                StartCoroutine(Formula.BlinkSelection(building.SelectionVisual));
            }
        }
    }
    
    private void HelpFixBuilding(GameObject target, List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (u.IsBuilder)
                u.Builder.BuilderStartFixBuilding(target);
        }
    }

}
