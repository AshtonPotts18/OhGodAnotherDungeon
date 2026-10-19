using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class PickupController : MonoBehaviour
{

    //references
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdHand;
    [SerializeField] private TextMeshProUGUI pickupPrompt;
    //pickup settings
    [SerializeField] private float range = 2.5f;

    private SellableItem heldItem;
    private SellableItem targetedItem;

    public SellableItem HeldItem => heldItem;

    // Update is called once per frame
    void Update()
    {
        PromptDisplay();

        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldItem == null)
            {
                Pickup();
            }
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (heldItem != null)
            {
                Drop();
            }
        }
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

    private void Drop()
    {
        if (heldItem == null) return;

        heldItem.transform.SetParent(null);
        heldItem.SetHeld(false);

        heldItem = null;
    }

    private void PromptDisplay()
    {

        targetedItem = null;

        if (heldItem == null && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                targetedItem = hit.collider.GetComponentInParent<SellableItem>();
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

        SellableItem itemToMove = heldItem;

        heldItem = null;
        targetedItem = null;

        Destroy(itemToMove.gameObject);
    }
}
