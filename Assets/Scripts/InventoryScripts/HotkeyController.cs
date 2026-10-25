using UnityEngine;
using UnityEngine.InputSystem;

public class HotkeyController : MonoBehaviour
{
    //References
    [SerializeField] private PickupController pickupController;
    [SerializeField] private ItemPlacement inventory;
    [SerializeField] private InputActionReference dropAction;

    private InventoryItemDimensions equippedItem;
    private void Start()
    {
        if (pickupController == null)
        {
            pickupController = GetComponentInChildren<PickupController>();
        }

        inventory = ItemPlacement.Instance;

        if (inventory == null)
        {
            Debug.LogError("No active ItemPlacement registered in the scene.", this);
        }

        if (inventory == null)
        {
            Debug.LogError("HotkeyEquipmentController: ItemPlacement not found in the scene.", this);
        }
    }

    void Update()
    {
        HandleDropInput();
        HandleHotkeys();
    }

    private void EquipInventoryItem(InventoryItemDimensions item)
    {
        if (item == null || inventory == null || pickupController == null) return;

        if(item.worldPrefab == null)
        {
            Debug.LogWarning("No prefab assigned");
            return;
        }

        if (pickupController.HeldItem != null && equippedItem == null)
        {
            Debug.LogWarning("Drop current item before equipping");
            return;
        }

        InventoryItemDimensions previousItem = equippedItem;

        if (previousItem != null)
        {
            if (!inventory.TryReturnItemToGrid(previousItem))
            {
                Debug.LogWarning("Cannot swap items: there is no room in the inventory.");
                return;
            }

            pickupController.MoveHeldItem();
            equippedItem = null;
        }

        int oldX = item.gridX;
        int oldY = item.gridY;
        bool oldRotation = item.isRotated;

        inventory.RemoveItemFromGrid(item);
        item.gameObject.SetActive(false);

        if(pickupController.EquipItem(item.worldPrefab))
        {
            equippedItem = item;
            return;
        }

        item.SetGridPosition(oldX, oldY);
        item.SetRotation(oldRotation);

        if (!inventory.TryReturnItemToGrid(item))
        {
            Debug.LogError("Failed to equip item and could not rturn to inventory", item);
        }
    }

    private void UnequipToInventory()
    {
        if (equippedItem == null || pickupController == null || inventory == null) return;

        if(!inventory.TryReturnItemToGrid(equippedItem))
        {
            Debug.LogWarning("Cannot unequip: there is no room in the inventory.");
            return;
        }

        pickupController.MoveHeldItem();
        equippedItem = null;
    }

    private bool IsHotkeyPressed(InventoryItemDimensions item)
    {
        if(item == null || item.HotkeyAction == null || item.HotkeyAction.action == null)
        {
            return false;
        }
        return item.HotkeyAction.action.WasPressedThisFrame();
    }

    private void HandleDropInput()
    {
        if (dropAction == null || dropAction.action == null) return;

        if (!dropAction.action.WasPressedThisFrame()) return;

        if (pickupController == null || pickupController.HeldItem == null) return;

        if (equippedItem != null)
        {
            Destroy(equippedItem.gameObject);
            equippedItem = null;
        }

        pickupController.DropHeldItem();
    }
    private void HandleHotkeys()
    {
        if (equippedItem != null && IsHotkeyPressed(equippedItem))
        {
            UnequipToInventory();
            return;
        }

        InventoryItemDimensions[] items = FindObjectsByType<InventoryItemDimensions>(FindObjectsInactive.Include);

        foreach (InventoryItemDimensions item in items)
        {
            if (item == null || !item.isPlaced) continue;

            if (!IsHotkeyPressed(item)) continue;

            EquipInventoryItem(item);
            return;
        }
    }
}
