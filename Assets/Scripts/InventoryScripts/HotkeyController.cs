using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

        HandleHotKeyAssignment();

        if (hoveredItem != null && (hotkey1.action.WasPressedThisFrame() || hotkey2.action.WasPressedThisFrame() || hotkey3.action.WasPressedThisFrame())) return;

        HandleDropInput();
        HandleHotkeys();
    }

    private void EquipInventoryItem(InventoryItemDimensions item)
    {
        if (item == null || inventory == null || pickupController == null) return;

        if(item.worldPrefab == null) return;

        if (pickupController.HeldItem != null && equippedItem == null) return;

        InventoryItemDimensions previousItem = equippedItem;

        if (previousItem != null)
        {
            if (!inventory.TryReturnItemToGrid(previousItem)) return;

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

    }

    private void UnequipToInventory()
    {
        if (equippedItem == null || pickupController == null || inventory == null) return;

        if(!inventory.TryReturnItemToGrid(equippedItem)) return;

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

            if (item.originalItem == null) continue;

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
        if (hoveredItem == null) return;

        if (hoveredItem.originalItem == null) return;

        if (!(hoveredItem.originalItem is UsableItem)) return;

        if (hotkey1 == null || hotkey2 == null || hotkey3 == null) return;

        if (hotkey1.action.WasPressedThisFrame())
        {
            AssignHotkey(hoveredItem, hotkey1);
        }
        else if (hotkey2.action.WasPressedThisFrame())
        {
            AssignHotkey(hoveredItem, hotkey2);
        }
        else if (hotkey3.action.WasPressedThisFrame())
        {
            AssignHotkey(hoveredItem, hotkey3);
        }
    }

    private void AssignHotkey(InventoryItemDimensions item, InputActionReference action)
    {
        if (item == null || action == null || action.action == null) return;

        InventoryItemDimensions[] items = FindObjectsByType<InventoryItemDimensions>(FindObjectsInactive.Include);

        foreach (InventoryItemDimensions existingItem in items)
        {
            if (existingItem == item) continue;

            if (existingItem.HotkeyAction != null && existingItem.HotkeyAction.action == action.action)
            {
                if (hotkeyPrompt != null)
                {
                    hotkeyPrompt.text = "That key is already assigned!";
                }

                return;
            }
        }

        item.SetHotkeyAction(action);

        if (hotkeyPrompt != null)
        {
            hotkeyPrompt.text = "Assigned: " + action.action.name;
        }
    }
}
