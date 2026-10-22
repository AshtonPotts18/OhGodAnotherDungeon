using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    //References
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventoryDragController dragController;
    [SerializeField] private PickupController pickupController;
    private bool isOpen = false;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        UpdateCameraLock();
        UpdateCursorState();
    }

    private void Update()
    {
        //if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        //{
        //    if(!isOpen && pickupController != null && pickupController.HeldItem != null)
        //    {
        //        StoreHeldItem();
        //    }
        //    else
        //    {
        //        ToggleInventory();
        //    }
        //}

        if(Keyboard.current == null || !Keyboard.current.iKey.wasPressedThisFrame)
        {
            return;
        }
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if(playerObject != null)
        {
            pickupController = playerObject.GetComponentInChildren<PickupController>();
        }

        if(!isOpen && pickupController != null && pickupController.HeldItem != null)
        {
            StoreHeldItem();
        }

        else
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {

        isOpen = !isOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isOpen);
        }

        UpdateCameraLock();
        UpdateCursorState();

    }
    
    private void StoreHeldItem()
    {
        SellableItem worldItem = pickupController.HeldItem;
    
        if (worldItem == null || dragController == null) return;
    
        DisplayedInventoryItem prefab = worldItem.InventoryPrefab;
    
        if(prefab == null)
        {
            Debug.LogError("No prefab assinged.");
            return;
        }
    
        DisplayedInventoryItem uiItem = Instantiate(prefab);
    
        if(!dragController.PickupItem(uiItem))
        {
            Destroy(uiItem.gameObject);
            Debug.LogError("Item rejected");
            return;
        }
    
        pickupController.MoveHeldItem();
    
        isOpen = true;
    
        if(inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    
        UpdateCameraLock();
        UpdateCursorState();
    
        Debug.Log("Item fits");
    }

    private void UpdateCameraLock()
    {
        if (dragController != null)
        {
            dragController.SetInventoryOpen(isOpen);
        }
    }

    public void CloseInventory()
    {
        isOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        UpdateCameraLock();
        UpdateCursorState();

    }
    private void UpdateCursorState()
    {
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
