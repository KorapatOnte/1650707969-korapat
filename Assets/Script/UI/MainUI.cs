using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainUI : MonoBehaviour
{
    [SerializeField] private GameObject selectionMarker;
    public GameObject SelectionMarker { get { return selectionMarker; } }

    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private RectTransform selectionBox;
    public RectTransform SelectionBox { get { return selectionBox; } }

    private Canvas canvas;
    public Canvas Canvas { get { return canvas; } }

    public static MainUI instance;

    private void Awake()
    {
        instance = this;
        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UpdateAllResource(Faction faction)
    {
        unitCountText.text = $"{faction.AliveUnits.Count}/{faction.UnitLimit}";
        foodText.text = faction.Food.ToString();
        woodText.text = faction.Wood.ToString();
        goldText.text = faction.Gold.ToString();
        stoneText.text = faction.Stone.ToString();
    }
    public Vector3 ScalePosition(Vector3 pos)
    {
        Vector3 newPos;

        newPos = new Vector3(pos.x * canvas.transform.localScale.x
                             , pos.y * canvas.transform.localScale.y
                             , pos.z * canvas.transform.localScale.z);

        return newPos;
    }
}
