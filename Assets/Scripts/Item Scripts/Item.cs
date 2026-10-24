using UnityEngine;

public class Item : MonoBehaviour, IInteract
{
    //References

    //fields
    [SerializeField] private string itemName = "item";
    [SerializeField] private int value = 100;
    private Rigidbody rb;

    //Getters
    public InventoryItemDimensions InventoryPrefab { get; private set; }
    public string ItemName { get; private set; } = "item";
    public int Value { get; private set; } = 100;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ItemName = itemName;
        Value = value;
    }

    public virtual void SetHeld(bool isHeld)
    {
        if (rb != null)
        {
            rb.isKinematic = isHeld;
        }
    }
}