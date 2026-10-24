using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class PickupController : MonoBehaviour
{

    //references
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdHand;
    [SerializeField] private TextMeshProUGUI pickupPrompt;
    [SerializeField] private InputActionReference pickup;
    [SerializeField] private InputActionReference drop;
    //pickup settings
    [SerializeField] private float range = 2.5f;

    //field
    private Item heldItem;
    private Item targetedItem;
    //Getter
    public Item HeldItem => heldItem;

    // Update is called once per frame
    private void OnEnable()
    {
        if (pickup == null || pickup.action == null)
        {
            Debug.LogError("pickup not assigned");
            return;
        }

        if (drop == null || drop.action == null)
        {
            Debug.LogError("drop not assigned");
            return;
        }
        pickup.action.performed += OnPickup;
        drop.action.performed += OnDrop;
    }
    private void OnDisable()
    {
        if (pickup != null && pickup.action != null)
        {
            pickup.action.performed -= OnPickup;
        }
        if (drop != null && drop.action != null)
        {
            drop.action.performed -= OnDrop;
        }
    }
    private void Start()
    {
        TextMeshProUGUI[] texts =
        FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "PickupPrompt")
            {
                pickupPrompt = text;
                Debug.Log("Pickup prompt assigned successfully!");
                break;
            }
        }

        if (pickupPrompt == null)
        {
            Debug.LogError("PickupPrompt was not found!");
        }
    }
    private void Update()
    {
        PromptDisplay();
    }
    
    private void OnPickup(InputAction.CallbackContext context)
    {
        if (heldItem == null)
        {
            Pickup();
        }
    }
    private void OnDrop(InputAction.CallbackContext context)
    {
        if (heldItem != null)
        {
            Drop();
        }
    }
    private void Drop()
    {
        if (heldItem == null) return;

        heldItem.transform.SetParent(null);
        heldItem.SetHeld(false);

        heldItem = null;
    }
    private void Pickup()
    {
        if (targetedItem == null || holdHand == null) return;

        heldItem = targetedItem;

        heldItem.SetHeld(true);

        heldItem.transform.SetParent(holdHand);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }
    private void PromptDisplay()
    {

        targetedItem = null;

        if (heldItem == null && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                targetedItem = hit.collider.GetComponentInParent<Item>();
            }
        }

        if (pickupPrompt == null)
            return;

        if (heldItem != null)
        {
            pickupPrompt.gameObject.SetActive(true);
            pickupPrompt.text = "Q - Drop " + heldItem.ItemName;
        }
        else if (targetedItem != null)
        {
            pickupPrompt.gameObject.SetActive(true);
            pickupPrompt.text = "E - Pick Up " + targetedItem.ItemName + " | Value: " + targetedItem.Value;
        }
        else
        {
            pickupPrompt.gameObject.SetActive(false);
        }
    }

    public void MoveHeldItem()
    {
        if (heldItem == null) return;

        Item itemToMove = heldItem;

        heldItem = null;
        targetedItem = null;

        Destroy(itemToMove.gameObject);
    }
}
