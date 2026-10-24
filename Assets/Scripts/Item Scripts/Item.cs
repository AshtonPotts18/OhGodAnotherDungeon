using UnityEngine;

public class Item : MonoBehaviour
{
    //References

    //fields
    [SerializeField] private string itemName = "item";
    [SerializeField] private int value = 100;
    private Rigidbody rb;

    //Getters
    //public InventoryItemDimensions InventoryPrefab => inventoryPrefab;
    public InventoryItemDimensions InventoryPrefab { get; private set; }
    public string ItemName { get; private set; } = "item";
    public int Value { get; private set; } = 100;

    Item()
    {
        ItemName = itemName;
        Value = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetHeld(bool isHeld)
    {
        if (rb != null)
        {
            rb.isKinematic = isHeld;
        }
    }
}