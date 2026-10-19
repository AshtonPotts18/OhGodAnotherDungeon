using UnityEngine;

public class SellableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "item";
    [SerializeField, Min(0)] private int value = 100;

    private Rigidbody rb;

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