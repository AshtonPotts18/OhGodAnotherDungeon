using UnityEngine;

public class Item : MonoBehaviour, IInteract
{
    //References

    //fields
    [SerializeField] private string itemName = "item";
    [SerializeField] private int value = 100;
    [SerializeField] private InventoryItemDimensions inventoryPrefab;
    private Rigidbody rb;

    //Getters
    public InventoryItemDimensions InventoryPrefab { get; set; }
    public string ItemName { get; set; } = "item";
    public int Value { get; set; } = 100;

    public Item()
    {}

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ItemName = itemName;
        Value = value;
        InventoryPrefab = inventoryPrefab;
    }

    public virtual void SetHeld(bool isHeld)
    {
        if (rb != null)
        {
            rb.isKinematic = isHeld;
        }
    }
    public virtual void UseItem()
    {
        Debug.Log("Not a useable item");
    }
}