using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
public class HotkeyController : MonoBehaviour
{
    //References
    [SerializeField] private PickupController pickupController;
    [SerializeField] private ItemPlacement inventory;
    [SerializeField] private InputActionReference dropAction;

    //Hotkey Assignment
    [SerializeField] private InputActionReference hotkey1;
    [SerializeField] private InputActionReference hotkey2;
    [SerializeField] private InputActionReference hotkey3;
    [SerializeField] private TMP_Text hotkeyPrompt;
    private InventoryItemDimensions equippedItem;
    private InventoryItemDimensions hoveredItem;
    public TMP_Text HotkeyPrompt => hotkeyPrompt;

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

    private void OnEnable()
    {
        if (hotkey1 != null) hotkey1.action.Enable();
        if (hotkey2 != null) hotkey2.action.Enable();
        if (hotkey3 != null) hotkey3.action.Enable();
    }

    private void OnDisable()
    {
        if (hotkey1 != null) hotkey1.action.Disable();
        if (hotkey2 != null) hotkey2.action.Disable();
        if (hotkey3 != null) hotkey3.action.Disable();
    }

    void Update()
    {
        if (hoveredItem != null)
        {
            Debug.Log("Currently hovering: " + hoveredItem.name);
        }

        if (hotkey1 != null && hotkey1.action.WasPressedThisFrame())
            Debug.Log("KEY 1 DETECTED");

        if (hotkey2 != null && hotkey2.action.WasPressedThisFrame())
            Debug.Log("KEY 2 DETECTED");

        if (hotkey3 != null && hotkey3.action.WasPressedThisFrame())
            Debug.Log("KEY 3 DETECTED");

        HandleHotKeyAssignment();

        if (hoveredItem != null && (hotkey1.action.WasPressedThisFrame() || hotkey2.action.WasPressedThisFrame() || hotkey3.action.WasPressedThisFrame())) return;

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

    public void SetHoveredItem(InventoryItemDimensions item)
    {
        hoveredItem = item;

        if (hotkeyPrompt == null) return;

        if (item.originalItem == null)
        {
            hotkeyPrompt.gameObject.SetActive(false);
            return;
        }

        if (item.originalItem is UsableItem)
        {
            hotkeyPrompt.gameObject.SetActive(true);

            if (item.HotkeyAction != null && item.HotkeyAction.action != null)
            {
                hotkeyPrompt.text = "Assigned: " + item.HotkeyAction.action.name;
            }
            else
            {
                hotkeyPrompt.text = "Assign a hotkey: 1, 2, or 3";
            }
        }
        else
        {
            hotkeyPrompt.gameObject.SetActive(false);
        }
    }

    public void ClearHoveredItem(InventoryItemDimensions item)
    {
        if (hoveredItem != item) return;

        hoveredItem = null;

        if(hotkeyPrompt != null)
        {
            hotkeyPrompt.gameObject.SetActive(false);
        }
    }

    private void HandleHotKeyAssignment()
    {
        //if (hoveredItem == null || Keyboard.current == null) return;
        //
        //if(!(hoveredItem.originalItem is UsableItem)) return;
        //
        //if(hotkey1.action.WasPressedThisFrame())
        //{
        //    AssignHotkey(hoveredItem, hotkey1);
        //}
        //else if (hotkey2.action.WasPressedThisFrame())
        //{
        //    AssignHotkey(hoveredItem, hotkey2);
        //}
        //else if (hotkey3.action.WasPressedThisFrame())
        //{
        //    AssignHotkey(hoveredItem, hotkey3);
        //}
        if (hoveredItem == null)
            return;

        if (hoveredItem.originalItem == null)
        {
            Debug.LogWarning("Hovered item has no originalItem!");
            return;
        }

        if (hoveredItem.originalItem is not UsableItem)
            return;

        if (hotkey1 == null || hotkey2 == null || hotkey3 == null)
        {
            Debug.LogError("One or more hotkey references are missing!");
            return;
        }

        if (hotkey1.action.WasPressedThisFrame())
        {
            Debug.Log("Hotkey 1 pressed");
            AssignHotkey(hoveredItem, hotkey1);
        }
        else if (hotkey2.action.WasPressedThisFrame())
        {
            Debug.Log("Hotkey 2 pressed");
            AssignHotkey(hoveredItem, hotkey2);
        }
        else if (hotkey3.action.WasPressedThisFrame())
        {
            Debug.Log("Hotkey 3 pressed");
            AssignHotkey(hoveredItem, hotkey3);
        }
    }

    private void AssignHotkey(InventoryItemDimensions item, InputActionReference action)
    {
        //if (action == null || action.action == null) return;
        //
        //InventoryItemDimensions[] items = FindObjectsByType<InventoryItemDimensions>(FindObjectsInactive.Include);
        //
        //foreach(InventoryItemDimensions existingItem in items)
        //{
        //    if(existingItem == item) continue;
        //    
        //    if(existingItem.HotkeyAction  != null && existingItem.HotkeyAction.action == action.action)
        //    {
        //        if(hotkeyPrompt != null)
        //        {
        //            hotkeyPrompt.text = "That key is already assigned!";
        //            return;
        //        }
        //    }
        //}
        //
        //item.SetHotkeyAction(action);
        //
        //if(hotkeyPrompt != null)
        //{
        //    hotkeyPrompt.text = "Assigned: " + action.action.name;
        //}
        //
        //Debug.Log(item.originalItem.ItemName + " assigned to " + action.action.name);
        if (item == null || action == null || action.action == null) return;

        InventoryItemDimensions[] items =
            FindObjectsByType<InventoryItemDimensions>(FindObjectsInactive.Include);

        foreach (InventoryItemDimensions existingItem in items)
        {
            if (existingItem == item) continue;

            if (existingItem.HotkeyAction != null &&
                existingItem.HotkeyAction.action == action.action)
            {
                if (hotkeyPrompt != null)
                    hotkeyPrompt.text = "That key is already assigned!";

                return;
            }
        }

        item.SetHotkeyAction(action);

        Debug.Log("Assigned " + action.action.name + " to " + item.name);

        if (hotkeyPrompt != null)
            hotkeyPrompt.text = "Assigned: " + action.action.name;
    }
}
