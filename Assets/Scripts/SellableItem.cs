using UnityEngine;

public class SellableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "item";
    [SerializeField, Min(0)] private int value = 100;
    [SerializeField] private Item inventoryPrefab;

    private Rigidbody rb;

    public string ItemName => itemName;
    public int Value => value;
    public Item InventoryPrefab => inventoryPrefab;

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
