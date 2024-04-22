using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ResourceType
{
    Food,
    Wood,
    Gold,
    Stone
}

public class ResourceSource : MonoBehaviour
{
    [SerializeField] private string rsrcName;
    public string RsrcName { get { return rsrcName; } }

    [SerializeField] private Sprite rsrcPic;
    public Sprite RsrcPic { get { return rsrcPic; } }

    [SerializeField] private ResourceType rsrcType;
    public ResourceType RsrcType { get { return rsrcType; } }

    [SerializeField] private int quantity;
    public int Quantity { get { return quantity; } set { quantity = value; } }

    [SerializeField] private int maxQuantity;
    public int MaxQuantity { get { return maxQuantity; } }

    [SerializeField] private GameObject selectionVisual;
    public GameObject SelectionVisual { get { return selectionVisual; } }

    [SerializeField] private UnityEvent onRsrcQuantityChange;
    [SerializeField] private UnityEvent onInfoQuantityChange;
    
    void Start()
    {
        onRsrcQuantityChange.Invoke();

        //onInfoQuantityChange.AddListener( delegate { InfoManager.instance.ShowAllInfo(this);}};
        //onInfoQuantityChange.AddListener(() => InfoManager.instance.ShowAllInfo(this));
    }

    void Update()
    {
        if(quantity <= 0)
        {
            InfoManager.instance.ClearAllInfo();
            Destroy(gameObject);
        }
    }

    public void GatherResource(int amountRequest)
    {
        int amountToGive;

        if(amountRequest > quantity)
        {
            amountToGive = quantity;
        }
        else
        {
            amountToGive = amountRequest;
        }

        quantity -= amountToGive;

        onRsrcQuantityChange.Invoke();

        if(quantity <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ToggleSelectionVisual(bool selected)
    {
        if(SelectionVisual != null)
        {
            SelectionVisual.SetActive(selected);
        }
    }
}
