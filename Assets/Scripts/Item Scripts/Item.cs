using UnityEngine;

public class Item : MonoBehaviour
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


    private void Awake()
    {
        InventoryPrefab = GetComponent<InventoryItemDimensions>();
        rb = GetComponent<Rigidbody>();
        ItemName = itemName;
        Value = value;
    }

    public void SetHeld(bool isHeld)
    {
        if (rb != null)
        {
            rb.isKinematic = isHeld;
        }
    }
}