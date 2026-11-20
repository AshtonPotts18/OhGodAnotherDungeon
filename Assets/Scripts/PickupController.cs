using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class PickupController : MonoBehaviour
{

    //references
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdHand;
    [SerializeField] private TextMeshProUGUI pickupPrompt;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference drop;
    //pickup settings
    [SerializeField] private float range = 2.5f;

    //field
    private Item heldItem;
    private Item targetedItem;

    //Getter
    public Item HeldItem => heldItem;
    public event Action<Item> ItemPickedUp;


    // Update is called once per frame
    private void Start()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "PickupPrompt")
            {
                pickupPrompt = text;
                break;
            }
        }

        
    }
    private void Update()
    {
        PromptDisplay();
    }

    private void Pickup()
    {
        if (targetedItem == null || holdHand == null || targetedItem.isShopItem) return;

        heldItem = targetedItem;

        heldItem.SetHeld(true);

        heldItem.transform.SetParent(holdHand);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.Euler(heldItem.pickupRotationOffset);

        ItemPickedUp?.Invoke(heldItem);
    }

    private void Drop()
    {
        if (heldItem == null) return;

        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;

        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1.5f))
        {
            dropPosition = hit.point - playerCamera.transform.forward * 0.3f;
        }

        GameObject itemManager = GameObject.Find("ItemManager");
        heldItem.transform.SetParent(itemManager.transform);
        heldItem.SetHeld(false);
        heldItem.transform.position = dropPosition;

        heldItem = null;
    }

    private void OnEnable()
    {
        if (interactAction == null || interactAction.action == null)
        {
            return;
        }

        if (drop == null || drop.action == null)
        {
            return;
        }
        interactAction.action.performed += OnPickup;
            drop.action.performed += OnDrop;
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnPickup;
        }
        if (drop != null && drop.action != null)
        {
            drop.action.performed -= OnDrop;
        }
    }

    private void OnPickup(InputAction.CallbackContext context)
    {
        if(heldItem == null)
        {
            Pickup();
        }
    }
    private void OnDrop(InputAction.CallbackContext context)
    {
        if(heldItem != null)
        {
            Drop();
        }
    }

    private void PromptDisplay()
    {

        targetedItem = null;

        if (heldItem == null && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Item item = hit.collider.GetComponentInParent<Item>();

                if(item != null && !item.isShopItem)
                {
                    targetedItem = item;
                }
            }
        }

        if (pickupPrompt == null) return;

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

        itemToMove.gameObject.SetActive(false);
        itemToMove.transform.SetParent(null);
    }

    public bool EquipItem(GameObject prefab)
    {
        Debug.Log("EquipItem was called!");
        if (prefab == null || holdHand == null || heldItem != null)
        {
            return false;
        }

        Item item = prefab.GetComponent<Item>();

        if (item == null)
        {
            return false;
        }
        
        item.gameObject.SetActive(true);

        heldItem = item;
        heldItem.SetHeld(true);

        heldItem.transform.SetParent(holdHand);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.Euler(heldItem.pickupRotationOffset);

        targetedItem = null;

        return true;
    }

    public void DropHeldItem()
    {
        Drop();
    }

    
}
