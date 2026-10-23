using UnityEngine;

public class Item : MonoBehaviour
{
    //References
    [SerializeField] private InventoryItemDimensions inventoryPrefab;

    //fields
    [SerializeField] private string itemName = "item";
    [SerializeField] private int value = 100;
    private Rigidbody rb;

    //Getters
    public InventoryItemDimensions InventoryPrefab => inventoryPrefab;
    public string ItemName => itemName;
    public int Value => value;

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